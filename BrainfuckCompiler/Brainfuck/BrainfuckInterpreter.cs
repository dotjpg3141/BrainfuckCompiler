using System;
using System.IO;

namespace BrainfuckCompiler.Brainfuck
{
    public class BrainfuckInterpreter
    {
        public BrainfuckInterpreter(string source, byte[] cache)
        {
            this.Source = source;
            this.Cache = cache;
            this.Input = Console.In;
            this.Output = Console.Out;
        }

        public TextReader Input { get; set; }

        public TextWriter Output { get; set; }

        public bool Terminated => this.InstructionPointer == this.Source.Length;

        public string Source { get; private set; }

        public byte[] Cache { get; private set; }

        public int CachePointer { get; set; }

        public int InstructionPointer { get; private set; }

        public char BreakpointChar { get; set; } = '#';

        public static Tuple<string, byte[]> Run(string source, string input, byte[] cache)
        {
            var intptr = new BrainfuckInterpreter(source, cache);
            intptr.Input = new StringReader(input);
            intptr.Output = new StringWriter();
            intptr.Run();
            return Tuple.Create(intptr.Output.ToString(), intptr.Cache);
        }

        public void Reset()
        {
            this.CachePointer = this.InstructionPointer = 0;
            this.Cache = new byte[this.Cache.Length];
        }

        public void Run()
        {
            while (!this.Terminated)
            {
                this.SingleStep();
            }
        }

        public void NextBreakpoint()
        {
            if (!this.Terminated)
            {
                this.SingleStep();
                while (!this.Terminated && this.Source[this.InstructionPointer] != this.BreakpointChar)
                {
                    this.SingleStep();
                }
            }
        }

        public void SingleStep()
        {
            if (this.Terminated)
            {
                return;
            }

            switch (this.Source[this.InstructionPointer])
            {
                case '>':
                    this.CachePointer = this.CachePointer == this.Cache.Length - 1 ? 0 : this.CachePointer + 1;
                    break;
                case '<':
                    this.CachePointer = this.CachePointer == 0 ? this.Cache.Length - 1 : this.CachePointer - 1;
                    break;
                case '+':
                    this.Cache[this.CachePointer]++;
                    break;
                case '-':
                    this.Cache[this.CachePointer]--;
                    break;
                case '[':
                    this.HandleStartLoop();
                    break;
                case ']':
                    this.HandleEndLoop();
                    break;
                case '.':
                    this.Output.Write((char)this.Cache[this.CachePointer]);
                    break;
                case ',':
                    this.Cache[this.CachePointer] = (byte)this.Input.Read();
                    break;
                case '#':
                    this.OnDebugInstruction();
                    break;
            }

            this.InstructionPointer++;

            // skip comments
            while (!this.Terminated &&
                   this.Source[this.InstructionPointer] != '<' && this.Source[this.InstructionPointer] != '>' && this.Source[this.InstructionPointer] != '.' &&
                   this.Source[this.InstructionPointer] != '-' && this.Source[this.InstructionPointer] != '+' && this.Source[this.InstructionPointer] != ',' &&
                   this.Source[this.InstructionPointer] != '[' && this.Source[this.InstructionPointer] != ']' && this.Source[this.InstructionPointer] != '#')
            {
                this.InstructionPointer++;
            }
        }

        public void MultiStep()
        {
            if (!this.Terminated)
            {
                var chr = this.Source[this.InstructionPointer];
                this.SingleStep();
                if (chr != '[' && chr != ']')
                {
                    while (!this.Terminated && this.Source[this.InstructionPointer] == chr)
                    {
                        this.SingleStep();
                    }
                }
            }
        }

        public void Terminate()
        {
            this.InstructionPointer = this.Source.Length;
        }

        protected virtual void OnDebugInstruction()
        {
        }

        private void HandleEndLoop()
        {
            int loop = 1;
            while (loop > 0)
            {
                this.InstructionPointer--;
                if (this.Source[this.InstructionPointer] == '[')
                {
                    loop--;
                }
                else if (this.Source[this.InstructionPointer] == ']')
                {
                    loop++;
                }
            }

            this.InstructionPointer--;
        }

        private void HandleStartLoop()
        {
            if (this.Cache[this.CachePointer] == 0)
            {
                int loop = 1;
                while (loop > 0)
                {
                    this.InstructionPointer++;
                    if (this.Source[this.InstructionPointer] == '[')
                    {
                        loop++;
                    }
                    else if (this.Source[this.InstructionPointer] == ']')
                    {
                        loop--;
                    }
                }
            }
        }
    }
}
