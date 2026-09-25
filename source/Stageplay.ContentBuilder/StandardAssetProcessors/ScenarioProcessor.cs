using System.Diagnostics;
using JetBrains.Annotations;
using Radish.ContentBuilder.AssetProcessors;
using Radish.ContentBuilder.Scenario;
using Radish.Resources;
using Radish.Scenario;
using Radish.Scenario.Commands;
using Radish.Serialization;

namespace Radish.ContentBuilder.StandardAssetProcessors;

/// <summary>
/// Asset processor that converts .scenario source files into scenario binaries.
/// It will also generate a source map file.
/// </summary>
[PublicAPI]
public sealed class ScenarioProcessor : AssetProcessor
{
    /// <summary>
    /// A list of sources for runtime commands, used to validate the script instructions at compile-time.
    /// </summary>
    public required IReadOnlyList<IReadOnlyDictionary<string, ICommand>> CommandSources { get; init; }
    
    /// <inheritdoc/>
    public override async Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input)
    {
        var commands = new Dictionary<string, ICommand>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var src in CommandSources)
        {
            foreach (var (name, cmd) in src)
            {
                if (!commands.TryAdd(name, cmd))
                    throw new Exception($"Attempted to register multiple commands named \"{name}\"");
            }
        }
        
        var scenarioDirectory = input.ContentFile.Directory 
                                ?? throw new InvalidOperationException("Somehow, the content file has no directory");

        var result = await ParseScenarioDirectives(input, scenarioDirectory);
        var stringTable = new WritableStringTable();
        var globals = new WritableGlobals(stringTable);
        var writer = new BytecodeWriter(stringTable);
        
        foreach (var script in result.Scripts)
        {
            await ParseScriptFile(script, scenarioDirectory, writer, commands, globals);
        }
        
        var entrypointStringIndex = stringTable.GetUniqueStringIndex(result.Entrypoint);
        var runtimeGlobals = globals.BuildRuntimeGlobals();
        var runtimeBytecode = writer.Compile(entrypointStringIndex);
        
        // This must be called last!
        var runtimeStringTable = stringTable.BuildRuntimeStringTable();

        var compiledScript =
            new CompiledScenario(runtimeGlobals, runtimeBytecode, runtimeStringTable);

        var destScenario = MakeOutputFileFromInput(input, ".bscn");
        var destSourceMap = MakeOutputFileFromInput(input, ".lno");
        
        {
            await using var destScenarioFile = destScenario.OpenWrite();
            destScenarioFile.SetLength(0);
            await BinaryObject.ToStreamAsync(compiledScript, destScenarioFile);
        }
        
        return new AssetProcessorResult([destScenario.FullName]);
    }

    private static async Task<(string Entrypoint, List<FileInfo> Scripts)> ParseScenarioDirectives(AssetProcessorInput input, DirectoryInfo scenarioDirectory)
    {
        string? entrypointLabel = null;
        var scriptFiles = new List<FileInfo>();
        
        using var scenarioReader = input.ContentFile.OpenText();
        var lineIndex = 0;
        while (await scenarioReader.ReadLineAsync() is { } line)
        {
            lineIndex++;
            
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            // Skip comments
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("//"))
                continue;

            var tokens = trimmedLine.TokenizeWithStringHandling();
            Debug.Assert(tokens.Count > 0);

            var directive = tokens[0];
            switch (directive)
            {
                case "entrypoint":
                    ValidateDirectiveTokenCount(tokens, 2, input.ContentFilePath, lineIndex);
                    entrypointLabel = tokens[1];
                    break;
                case "include":
                    ValidateDirectiveTokenCount(tokens, 2, input.ContentFilePath, lineIndex);
                    scriptFiles.Add(new FileInfo(Path.Combine(scenarioDirectory.FullName, tokens[1])));
                    break;
                default:
                    throw new ScenarioParseException("DIR0002", input.ContentFilePath, lineIndex,
                        $"unknown scenario directive \"{directive}\"");
            }
        }

        if (entrypointLabel is null)
            throw new ScenarioParseException("DIR0003", input.ContentFilePath, null,
                "an entrypoint directive must be provided");

        return (Entrypoint: entrypointLabel, Scripts: scriptFiles);
    }

    private static void ValidateDirectiveTokenCount(IReadOnlyList<string> tokens, int requiredCount, string file, int line)
    {
        if (tokens.Count < requiredCount)
            throw new ScenarioParseException("DIR0001", file, line, "too few arguments in scenario directive");
    }
    
    private static async Task ParseScriptFile(FileInfo file, DirectoryInfo scenarioDirectory, BytecodeWriter writer, 
        IReadOnlyDictionary<string, ICommand> commands, WritableGlobals globals)
    {
        var scriptPath = Path.GetRelativePath(scenarioDirectory.FullName, file.FullName)
            .Replace('\\', '/'); // Without this the source map paths will differ between building on windows vs unix
        
        // Exceptions + IDisposable sucks.
        StreamReader? reader;
        try
        {
            reader = file.OpenText();
        }
        catch (Exception)
        {
            throw new ScenarioParseException("SCR9999", scriptPath, null, "could not open file for reading");
        }
        
        try
        {
            var lineIndex = 0;
            while (await reader.ReadLineAsync() is { } line)
            {
                ParseScriptLine(writer, commands, globals, ref lineIndex, line, scriptPath);
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static void ParseScriptLine(BytecodeWriter writer, IReadOnlyDictionary<string, ICommand> commands, WritableGlobals globals,
        ref int lineIndex, string line, string scriptPath)
    {
        lineIndex++;

        // Skip empty lines
        if (string.IsNullOrWhiteSpace(line))
            return;

        // Skip comments
        var trimmedLine = line.TrimStart();
        if (trimmedLine.StartsWith("//"))
            return;

        var tokens = trimmedLine.TokenizeWithStringHandling();
        Debug.Assert(tokens.Count > 0);

        if (tokens[0].StartsWith('$'))
        {
            var globalName = tokens[0][1..];
            if (!int.TryParse(tokens[1], out var defVal))
                throw new ScenarioParseException("SCR0001", scriptPath, lineIndex,
                    "global variable value not a valid integer");

            if (!globals.Add(globalName, defVal))
                throw new ScenarioParseException("SCR0002", scriptPath, lineIndex,
                    $"duplicate global named \"{globalName}\"");
        }
        else if (tokens[0].StartsWith(':'))
        {
            var labelName = tokens[0];
            writer.WriteLabel(labelName);
        }
        else
        {
            var commandName = tokens[0];
            var args = new WritableTokenStreamImpl(tokens, scriptPath, lineIndex);
            if (!commands.TryGetValue(commandName, out var cmd))
                throw new ScenarioParseException("SCR0003", scriptPath, lineIndex,
                    $"unknown command \"{commandName}\"");
            
            try
            {
                var data = cmd.Parse(new CommandParseContext(args, (scriptPath, lineIndex)));
                
                // No-op
                if (!data.HasValue)
                    return;

                if (data.Value.Arguments.Count > byte.MaxValue)
                    throw new ScenarioParseException("SCR0008", scriptPath, lineIndex,
                        "too many arguments in command (max 255)");


                writer.WriteCommandPacket(new CommandPacket(data.Value.Name, (scriptPath, lineIndex),
                    [.. MarshalArguments(data.Value, (scriptPath, lineIndex))]));
            }
            catch (CommandParseException ex)
            {
                throw new ScenarioParseException($"SCR{ex.ErrorCode:0000}", scriptPath, lineIndex, ex.Message);
            }
        }
    }

    private static IEnumerable<BytecodeCommandValue> MarshalArguments(CommandData command, (string, int) source)
    {
        foreach (var arg in command.Arguments)
        {
            yield return arg switch
            {
                byte b => new BytecodeByte(b),
                short s => new BytecodeShort(s),
                int i => new BytecodeInteger(i),
                float f => new BytecodeFloat(f),
                bool b => new BytecodeBoolean(b),
                string s => new BytecodeString(s),
                _ => throw new ScenarioParseException("SCR0004", source.Item1, source.Item2, $"bad command argument of type {arg.GetType().FullName}")
            };
        }
    }
}