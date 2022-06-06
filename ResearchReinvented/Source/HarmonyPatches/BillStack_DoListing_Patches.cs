using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using UnityEngine;
using PeteTimesSix.ResearchReinvented.Extensions;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(BillStack), nameof(BillStack.DoListing))]
    public static class Patch_BillStack_DoListing_Patches 
    {
        [HarmonyPrefix]
        public static void Prefix(BillStack __instance, ref Func<List<FloatMenuOption>> recipeOptionsMaker) 
        {
            var oldRecipeOptionsMaker = recipeOptionsMaker;
            Func<List<FloatMenuOption>> newRecipeOptionsMaker = () =>
            {
                var vanillaResult = oldRecipeOptionsMaker();
                var prototypes = GetAvailablePrototypeOptions(__instance.billGiver);
                if (!prototypes.NullOrEmpty()) 
                {
                    if(vanillaResult.Count == 1 && vanillaResult[0].Label == "NoneBrackets".Translate())
                        vanillaResult.RemoveAt(0);
                    vanillaResult.AddRange(prototypes);
                }
                return vanillaResult;
            };
            recipeOptionsMaker = newRecipeOptionsMaker;
        }

        private static List<FloatMenuOption> GetAvailablePrototypeOptions(IBillGiver billGiver)
        {
            var asThing = billGiver as Thing;
            if (asThing == null)
                return null;

            var retList = new List<FloatMenuOption>();

            foreach (var recipe in asThing.def.AllRecipes) 
            {
                if (recipe.IsAvailableOnlyForPrototyping() && recipe.AvailableOnNow(asThing, null))
                {
                    var option = (new FloatMenuOption("[Prototype] " + recipe.LabelCap, () => OnClick(billGiver, asThing, recipe, null), recipe.UIIconThing, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => ExtraPartOnGUI(rect, recipe, null), null, true, 0));
                    retList.Add(option);
                    foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
                    {
                        foreach (Precept_Building precept_Building in ideo.cachedPossibleBuildings)
                        {
                            if (precept_Building.ThingDef == recipe.ProducedThingDef)
							{
                                var preceptOption = (new FloatMenuOption("[Prototype] " + "RecipeMake".Translate(precept_Building.def.LabelCap).CapitalizeFirst(), () => OnClick(billGiver, asThing, recipe, precept_Building), recipe.UIIconThing, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => ExtraPartOnGUI(rect, recipe, precept_Building), null, true, 0));
                                retList.Add(option);
                            }
                        }
                    }
                }
            }

            return retList;
        }

        internal static void OnClick(IBillGiver asBillGiver, Thing asThing, RecipeDef recipe, Precept_ThingStyle precept)
        {
            List<Pawn> freeColonists = asThing.Map.mapPawns.FreeColonists;
            if (!freeColonists.Any(p => recipe.PawnSatisfiesSkillRequirements(p)))
            {
                Bill.CreateNoPawnsWithSkillDialog(recipe);
            }
            Bill bill = recipe.MakeNewBill(precept);
            asBillGiver.BillStack.AddBill(bill);
            if (recipe.conceptLearned != null)
				{
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
            }
            if (TutorSystem.TutorialMode)
            {
                TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
            }
        }

        internal static bool ExtraPartOnGUI(Rect rect, RecipeDef recipe, Precept_ThingStyle precept)
        {
            return Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe, precept);
        }
    }
}
