using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainfuckCompiler
{
    public struct Equal
    {
        public static Equal New = true;
        public bool Value;

        public Equal(bool value)
            : this()
        {
            this.Value = value;
        }

        public static implicit operator bool(Equal eq) => eq.Value;

        public static implicit operator Equal(bool bl) => new Equal(bl);

        public Equal Compare<T>(T thisValue, T otherValue)
            => this.Value && object.Equals(thisValue, otherValue);

        public Equal Compare<T>(T thisValue, T otherValue, IEqualityComparer<T> comparer)
            => this.Value && comparer.Equals(thisValue, otherValue);

        public Equal Compare<T>(T thisValue, T otherValue, Func<T, T, bool> comparer)
            => this.Value && comparer(thisValue, otherValue);

        public Equal CompareEnumerable<T>(IEnumerable<T> thisValue, IEnumerable<T> otherValue)
            => this.Value && thisValue.SequenceEqual(otherValue);

        public Equal CompareEnumerable<T>(IEnumerable<T> thisValue, IEnumerable<T> otherValue, IEqualityComparer<T> comparer)
            => this.Value && thisValue.SequenceEqual(otherValue, comparer);

        public Equal CompareEnumerable<T>(IEnumerable<T> thisValue, IEnumerable<T> otherValue, Func<T, T, bool> comparer)
            => this.Value && thisValue.SequenceEqual(otherValue, new FuncEqualityComparer<T>(comparer));

        private class FuncEqualityComparer<T> : IEqualityComparer<T>
        {
            public FuncEqualityComparer(Func<T, T, bool> comparer)
            {
                this.Comparer = comparer;
            }

            public Func<T, T, bool> Comparer { get; }

            public bool Equals(T x, T y) => this.Comparer(x, y);

            public int GetHashCode(T obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
