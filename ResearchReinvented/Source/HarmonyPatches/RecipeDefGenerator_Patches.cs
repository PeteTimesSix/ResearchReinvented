using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefGenerators;
using PeteTimesSix.ResearchReinvented.Managers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(RecipeDefGenerator), "CreateRecipeDefFromMaker")]
    public static class RecipeDefGenerator_CreateRecipeDefFromMaker_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(RecipeDef __result, ThingDef def, int adjustedCount)
        {
            if (AlternateResearchSubjectDefGenerator.recipeMakerRecipes.TryGetValue(def, out var preexistingRecipes))
            {
                var recipes = preexistingRecipes.ToList();
                recipes.Add(__result);
                AlternateResearchSubjectDefGenerator.recipeMakerRecipes[def] = recipes.ToArray();
            }
            else
            {
                AlternateResearchSubjectDefGenerator.recipeMakerRecipes[def] = new RecipeDef[] { __result };
            }
        }
    }
}
