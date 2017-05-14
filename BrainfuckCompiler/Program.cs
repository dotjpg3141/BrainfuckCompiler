using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BrainfuckCompiler.Brainfuck;
using BrainfuckCompiler.Compiler;
using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Model;
using BrainfuckCompiler.Compiler.Pass;
using BrainfuckCompiler.Compiler.Visitors;

namespace BrainfuckCompiler
{
    public class Program
    {
        public enum BrainfuckInterpreterExecution
        {
            No,
            YesRun,
            YesDebug
        }

        public static int Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
#if DEBUG
                Main(options);
                WaitForAnyKey();
                return 0;
#else
                try
                {
                    Run(options);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
#endif
            }

            return 1;
        }

        public static void Main(Options options)
        {
            var inputReader = options.InputFile == null ? Console.In :
                              new StreamReader(options.InputFile);

            var outputWriter = options.OutputFile == null ? Console.Out :
                               new StreamWriter(options.OutputFile);

            var execution = options.DebugBrainfuck ? BrainfuckInterpreterExecution.YesDebug :
                            options.RunBrainfuck ? BrainfuckInterpreterExecution.YesRun :
                            BrainfuckInterpreterExecution.No;

            Main(inputReader, outputWriter, !options.DisableCompilation, execution, options.Verbose);
        }

        public static void Main(TextReader reader, TextWriter writer, bool compile, BrainfuckInterpreterExecution execution, bool verbose)
        {
            string brainfuckSource = null;
            if (compile)
            {
                var compileInput = reader;
                var compileOuput = execution == BrainfuckInterpreterExecution.No ? writer : new StringWriter();
                Compile(compileInput, compileOuput, verbose);
                if (execution != BrainfuckInterpreterExecution.No)
                {
                    brainfuckSource = compileOuput.ToString();
                    writer.Write(brainfuckSource);
                }
            }

            if (execution != BrainfuckInterpreterExecution.No)
            {
                brainfuckSource = brainfuckSource ?? reader.ReadToEnd();
                if (execution == BrainfuckInterpreterExecution.YesRun)
                {
                    Runner(brainfuckSource);
                }
                else if (execution == BrainfuckInterpreterExecution.YesDebug)
                {
                    WaitForAnyKey("Press any key to start debugging");
                    Debugger(brainfuckSource);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public static void Compile(TextReader reader, TextWriter writer, bool verbose)
        {
            // parse input
            var charReader = new CharReader(reader);
            var tokenizer = new Tokenizer(charReader);
            var astGenerator = new AstGenerator(tokenizer);
            var statement = astGenerator.ParseCompilationUnit();

            // manipulate ast
            var scope = GlobalScope.Generate();
            statement = statement.Accept(scope, new AstParser());
            if (verbose)
            {
                DumpAstStatements(statement);
            }

            // generate instructions
            var instructions = statement.Accept(null, new InstructionGenerator());
            if (verbose)
            {
                DumpInstrutions("raw", instructions);
            }

            // transform instructions
            var passList = new ICompilerPass[]
            {
                new InsertMethodsPass(scope.GlobalScope),
                new JumpsToFsmPass(),
            };

            foreach (var pass in passList)
            {
                instructions = pass.Pass(instructions);
                if (verbose)
                {
                    DumpInstrutions(pass.GetType().Name, instructions);
                }
            }

            // instructions to brainfuck
            CodeWriter cw = new CodeWriter(writer);
            if (verbose)
            {
                DumpBrainfuckDebug(cw);
            }

            cw.Begin();
            foreach (var instruction in instructions)
            {
                cw.Write(instruction);
            }

            cw.End();
        }

        public static void Runner(string source)
        {
            Console.WriteLine("Running...");
        }

        public static void Debugger(string source)
        {
            MaximizeMainWindow();
            new ConsoleDebugger().Run(source);
        }

        private static void DumpBrainfuckDebug(CodeWriter cw)
        {
            cw.EmitDebugInfo = true;
            cw.EmitDebugBreakpoint = true;
            Console.WriteLine("Brainfuck");
            ConsoleWriteSeperator();
        }

        private static void DumpInstrutions(string header, List<Instruction> instructions)
        {
            Console.WriteLine("Instructions " + header);
            ConsoleWriteSeperator();
            foreach (var instruction in instructions)
            {
                Console.WriteLine(instruction);
            }

            Console.WriteLine();
        }

        private static void DumpAstStatements(AstStatement statement)
        {
            Console.WriteLine("AST Statements");
            ConsoleWriteSeperator();

            var writer = new IndentWriter(Console.Out);
            statement.Accept(writer, new StatementPrinter());
            Console.WriteLine();
        }

        private static void ConsoleWriteSeperator(char chr = '=')
        {
            Console.WriteLine(new string(chr, Console.WindowWidth - 1));
        }

        private static void WaitForAnyKey(string message = null)
        {
            Console.WriteLine(message ?? "Press any key to continue...");
            Console.ReadKey(true);
        }

        private static bool MaximizeMainWindow()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                const int SW_MAXIMIZE = 3;
                return NativeMethods.ShowWindow(process.MainWindowHandle, SW_MAXIMIZE);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
