using System.Collections.Generic;

namespace BrainfuckCompilerTests
{
    internal static class StringExt
    {
        public static string[] SplitEmpty(this string source, char seperator)
            => source == string.Empty ? new string[0] : source.Split(seperator);

        public static string Join<T>(this IEnumerable<T> source, string seperator)
            => string.Join(seperator, source);
    }
}
