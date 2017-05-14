using System;
using System.Linq;

namespace BrainfuckCompilerTests.CodeGenerator
{
    internal class CacheBuilder
    {
        public object[] Stack { get; set; }

        public object[] Heap { get; set; }

        public int CacheSize { get; set; }

        public int CachePointer => this.Stack?.Sum(item => this.StackSizeOf(item)) ?? 0;

        public byte[] Build()
        {
            var stack = this.Normalize(this.Stack);
            var heap = this.Normalize(this.Heap);

            var cache = new byte[this.CacheSize];
            cache[cache.Length - 1] = 255;

            // handle stack
            var cp = this.StackSizeOf(0);
            foreach (var item in stack)
            {
                if (item is byte)
                {
                    cache[cp] = (byte)item;
                }
                else
                {
                    throw new NotImplementedException();
                }

                cp += this.StackSizeOf(item);
            }

            // handle heap
            var hp = cache.Length - 2;
            foreach (var item in heap)
            {
                if (item is byte)
                {
                    cache[hp] = (byte)item;
                }
                else
                {
                    throw new NotImplementedException();
                }

                hp -= this.HeapSizeOf(item);
            }

            return cache;
        }

        private int HeapSizeOf(object obj)
        {
            if (obj is byte || obj is int)
            {
                return 1;
            }

            throw new NotImplementedException();
        }

        private int StackSizeOf(object obj)
        {
            if (obj is byte || obj is int)
            {
                return 2;
            }

            throw new NotImplementedException();
        }

        private object[] Normalize(object[] items)
        {
            items = items ?? new object[0];
            for (int i = 0; i < items.Length; i++)
            {
                // TODO handle strings
                if (items[i] is char)
                {
                    items[i] = (byte)(char)items[i];
                }
                else if (items[i] is int)
                {
                    items[i] = (byte)(int)items[i];
                }
                else
                {
                    items[i] = (byte)items[i];
                }
            }

            return items;
        }
    }
}
