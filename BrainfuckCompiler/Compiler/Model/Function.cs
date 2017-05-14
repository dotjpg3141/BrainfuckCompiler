using System;
using System.Collections.Generic;
using System.Linq;
using BrainfuckCompiler.Compiler.CodeGenerator;

namespace BrainfuckCompiler.Compiler.Model
{
    public class Function
    {
        public List<Variable> Parameters { get; set; }

        public DataType ReturnType { get; set; }

        public string Name { get; set; }

        public List<Instruction> Body { get; set; }

        public bool IsBuildIn { get; set; }

        public int Id { get; set; }

        public static Function BuildIn(string name, DataType returnType, DataType[] parameters, Instruction[] body)
        {
            if (returnType == null)
            {
                throw new ArgumentNullException(nameof(returnType));
            }

            return new Function()
            {
                Name = name,
                Parameters = parameters.Select((dt, i) => new Variable()
                {
                    Name = "var_" + i,
                    Type = dt
                }).ToList(),
                ReturnType = returnType,
                IsBuildIn = true,
                Body = body.ToList()
            };
        }

        public static Function BuildInBinary(string name, DataType type, Instruction[] body)
            => BuildIn(name, type, new[] { type, type }, body);
    }
}
