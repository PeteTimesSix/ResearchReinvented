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

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
    public static class GenRecipe_PostProcessProduct_Patches
    {

        private static float PROTOTYPE_QUALITY_MULTIPLIER = 0.3f;
        private static float PROTOTYPE_HEALTH_MULTIPLIER = 0.3f;

        [HarmonyPostfix]
        public static void Postfix(Thing product, RecipeDef recipeDef, Pawn worker, Precept_ThingStyle precept = null) 
        {
            product.HitPoints = (int)Math.Max(1, (product.HitPoints * PROTOTYPE_HEALTH_MULTIPLIER));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            //search for !this.researchPrerequisite.IsFinished
            var finally_instructions = new CodeInstruction[] { //, AccessTools.Method(typeof(IDisposable), "Dispose")
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.workSkill))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) }))
            };

            var add_prototype_decrease_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenRecipe_PostProcessProduct_Patches), nameof(GenRecipe_PostProcessProduct_Patches.DoPrototypeQualityDecrease))),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, finally_instructions, out CodeInstruction[] matchedInstructions, out bool found);
            foreach (var instruction in iteratedOver)
            {
                yield return instruction;
            }

            if (!found)
            {
                Log.Warning("failed to apply patch (instructions not found)");
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

        private static QualityCategory DoPrototypeQualityDecrease(QualityCategory category, Thing product, RecipeDef recipe, Pawn worker)
        {
            bool isPrototype = recipe.IsAvailableOnlyForPrototyping();
            if(isPrototype)
            {
                byte asByte = (byte)category;
                var adjusted = (QualityCategory)Math.Max((byte)0, (byte)Math.Round((float)asByte * PROTOTYPE_QUALITY_MULTIPLIER));
                Log.Message($"adjusted quality for product {product} (recipe: {recipe} worker {worker}): {category} to {adjusted}");
                return adjusted;
            }
            return category;
        }
    }
}
