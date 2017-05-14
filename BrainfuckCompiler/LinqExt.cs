using System;
using System.Collections.Generic;

namespace BrainfuckCompiler
{
    public static class LinqExt
    {
        public static T Peek<T>(this IList<T> source)
        {
            if (source.Count == 0)
            {
                throw new InvalidOperationException();
            }

            return source[source.Count - 1];
        }

        public static T Pop<T>(this IList<T> source)
        {
            if (source.Count == 0)
            {
                throw new InvalidOperationException();
            }

            var item = source[source.Count - 1];
            source.RemoveAt(source.Count - 1);
            return item;
        }

        public static IEnumerable<T> SubList<T>(this IEnumerable<T> source, int start, int length)
        {
            var enumerator = source.GetEnumerator();

            for (int i = 0; i < start; i++)
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
            }

            for (int i = 0; i < length && enumerator.MoveNext(); i++)
            {
                yield return enumerator.Current;
            }
        }

        public static IEnumerable<T> FailIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, Exception> exception)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    throw exception(item);
                }

                yield return item;
            }
        }

        public static void EnqueAll<T>(this Queue<T> source, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                source.Enqueue(item);
            }
        }
    }
}
