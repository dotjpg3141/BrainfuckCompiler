using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler
{
    public class Tree
    {
        public Tree(string value, params Tree[] children)
        {
            this.Value = value;
            this.Children = children;
        }

        public string Value { get; set; }

        public Tree[] Children { get; set; }

        public static implicit operator Tree(string value) => new Tree(value);

        public override string ToString()
        {
            var sb = new StringBuilder();
            this.ToString(sb, string.Empty);
            return sb.ToString();
        }

        public static void AssertEqual(Tree expected, Tree actual, Func<string, string, bool> comparator, string msg)
        {
            if (expected == null)
            {
                Assert.IsNull(actual, msg);
                return;
            }

            Assert.IsNotNull(actual, msg);
            Assert.IsTrue(comparator(expected.Value, actual.Value), msg);
            Assert.AreEqual(expected.Children.Length, actual.Children.Length, msg);
            for (int i = 0; i < expected.Children.Length; i++)
            {
                AssertEqual(expected.Children[i], actual.Children[i], comparator, msg);
            }
        }

        private void ToString(StringBuilder sb, string indent)
        {
            sb.Append(indent).Append("[").Append(this.Value).Append("]");
            if (this.Children.Length > 0)
            {
                sb.Append(" = {\n");
                foreach (var item in this.Children)
                {
                    if (item == null)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        item.ToString(sb, indent + "\t");
                    }

                    sb.Append(", \n");
                }

                sb.Append(indent).Append("}");
            }
        }
    }
}
