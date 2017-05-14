using System.Collections.Generic;
using System.IO;
using BrainfuckCompiler.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompilerTests.Compiler
{
    [TestClass]
    public class CharReaderTest
    {
        [TestMethod]
        public void CharReader_CursorPositionAfterRead()
        {
            var data = new Dictionary<string, CursorPosition>
            {
                [string.Empty] = Pos(1, 1),
                ["a"] = Pos(1, 2),

                // single new line
                ["\r"] = Pos(2, 1),
                ["\n"] = Pos(2, 1),
                ["\r\n"] = Pos(2, 1),

                // double new line
                ["\r\r"] = Pos(3, 1),
                ["\n\n"] = Pos(3, 1),
                ["\n\r"] = Pos(3, 1),

                // three new line characters
                ["\r\r\r"] = Pos(4, 1),
                ["\r\r\n"] = Pos(3, 1),
                ["\r\n\r"] = Pos(3, 1),
                ["\r\n\n"] = Pos(3, 1),
                ["\n\r\r"] = Pos(4, 1),
                ["\n\r\n"] = Pos(3, 1),
                ["\n\n\r"] = Pos(4, 1),
                ["\n\n\n"] = Pos(4, 1),
            };

            foreach (var item in data)
            {
                TestCursorPosition(item.Key, item.Value);
                TestCursorPosition(item.Key + "a", new CursorPosition()
                {
                    Column = item.Value.Column + 1,
                    Row = item.Value.Row
                });
            }
        }

        private static CursorPosition Pos(int row, int col) => new CursorPosition() { Row = row, Column = col };

        private static void TestCursorPosition(string text, CursorPosition position)
        {
            var reader = new CharReader(new StringReader(text));
            for (int i = 0; i < text.Length; i++)
            {
                reader.MoveNext();
            }

            string msg = $"'{text.Replace("\r", "\\r").Replace("\n", "\\n")}': current ";
            Assert.AreEqual(position.Row, reader.CurrentRow, msg + "row");
            Assert.AreEqual(position.Column, reader.CurrentColumn, msg + "column");
            Assert.AreEqual(text.Length, reader.CurrentIndex, msg + "index");
        }
    }
}
