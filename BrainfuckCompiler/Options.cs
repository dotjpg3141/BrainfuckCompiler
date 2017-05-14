using CommandLine;
using CommandLine.Text;

namespace BrainfuckCompiler
{
    public class Options
    {
        private const string BrainfuckExecution = "bf-execution";

        [Option('i', "input", HelpText = "Input file to read. If not specified, read from StdIn.")]
        public string InputFile { get; set; }

        [Option('o', "output", HelpText = "Output file to write. If not specified, write to StdOut.")]
        public string OutputFile { get; set; }

        [Option('n', "no-compilation", HelpText = "Disable compilation to brainfuck source code.")]
        public bool DisableCompilation { get; set; }

        [Option('r', "run", MutuallyExclusiveSet = BrainfuckExecution, HelpText = "Run brainfuck.")]
        public bool RunBrainfuck { get; set; }

        [Option('d', "debug", MutuallyExclusiveSet = BrainfuckExecution, HelpText = "Run brainfuck via a debugger.")]
        public bool DebugBrainfuck { get; set; }

        [Option('v', "verbose", HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [HelpOption('h', "help", HelpText = "Display this help screen.")]
        public string GetUsage()
            => HelpText.AutoBuild(this);
    }
}
