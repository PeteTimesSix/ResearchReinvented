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
using Verse.AI;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(BillStack), nameof(BillStack.DoListing))]
    public static class Patch_BillStack_DoListing_Patches
	{
		public delegate FloatMenuOption GenerateSurgeryOptionDelegate(Pawn pawn, Thing thingForMedBills, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, AcceptanceReport report, int index, BodyPartRecord part = null);

		public static GenerateSurgeryOptionDelegate method_GenerateSurgeryOptionDelegate;

        public static FieldRef<FloatMenuOption, string> field_FloatMenuOption_label;

        static Patch_BillStack_DoListing_Patches() 
        {
            method_GenerateSurgeryOptionDelegate = AccessTools.MethodDelegate<GenerateSurgeryOptionDelegate>(AccessTools.Method(typeof(HealthCardUtility), "GenerateSurgeryOption"));
            field_FloatMenuOption_label = AccessTools.FieldRefAccess<string>(typeof(FloatMenuOption), "labelInt");
		}

		[HarmonyPrefix]
        public static void Prefix(BillStack __instance, ref Func<List<FloatMenuOption>> recipeOptionsMaker) 
        {
            var oldRecipeOptionsMaker = recipeOptionsMaker;
            Func<List<FloatMenuOption>> newRecipeOptionsMaker = () =>
            {
                var vanillaResult = oldRecipeOptionsMaker();
                var billGiver = __instance.billGiver;
                if(billGiver is Pawn pawnBillGiver)
				{
					var surgeries = GetAvaiableExperimentalSurgeryOptions(pawnBillGiver, pawnBillGiver);
					if (!surgeries.NullOrEmpty())
					{
						if (vanillaResult.Count == 1 && vanillaResult[0].Label == "NoneBrackets".Translate())
							vanillaResult.RemoveAt(0);
						vanillaResult.AddRange(surgeries);
					}
				}
                else
				{
					var prototypes = GetAvailablePrototypeOptions(billGiver);
					if (!prototypes.NullOrEmpty())
					{
						if (vanillaResult.Count == 1 && vanillaResult[0].Label == "NoneBrackets".Translate())
							vanillaResult.RemoveAt(0);
						vanillaResult.AddRange(prototypes);
					}
				}
                return vanillaResult;
            };
            recipeOptionsMaker = newRecipeOptionsMaker;
        }

        public static List<FloatMenuOption> GetAvaiableExperimentalSurgeryOptions(Pawn pawn, Thing thingForMedBills) 
        {
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			int index = 0;
			foreach (RecipeDef recipe in thingForMedBills.def.AllRecipes)
			{
				if (recipe.IsAvailableOnlyForPrototyping(true))
				{
					AcceptanceReport report = recipe.Worker.AvailableReport(pawn, null);
					if (report.Accepted || !report.Reason.NullOrEmpty())
					{
						var missingIngredients = recipe.PotentiallyMissingIngredients(null, thingForMedBills.MapHeld);
						if (!missingIngredients.Any((ThingDef x) => x.isTechHediff))
						{
							if (!missingIngredients.Any((ThingDef x) => x.IsDrug) && (!missingIngredients.Any() || !recipe.dontShowIfAnyIngredientMissing))
							{
                                if (recipe.targetsBodyPart)
                                {
                                    foreach (var part in recipe.Worker.GetPartsToApplyOn(pawn, recipe))
                                    {
                                        if (recipe.AvailableOnNow(pawn, part))
                                        {
                                            //Log.Message($"pawn {pawn}/{thingForMedBills} adding {recipe} on part {part}");
                                            var option = method_GenerateSurgeryOptionDelegate(pawn, thingForMedBills, recipe, missingIngredients, report, index, part);
                                            field_FloatMenuOption_label(option) = "RR_ExperimentalSurgeryPrefix".Translate() + " " + field_FloatMenuOption_label(option);
											list.Add(option);
                                            index++;
                                        }
                                    }
                                }
                                else
								{
									//Log.Message($"pawn {pawn}/{thingForMedBills} adding {recipe}");
                                    var option = method_GenerateSurgeryOptionDelegate(pawn, thingForMedBills, recipe, missingIngredients, report, index, null);
									field_FloatMenuOption_label(option) = "RR_ExperimentalSurgeryPrefix".Translate() + " " + field_FloatMenuOption_label(option);
									list.Add(option);
                                    index++;
								}
							}
						}
					}
				}
			}
			return list;
		}

        public static List<FloatMenuOption> GetAvailablePrototypeOptions(IBillGiver billGiver)
        {
            var asThing = billGiver as Thing;
            if (asThing == null)
                return null;

            var retList = new List<FloatMenuOption>();

            foreach (var recipe in asThing.def.AllRecipes) 
            {
                if (recipe.IsAvailableOnlyForPrototyping() && recipe.AvailableOnNow(asThing, null))
                {
                    var option = new FloatMenuOption("RR_PrototypePrefix".Translate() + " " + recipe.LabelCap, () => OnClick(billGiver, asThing, recipe, null), recipe.UIIconThing, extraPartWidth: 29f, extraPartOnGUI: (Rect rect) => ExtraPartOnGUI(rect, recipe, null));
                    //var option = (new FloatMenuOption("RR_PrototypePrefix".Translate() + " " + recipe.LabelCap, () => OnClick(billGiver, asThing, recipe, null), recipe.UIIconThing, MenuOptionPriority.Default, null, null, , null, true, 0));
                    retList.Add(option);
                    foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
                    {
                        foreach (Precept_Building precept_Building in ideo.cachedPossibleBuildings)
                        {
                            if (precept_Building.ThingDef == recipe.ProducedThingDef)
                            {
                                var preceptOption = new FloatMenuOption("RR_PrototypePrefix".Translate() + " " + "RecipeMake".Translate(precept_Building.def.LabelCap).CapitalizeFirst(), () => OnClick(billGiver, asThing, recipe, precept_Building), recipe.UIIconThing, extraPartWidth: 29f, extraPartOnGUI: (Rect rect) => ExtraPartOnGUI(rect, recipe, precept_Building));
                                //var preceptOption = (new FloatMenuOption("RR_PrototypePrefix".Translate() + " " + "RecipeMake".Translate(precept_Building.def.LabelCap).CapitalizeFirst(), () => OnClick(billGiver, asThing, recipe, precept_Building), recipe.UIIconThing, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => ExtraPartOnGUI(rect, recipe, precept_Building), null, true, 0));
                                retList.Add(option);
                            }
                        }
                    }
                }
            }

            return retList;
        }

        public static void OnClick(IBillGiver asBillGiver, Thing asThing, RecipeDef recipe, Precept_ThingStyle precept)
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

        public static bool ExtraPartOnGUI(Rect rect, RecipeDef recipe, Precept_ThingStyle precept)
        {
            return Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe, precept);
        }
    }
}
