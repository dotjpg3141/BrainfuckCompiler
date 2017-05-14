using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BrainfuckCompiler.Brainfuck;
using BrainfuckCompilerTests;
using BrainfuckCompilerTests.CodeGenerator;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrainfuckCompiler.Compiler.CodeGenerator.Tests
{
    [TestClass]
    public class CodeWriterTests : TestBase
    {
        private const string CsvData = @"data\insns.csv";
        private const bool DebugEnabled = true;
        private static readonly Regex InsnRegex = new Regex(
            @"\s*(?<name>[a-zA-Z]+)\s*\[\s*(?<args>([-+]?\d+(\s*,\s*\d+)*)?)\s*\]\s*");

        [TestMethod]
        public void WriteTest()
        {
            using (var textReader = File.OpenText(CsvData))
            {
                var csvReader = new CsvReader(textReader, this.CsvConfig);
                string[] record = null;

                while (csvReader.Read())
                {
                    this.UpdateRecord(ref record, csvReader.CurrentRecord);

                    TestWithTimeout(() =>
                    {
                        int cacheSize = ParseInt("cache size", record[3]);
                        int? debugNop = string.IsNullOrEmpty(record[2]) ? (int?)null
                            : ParseInt("debug nop id", record[2]);

                        this.WriteTest(
                             record[0],
                             ParseInstructions(record[1]),
                             record[4],
                             new CacheBuilder()
                             {
                                 Stack = ParseData(record[5]),
                                 Heap = ParseData(record[6]),
                                 CacheSize = cacheSize,
                             },
                             record[7],
                             new CacheBuilder()
                             {
                                 Stack = ParseData(record[8]),
                                 Heap = ParseData(record[9]),
                                 CacheSize = cacheSize,
                             },
                             debugNop);
                    });
                    Trace.WriteLine(new string('-', 200));
                }
            }
        }

        private void UpdateRecord(ref string[] record, string[] currentRecord)
        {
            if (record == null)
            {
                record = new string[currentRecord.Length];
            }

            for (int i = 0; i < currentRecord.Length; i++)
            {
                if (currentRecord[i] != "\"")
                {
                    record[i] = currentRecord[i];
                }
            }
        }

        private void WriteTest(string name, Instruction[] instructions, string input, CacheBuilder inputCache, string output, CacheBuilder outputCache, int? nopBreakpointId)
        {
            Trace.WriteLine(name);

            // generate sourcecode
            var cw = new CodeWriter(new StringWriter());
            cw.EmitDebugInfo = DebugEnabled;
            cw.EmitDebugBreakpoint = DebugEnabled && nopBreakpointId == null;

            // cw.Begin(); // cache initialized by cache builder
            foreach (var insn in instructions)
            {
                if (insn.Prototype == InstructionPrototypes.Nop &&
                    insn.Get(0) == nopBreakpointId)
                {
                    cw.Write(new Instruction(InstructionPrototypes.Breakpoint));
                }

                cw.Write(insn);
            }

            cw.End();

            Trace.WriteLine(cw.Writer);
            this.DumpInfo("Input   ", input, inputCache.Build(), inputCache.CachePointer);
            this.DumpInfo("Expected", output, outputCache.Build(), outputCache.CachePointer);

            // run code
            var intptr = new BrainfuckInterpreter(cw.Writer.ToString(), inputCache.Build());
            intptr.Input = new StringReader(input);
            intptr.Output = new StringWriter();
            intptr.CachePointer = inputCache.CachePointer;

            if (nopBreakpointId != null)
            {
                intptr.NextBreakpoint();
                Assert.IsFalse(intptr.Terminated, "terminated");
            }
            else
            {
                intptr.Run();
            }

            this.DumpInfo("Actual  ", intptr.Output.ToString(), intptr.Cache, intptr.CachePointer);

            // asserts
            var prefix = $"\nTestcase: {name}\n";
            outputCache.CacheSize = inputCache.CacheSize;
            CollectionAssert.AreEqual(outputCache.Build(), intptr.Cache, prefix + "cache");
            Assert.AreEqual(outputCache.CachePointer, intptr.CachePointer, prefix + "cache pointer");
            Assert.AreEqual(output, intptr.Output.ToString(), prefix + "output");
        }

        private void DumpInfo(string prefix, string text, byte[] cache, int position)
        {
            Trace.WriteLine($@"{prefix}: [{
                    cache.Select(c => c.ToString().PadLeft(3)).Join(", ")}] at {
                    position.ToString().PadLeft(cache.Length.ToString().Length)} text '{text}'");
        }

        private static object[] ParseData(string text)
        {
            var result = new List<object>();
            foreach (var item in text.SplitEmpty(',').Select(item => item.Trim()))
            {
                if (item.StartsWith("\""))
                {
                    result.Add(item.Substring(1, item.Length - 2));
                }
                else if (item.StartsWith("'"))
                {
                    result.Add((byte)item[1]);
                }
                else
                {
                    result.Add(byte.Parse(item));
                }
            }

            return result.ToArray();
        }

        private static Instruction[] ParseInstructions(string text)
        {
            return text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(strInsn => new { Match = InsnRegex.Match(strInsn), Text = strInsn })
                .FailIf(item => !item.Match.Success, item => new FormatException("invalid instruction: " + item.Text))
                .Select(item => new
                {
                    Name = item.Match.Groups["name"].Value,
                    Args = item.Match.Groups["args"].Value.SplitEmpty(',')
                        .Select(strArg => ParseInt("intruction argument", strArg))
                        .ToArray()
                })
                .Select(item => new Instruction(InstructionPrototypes.GetByName(item.Name), item.Args))
                .ToArray();
        }

        private static int ParseInt(string name, string value)
        {
            int result;
            if (!int.TryParse(value, out result))
            {
                throw new Exception($"cannot parse {name} '{value}' to int");
            }

            return result;
        }
    }
}