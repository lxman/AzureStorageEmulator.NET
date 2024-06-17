using System.Text;

namespace AzureStorageEmulator.NET.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveLinearWhitespaceNotQuoted(this string input)
        {
            List<(int start, int end)> quotedSections = [];
            int quoteStart = FindFirstQuote(input);
            while (quoteStart > -1)
            {
                int quoteEnd = input.IndexOf(input[quoteStart], quoteStart + 1);
                quotedSections.Add((quoteStart, quoteEnd));
                quoteStart = FindFirstQuote(input[(quoteEnd + 1)..]);
            }

            if (quotedSections.Count == 0)
            {
                return RemoveWhitespace(input);
            }

            StringBuilder builder = new();
            int currentQuote = 0;
            int previousEnd = 0;
            while (currentQuote < quotedSections.Count)
            {
                (int start, int end) = quotedSections[currentQuote++];
                builder.Append(RemoveWhitespace(input[previousEnd..start]));
                previousEnd = end + 1;
                builder.Append(input[start..previousEnd]);
            }

            if (quotedSections.Last().end < input.Length - 1)
            {
                builder.Append(RemoveWhitespace(input[(quotedSections.Last().end + 1)..]));
            }

            return builder.ToString();
        }

        private static int FindFirstQuote(string s)
        {
            int doubleQuoteStart = s.IndexOf('"');
            int singleQuoteStart = s.IndexOf('\'');
            return doubleQuoteStart == -1 && singleQuoteStart == -1
                ? -1
                : doubleQuoteStart > -1 && singleQuoteStart != -1 && doubleQuoteStart < singleQuoteStart
                    ? doubleQuoteStart
                    : singleQuoteStart > -1
                        ? singleQuoteStart
                        : -1;
        }

        private static string RemoveWhitespace(string s)
        {
            return s.Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Replace("  ", " ");
        }
    }
}