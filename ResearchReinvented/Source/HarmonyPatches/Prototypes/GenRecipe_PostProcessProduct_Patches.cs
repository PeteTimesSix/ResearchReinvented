using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
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
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class GenRecipe_PostProcessProduct_Patches
    {

        [HarmonyPostfix]
        public static void Postfix(Thing product, RecipeDef recipeDef, Pawn worker, Precept_ThingStyle precept = null)
        {
            var usedRecipe = recipeDef; 
            bool isPrototype = product.def.IsAvailableOnlyForPrototyping() || (usedRecipe != null && usedRecipe.IsAvailableOnlyForPrototyping());
            if (isPrototype)
            {
                PrototypeUtilities.DoPrototypeHealthDecrease(product, recipeDef);
                PrototypeUtilities.DoPrototypeBadComps(product, recipeDef);
                PrototypeKeeper.Instance.MarkAsPrototype(product);
                PrototypeUtilities.DoPostFinishThingResearch(worker, recipeDef.WorkAmountTotal(product), product, recipeDef);
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            var finally_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.workSkill))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) }))
            };

            var add_prototype_decrease_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PrototypeUtilities), nameof(PrototypeUtilities.DoPrototypeQualityDecreaseRecipe))),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, finally_instructions, out CodeInstruction[] matchedInstructions, out bool found);
            foreach (var instruction in iteratedOver)
            {
                yield return instruction;
            }

            if (!found)
            {
                Log.Warning("GenRecipe_PostProcessProduct_Patches - failed to apply patch (instructions not found)");
                goto finalize;
            }
            else
            {
                foreach (var checkInstruction in add_prototype_decrease_instructions)
                    yield return checkInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
