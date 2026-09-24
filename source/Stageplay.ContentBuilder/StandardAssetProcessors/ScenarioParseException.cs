using System.Text;

namespace Radish.ContentBuilder.StandardAssetProcessors;

/// <summary>
/// Exception thrown for a scenario compiler error.
/// </summary>
/// <param name="errorCode">The error code in XYYY format.</param>
/// <param name="file">The path to the file where the error occurred.</param>
/// <param name="line">The line in the file where the error occurred.</param>
/// <param name="message">The description of what the error is.</param>
public sealed class ScenarioParseException(string errorCode, string file, int? line, string message) : Exception
{
    /// <inheritdoc/>
    public override string Message
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append(file);
            if (line.HasValue)
                sb.Append($":{line}");

            sb.Append(" - ");
            sb.Append(errorCode);
            sb.Append(" - ");
            sb.Append(message);

            return sb.ToString();
        }
    }
}