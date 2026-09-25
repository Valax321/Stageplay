using System.Text;

namespace Radish.ContentBuilder.Utility;

internal static class StringParseExtensions
{
    extension(string line)
    {
        public IReadOnlyList<string> TokenizeWithStringHandling()
        {
            var inQuotes = false;
            var escaped = false;
            var currentToken = new StringBuilder();
            var tokens = new List<string>();

            for (var i = 0; i < line.Length; ++i)
            {
                var chr = line[i];
            
                // Handle escape sequences
                if (escaped)
                {
                    switch (chr)
                    {
                        case 'n':
                            currentToken.Append('\n');
                            break;
                        default:
                            currentToken.Append(chr);
                            break;
                    }
                
                    escaped = false;
                    continue;
                }

                if (chr == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
            
                if (inQuotes && chr == '\\') {
                    escaped = true;
                    continue;
                }
            
                if (inQuotes) {
                    currentToken.Append(chr);
                    continue;
                }

                if (char.IsWhiteSpace(chr))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                    }

                    currentToken.Clear();
                    continue;
                }

                currentToken.Append(chr);
            }
        
            if (currentToken.Length > 0)
                tokens.Add(currentToken.ToString());

            return tokens;
        }
    }
}