using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Utilities
{
    public static class TranspilerUtils
    {
        public static IEnumerable<CodeInstruction> IterateTo(IEnumerator<CodeInstruction> enumerator, CodeInstruction[] targetInstructions, out CodeInstruction[] matchedInstructions, out bool found)
        {
            found = false;
            var matchBuffer = new CircularBuffer<CodeInstruction>(targetInstructions.Length);
            var instructionBuffer = new List<CodeInstruction>();
            while (enumerator.MoveNext())
            {
                var instruction = enumerator.Current;

                instructionBuffer.Add(instruction);
                matchBuffer.PushBack(instruction);

                if (InstructionsMatch(targetInstructions, matchBuffer))
                {
                    matchedInstructions = matchBuffer.ToArray();
                    found = true;
                    return instructionBuffer;
                }
            }
            matchedInstructions = null;
            return instructionBuffer;
        }

        private static bool InstructionsMatch(CodeInstruction[] targetInstructions, CircularBuffer<CodeInstruction> instructions)
        {
            if (instructions.Count() < targetInstructions.Length)
                return false;
            for (int i = 0; i < targetInstructions.Length; i++)
            {
                //Log.Message($"comparing {targetInstructions[i]} to {instructions[i]}");
                if (targetInstructions[i].opcode != instructions[i].opcode || targetInstructions[i].operand != instructions[i].operand)
                    return false;
            }
            return true;
        }
    }
}
