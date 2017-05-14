using System.Collections.Generic;
using System.Linq;

namespace BrainfuckCompiler
{
    public struct Hash
    {
        public static readonly Hash New = 1;

        public int Value;

        private const int Prime = 31;

        public Hash(int value)
            : this()
        {
            this.Value = value;
        }

        public static implicit operator int(Hash hash) => hash.Value;

        public static implicit operator Hash(int val) => new Hash(val);

        public Hash Add(int hash)
            => this.Value * Prime + hash;

        public Hash Add<T>(T value)
            => this.Add(value == null ? 0 : value.GetHashCode());

        public Hash Add<T>(ref T value)
            where T : struct
            => this.Add(value.GetHashCode());

        public Hash AddEnumerable<T>(IEnumerable<T> source)
            => source.Aggregate(this.Value, (value, next) => value * Prime + next.GetHashCode());
    }
}
