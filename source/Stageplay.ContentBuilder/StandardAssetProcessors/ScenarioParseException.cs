using System.Text;

namespace Radish.ContentBuilder.StandardAssetProcessors;

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