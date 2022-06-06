using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    /*[HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.AvailableNow), MethodType.Getter)]
    public static class RecipeDef_AvailableNow_Patches
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            //search for !this.researchPrerequisite.IsFinished
            var researchPrerequisite_IsFinished_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.researchPrerequisite))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ResearchProjectDef), nameof(ResearchProjectDef.IsFinished))),
                //new CodeInstruction(OpCodes.Brtrue_S) //need to NOT yield this too early
            };

            var iteratedOver = IterateTo(enumerator, researchPrerequisite_IsFinished_instructions, out CodeInstruction[] matchedInstructions, out bool found);
            foreach (var instruction in iteratedOver) 
            {
                yield return instruction;
            }

            if(matchedInstructions == null) 
            {
                Log.Warning("failed to apply patch (researchPrerequisite.IsFinished instructions not found)");
                goto finalize;
            }
            else 
            {
                enumerator.MoveNext();
                var jumpInstruction = enumerator.Current; 
                if(jumpInstruction == null)
                {
                    Log.Warning("failed to apply patch (researchPrerequisite.IsFinished was last instruction???");
                    goto finalize;
                }
                if(jumpInstruction.opcode != OpCodes.Brtrue_S) 
                {
                    Log.Warning("failed to apply patch (researchPrerequisite.IsFinished followup jump instruction not where expected)");
                    goto finalize;
                }
                else 
                {
                    foreach (var checkInstruction in PrototypeCheckInstructions((Label)jumpInstruction.operand))
                        yield return checkInstruction;
                }
                yield return jumpInstruction;
            }

            finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                Log.Message($"yielding {enumerator.Current}");
                yield return enumerator.Current;
            }
        }
        private static IEnumerable<CodeInstruction> PrototypeCheckInstructions(Label jumpTargetLabel)
        {
            var checkerFunc = AccessTools.Method(typeof(RecipeDef_AvailableNow_Patches), nameof(RecipeDef_AvailableNow_Patches.IsFinishedEnoughForPrototypes));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.researchPrerequisite)));
            yield return new CodeInstruction(OpCodes.Call, checkerFunc);
            yield return new CodeInstruction(OpCodes.Brtrue_S, jumpTargetLabel);
        }

        private static bool IsFinishedEnoughForPrototypes(this ResearchProjectDef project)
        {
            Log.Message($"checking if current project {project.defName} allows prototypes");
            return project.IsFinished || Find.ResearchManager.currentProj == project;
        }

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
            for(int i = 0; i < targetInstructions.Length; i++) 
            {
                Log.Message($"comparing {targetInstructions[i]} to {instructions[i]}");
                if(targetInstructions[i].opcode != instructions[i].opcode || targetInstructions[i].operand != instructions[i].operand)
                    return false;
            }
            return true;
        }
    }*/
}
