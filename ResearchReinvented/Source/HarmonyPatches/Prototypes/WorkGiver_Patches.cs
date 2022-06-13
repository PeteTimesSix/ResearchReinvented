using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(WorkGiver_ConstructFinishFrames), nameof(WorkGiver_ConstructFinishFrames.JobOnThing))]
    public static class WorkGiver_ConstructFinishFrames_JobOnThing_Patches
    {
        /*public static void Postfix(ref Job __result, Pawn pawn, Thing thing) 
        {
            if(__result != null)
            {
                Log.Message($"Checking if a job on {thing} is a prototype and if {pawn} can research");
                if (!IsNotPrototypeOrCanResearch(thing as Frame, pawn)) 
                {
                    Log.Message($"Blocked a job on {thing} : is a prototype and {pawn} cant research");
                    JobMaker.ReturnToPool(__result);
                    __result = null;
                }
            }
        }*/

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var enumerator = instructions.GetEnumerator();

            var canconstruct_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })),
                new CodeInstruction(OpCodes.Brtrue_S),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Ret)
            };

            var additional_check_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WorkGiver_ConstructFinishFrames_JobOnThing_Patches), nameof(WorkGiver_ConstructFinishFrames_JobOnThing_Patches.IsNotPrototypeOrCanResearch))),
                new CodeInstruction(OpCodes.Brtrue_S),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Ret)
            };
            var newJump = additional_check_instructions[3];

            var newLabel = generator.DefineLabel();
            additional_check_instructions[0].labels.Add(newLabel);

            var iteratedOver = TranspilerUtils.IterateTo(enumerator, canconstruct_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            if (!found)
            {
                Log.Warning("failed to apply patch (instructions not found)");
                foreach (var instruction in iteratedOver)
                    yield return instruction;
                goto finalize;
            }
            else
            {
                var oldJump = matchedInstructions.First(c => c.opcode == OpCodes.Brtrue_S);
                var oldLabel = (Label)oldJump.operand;
                oldJump.operand = newLabel;
                newJump.operand = oldLabel;

                foreach (var instruction in iteratedOver)
                    yield return instruction;
                foreach (var checkInstruction in additional_check_instructions)
                    yield return checkInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static bool IsNotPrototypeOrCanResearch(Frame frame, Pawn worker)
        {
            if (frame == null)
                return false;
            var canResearch = !(StatDefOf.ResearchSpeed.Worker.IsDisabledFor(worker));
            var isPrototype = frame.def.entityDefToBuild.IsAvailableOnlyForPrototyping();
            Log.Message($"Checking if a job on {frame} is a prototype and if {worker} can research,  can:{canResearch}, must: {isPrototype}, result {canResearch || !isPrototype}");
            return canResearch || !isPrototype;
        }
    }
}
