using System;

namespace BrainfuckCompiler.Compiler.CodeGenerator
{
    public static class InstructionUtil
    {
        public static int CalculateFsmCaseOffset(int sourceCase, int targetCase, int caseCount)
        {
            // 0            = before first case
            // 1..caseCount = case 1 .. case caseCount
            // caseCount+1  = after last case
            if (targetCase == -1)
            {
                targetCase = caseCount + 1;
            }

            if (caseCount < 0 ||
                sourceCase < 0 || sourceCase > caseCount ||
                targetCase <= 0 || targetCase > caseCount + 1)
            {
                throw new ArgumentException();
            }

            var delta = targetCase - sourceCase;
            if (delta <= 0)
            {
                delta += caseCount + 1;
            }

            return delta;
        }
    }
}
