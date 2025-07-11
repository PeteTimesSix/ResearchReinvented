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

            var research_check_instructions = new CodeMatch[] {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Designator_Build), "entDef")),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(BuildableDef), nameof(BuildableDef.IsResearchFinished))),
                new CodeMatch(OpCodes.Brtrue_S),
            };

            var jump_new = new CodeInstruction(OpCodes.Brtrue_S);
            var add_prototype_check_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Designator_Build), "entDef")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Designator_Build_Visible_Patches), nameof(Designator_Build_Visible_Patches.GetIsAvailableForPrototyping))),
                jump_new
            };


            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchEndForward(research_check_instructions);
            if (codeMatcher.IsInvalid)
                goto invalid;
            jump_new.operand = codeMatcher.Operand;
            codeMatcher.Advance(1);
            codeMatcher.Insert(add_prototype_check_instructions);

            return codeMatcher.InstructionEnumeration();

        invalid:
            Log.Warning("RR: Designator_Build_Visible_Patches - failed to apply patch (instructions not found)");
            return instructions;
        }

        private static bool GetIsAvailableForPrototyping(BuildableDef buildable)
        {
            return buildable.IsAvailableOnlyForPrototyping(true);
        }
    }
}
