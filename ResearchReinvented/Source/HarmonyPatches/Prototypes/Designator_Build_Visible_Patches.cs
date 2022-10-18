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

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.Visible), MethodType.Getter)]
    public static class Designator_Build_Visible_Patches
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var enumerator = instructions.GetEnumerator();

            var research_check_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Designator_Build), "entDef")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(BuildableDef), nameof(BuildableDef.IsResearchFinished))),
                new CodeInstruction(OpCodes.Brtrue_S)
            };

            var jump_new = new CodeInstruction(OpCodes.Brtrue_S);
            var add_prototype_check_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Designator_Build), "entDef")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Designator_Build_Visible_Patches), nameof(Designator_Build_Visible_Patches.GetIsAvailableForPrototyping))),
                jump_new
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, research_check_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            foreach (var instruction in iteratedOver)
                yield return instruction;

            if (!found)
            {
                Log.Warning("Designator_Build_Visible_Patches - failed to apply patch (instructions not found)");
                goto finalize;
            }
            else
            {
                jump_new.operand = matchedInstructions.First(c => c.opcode == OpCodes.Brtrue_S).operand;
                
                foreach (var checkInstruction in add_prototype_check_instructions)
                    yield return checkInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static bool GetIsAvailableForPrototyping(BuildableDef buildable)
        {
            return buildable.IsAvailableOnlyForPrototyping(true);
        }
    }
}
