using System.Collections.Generic;
using System.Linq;
using BrainfuckCompiler.Compiler.CodeGenerator;
using BrainfuckCompiler.Compiler.Model;

namespace BrainfuckCompiler.Compiler.Pass
{
    public class InsertMethodsPass : ICompilerPass
    {
        private readonly GlobalScope scope;

        public InsertMethodsPass(GlobalScope scope)
        {
            this.scope = scope;
        }

        public List<Instruction> Pass(List<Instruction> list)
        {
            var visitedFunctions = new HashSet<int>();
            var functions = new Queue<int>();
            functions.EnqueAll(GetFunctionsToVisit(list, visitedFunctions));

            if (functions.Any())
            {
                var endLabel = this.scope.GetNextLabelId();
                list.Add(InstructionPrototypes.Jump, new int[] { endLabel });

                while (functions.Any())
                {
                    var fn = functions.Dequeue();
                    var fnInsn = this.scope.FunctionById[fn].Body;

                    list.Add(InstructionPrototypes.Label, new int[] { fn });
                    list.AddRange(fnInsn);

                    visitedFunctions.Add(fn);
                    functions.EnqueAll(GetFunctionsToVisit(fnInsn, visitedFunctions));
                }

                list.Add(InstructionPrototypes.Label, new int[] { endLabel });
            }

            return list;
        }

        private static IEnumerable<int> GetFunctionsToVisit(List<Instruction> list, HashSet<int> visitedFunctions)
        {
            return list
                .Where(insn => insn.Prototype == InstructionPrototypes.Invoke)
                .Select(insn => insn.Get(0))
                .Where(fn => !visitedFunctions.Contains(fn));
        }
    }
}
