using System;
using System.Collections.Generic;
using BrainfuckCompiler.Compiler.Model;
using InsnProto = BrainfuckCompiler.Compiler.CodeGenerator.InstructionPrototype;

namespace BrainfuckCompiler.Compiler.CodeGenerator
{
    public static class InstructionPrototypes
    {
        private static readonly Dictionary<string, InsnProto> Prototypes = new Dictionary<string, InsnProto>();

        private static readonly DataType[] Void0 = { };
        private static readonly DataType[] Int1 = { DataTypes.Int };
        private static readonly DataType[] Int2 = { DataTypes.Int, DataTypes.Int };
        private static readonly DataType[] Lbool1 = { DataTypes.LaxBool };
        private static readonly DataType[] Bool1 = { DataTypes.Bool };
        private static readonly DataType[] Fsm1 = { DataTypes.Fsm };

        public static InsnProto Nop { get; } = NewProto("nop", 1, Void0, Void0);

        public static InsnProto Breakpoint { get; } = NewProto("breakpoint", 0, Void0, Void0);

        public static InsnProto PrintInt { get; } = NewProto("printInt", 0, Int1, Int1);

        public static InsnProto ReadChar { get; } = NewProto("readChar", 0, Void0, Int1);

        public static InsnProto PrintChar { get; } = NewProto("printChar", 0, Int1, Int1);

        public static InsnProto PushInt { get; } = NewProto("pushInt", 1, Void0, Int1);

        public static InsnProto PopInt { get; } = NewProto("popInt", 0, Int1, Void0);

        public static InsnProto DupInt { get; } = NewProto("dupInt", 0, Int1, Int2);

        public static InsnProto SwapInt { get; } = NewProto("swapInt", 0, Int2, Int2);

        public static InsnProto Not { get; } = NewProto("not", 0, Lbool1, Bool1);

        public static InsnProto Add { get; } = NewProto("add", 0, Int2, Int1);

        public static InsnProto Sub { get; } = NewProto("sub", 0, Int2, Int1);

        public static InsnProto Mul { get; } = NewProto("mul", 0, Int2, Int1);

        public static InsnProto Div { get; } = NewProto("div", 0, Int2, Int1);

        public static InsnProto Greater { get; } = NewProto("greater", 0, Int2, Int1);

        public static InsnProto IfElseBegin { get; } = NewProto("ifElseBegin", 0, Int1, Void0);

        public static InsnProto IfElseElse { get; } = NewProto("ifElseElse", 0, Void0, Void0);

        public static InsnProto IfElseEnd { get; } = NewProto("ifElseEnd", 0, Void0, Void0);

        public static InsnProto FsmBegin { get; } = NewProto("fsmBegin", 1, Void0, Int1);

        public static InsnProto FsmEnd { get; } = NewProto("fsmEnd", 0, Fsm1, Void0);

        public static InsnProto FsmCase { get; } = NewProto("fsmCase", 0, Fsm1, Void0);

        public static InsnProto FsmJmp { get; } = NewProto("fsmJmp", 1, Void0, Fsm1);

        public static InsnProto FsmIf { get; } = NewProto("fsmIf", 2, Lbool1, Fsm1);

        public static InsnProto FsmCall { get; } = NewProto("fsmCall", 1, null, Fsm1);

        public static InsnProto FsmReturn { get; } = NewProto("fsmReturn", 0, Fsm1, Fsm1);

        public static InsnProto Jump { get; } = NewProto("jump", 1, Void0, Void0);

        public static InsnProto JumpIf { get; } = NewProto("jumpIf", 1, Void0, Void0);

        public static InsnProto Label { get; } = NewProto("label", 1, Void0, Void0);

        public static InsnProto Invoke { get; } = NewProto("invoke", 1, null, null);

        public static InsnProto Return { get; } = NewProto("return", 0, Void0, Void0);

        public static InsnProto ReadLocal { get; } = NewProto("readLocal", -1, Void0, Int1);

        public static InsnProto WriteLocal { get; } = NewProto("writeLocal", -1, Int1, Void0);

        public static InsnProto ClearLocal { get; } = NewProto("clearLocal", -1, Int1, Void0);

        public static InsnProto GetByName(string insnName)
        {
            if (!Prototypes.ContainsKey(insnName))
            {
                throw new ArgumentException("cannot find instruction " + insnName);
            }

            return Prototypes[insnName];
        }

        private static InsnProto NewProto(string name, int argCount, DataType[] input, DataType[] output)
            => Prototypes[name] = new InsnProto(name, argCount, input, output);
    }
}
