using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrainfuckCompiler.Brainfuck
{
    public class ConsoleDebugger
    {
        private List<DebugAction> debugActions = new List<DebugAction>()
        {
            new DebugAction()
            {
                Name = "Single Step",
                Action = bi => bi.SingleStep(),
                Key = ConsoleKey.F1,
            },
            new DebugAction()
            {
                Name = "Multi Step",
                Action = bi => bi.MultiStep(),
                Key = ConsoleKey.F2,
            },
            new DebugAction()
            {
                Name = "Breakpoint",
                Action = bi => bi.NextBreakpoint(),
                Key = ConsoleKey.F3,
            },
            new DebugAction()
            {
                Name = "Run",
                Action = bi => bi.Run(),
                Key = ConsoleKey.F4
            },
            new DebugAction()
            {
                Name = "Exit",
                Action = bi => bi.Terminate(),
                Key = ConsoleKey.Escape,
            },
            new DebugAction()
            {
                Name = "Restart",
                Action = bi => bi.Reset(),
                Key = ConsoleKey.F12
            }
        };

        public void Run(string source)
        {
            var intptr = new BrainfuckDebugger(source.Trim(), new byte[64]);
            intptr.Output = new StringWriter();

            while (!intptr.Terminated)
            {
                this.DebugDump(intptr);
                this.DebugStep(intptr);
            }

            this.DebugDump(intptr);
            Console.WriteLine("Press any key to exit");
            ConsumeConsoleKey();
        }

        private void DebugDump(BrainfuckDebugger intptr)
        {
            intptr.DebugDump();
            Console.WriteLine(intptr.Output);
        }

        private void DebugStep(BrainfuckDebugger intptr)
        {
            Console.Write("Choose Action: ");
            foreach (var action in this.debugActions)
            {
                Console.Write($"[{action.Key}] = {action.Name} | ");
            }

            var key = ConsumeConsoleKey().Key;
            var debugAction = this.debugActions.FirstOrDefault(action => action.Key == key) ?? this.debugActions[0];
            debugAction.Action(intptr);
        }

        /// <summary>
        /// Read a key from the Console without showing it on screen
        /// </summary>
        /// <returns>The key entered by the user</returns>
        private static ConsoleKeyInfo ConsumeConsoleKey()
        {
            var cout = Console.Out;
            Console.SetOut(new StringWriter());
            var key = Console.ReadKey();
            Console.SetOut(cout);
            return key;
        }
    }
}
