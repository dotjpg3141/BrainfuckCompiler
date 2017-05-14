using System;
using System.Collections.Generic;

namespace BrainfuckCompiler.Compiler.Model
{
    public static class DataTypes
    {
        private static readonly Dictionary<int, DataType> IdToType = new Dictionary<int, DataType>();
        private static readonly Dictionary<string, DataType> NameToType = new Dictionary<string, DataType>();

        public static DataType Void { get; } = NewType(0, "void", null, null);

        public static DataType Int { get; } = NewType(1, "int", "<", ">");

        public static DataType LaxBool { get; } = NewType(2, "lbool", "<", ">");

        public static DataType Bool { get; } = NewType(3, "bool", "<", ">");

        public static DataType Fsm { get; } = NewType(4, "fsm", "<", ">");

        public static DataType String { get; } = NewType(5, "string", "<[<]<", ">>[>]");

        public static DataType GetFromId(int id)
        {
            if (!IdToType.ContainsKey(id))
            {
                throw new ArgumentException($"Could not find a type with id={id}.");
            }

            return IdToType[id];
        }

        public static DataType GetFromName(string name)
        {
            if (!NameToType.ContainsKey(name))
            {
                throw new ArgumentException($"Could not find a type with name '{name}'");
            }

            return NameToType[name];
        }

        private static DataType NewType(int id, string name, string moveLeft, string moveRight)
        {
            var type = new DataType(id, name, moveLeft, moveRight);
            IdToType[type.Id] = type;
            NameToType[type.Name] = type;
            return type;
        }
    }
}
