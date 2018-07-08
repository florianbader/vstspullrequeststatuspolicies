using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static string ToKebabCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return Regex.Replace(
                str,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }
}