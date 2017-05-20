using System.Collections.Generic;
using BrainfuckCompiler.Compiler.CodeGenerator;
using static BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototypes;

namespace BrainfuckCompiler.Compiler.Model
{
    public class GlobalScope
    {
        private int nextLabelId = 0;

        public Variable ReturnAddress { get; private set; }

        public Dictionary<int, Function> FunctionById { get; } = new Dictionary<int, Function>();

        public int GetNextLabelId() => this.nextLabelId++;

        public static Scope Generate()
        {
            var global = new GlobalScope();

            var scope = new Scope(global)
            {
                Functions =
                {
                    Function.BuildIn("print", DataTypes.Void, new[] { DataTypes.Int }, new[]
                    {
                        new Instruction(PrintChar),
                        new Instruction(PopInt),
                    }),
                    Function.BuildIn("read", DataTypes.Int, new DataType[0], new[]
                    {
                        new Instruction(ReadChar),
                    }),
                    Function.BuildInBinary("==", DataTypes.Int, new[]
                    {
                        new Instruction(Sub),
                        new Instruction(Not),
                    }),
                    Function.BuildInBinary(">", DataTypes.Int, new[] { new Instruction(Greater) }),
                    Function.BuildInBinary("+", DataTypes.Int, new[] { new Instruction(Add) }),
                    Function.BuildInBinary("-", DataTypes.Int, new[] { new Instruction(Sub) }),
                    Function.BuildInBinary("*", DataTypes.Int, new[] { new Instruction(Mul) }),
                    Function.BuildInBinary("/", DataTypes.Int, new[] { new Instruction(Div) }),
                    Function.BuildInBinary("=", DataTypes.Int, new Instruction[0]),
                }
            };

            var returnAddress = new Variable()
            {
                Name = "<return_address>",
                Type = DataTypes.Fsm,
            };
            scope.DeclareVariable(returnAddress);
            global.ReturnAddress = returnAddress;
            return scope;
        }
    }
}
