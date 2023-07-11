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
using System.Reflection.Emit;
using PeteTimesSix.ResearchReinvented.Utilities;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes;
using Verse.Sound;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    public static class DMM_Patch_HealthCardUtility_Patches
    {

        public delegate void GenerateListingDelegate(Pawn pawn2, RecipeDef recipe, BodyPartRecord part);

        public static GenerateListingDelegate method_GenerateListing;

        public static FieldRef<string> searchString { get; set; }

        public static IEnumerable<CodeInstruction> Postfix_Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            var type = AccessTools.TypeByName("DubsMintMenus.Patch_HealthCardUtility");
            var listerField = type.GetField("lister", BindingFlags.Static | BindingFlags.NonPublic);
            method_GenerateListing = type.GetMethod("GenerateListing").CreateDelegate(typeof(GenerateListingDelegate)) as GenerateListingDelegate;

            var closeout_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldsfld, listerField),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Listing), nameof(Listing.CurHeight))),
                new CodeInstruction(OpCodes.Stsfld, type.GetField("RecipesScrollHeight", BindingFlags.Static | BindingFlags.NonPublic)),
                new CodeInstruction(OpCodes.Ldsfld, listerField),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Listing), nameof(Listing.End)))
            };

            var add_prototypes_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DMM_Patch_HealthCardUtility_Patches), nameof(DMM_Patch_HealthCardUtility_Patches.AddPrototypeRows))),
                new CodeInstruction(OpCodes.Ldsfld, listerField),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, closeout_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            if (!found)
            {
                Log.Warning("RR: DMM_Patch_HealthCardUtility_Patches - Postfix - failed to apply patch (instructions not found)");

                foreach (var instruction in iteratedOver)
                    yield return instruction;

                goto finalize;
            }
            else
            {
                foreach (var instruction in iteratedOver.Take(iteratedOver.Count() - matchedInstructions.Count()))
                    yield return instruction;

                var remainingInstructions = iteratedOver.Skip(iteratedOver.Count() - matchedInstructions.Count());
                yield return remainingInstructions.First();

                foreach (var checkInstruction in add_prototypes_instructions)
                    yield return checkInstruction;

                foreach (var instruction in remainingInstructions.Skip(1))
                    yield return instruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static Color CyanishTransparentBG = new Color(0.5f, 0.75f, 1f, 0.5f);
        private static Color CyanishTransparent = new Color(0.5f, 1f, 1f, 0.8f);

        private class BillEntry
        {
            public RecipeDef def;

            public string label;

            public BodyPartRecord part;

            public BillEntry(string s, RecipeDef d, BodyPartRecord p)
            {
                label = s;
                def = d;
                part = p;
            }
        }

        private static int lastCacheFrame = 0;
        private static Pawn cachedPawn = null;
        private static List<BillEntry> BillsToListCache { get; set; } = new List<BillEntry>();

        private static void AddPrototypeRows(Listing_Standard lister, Pawn pawn, Thing thingForMedBills) 
        {
            Pawn pawn2 = thingForMedBills as Pawn;
            lastCacheFrame--;
            if (cachedPawn != pawn || lastCacheFrame < 0)
            {
			    lastCacheFrame = 500;
			    cachedPawn = pawn;
			    BillsToListCache.Clear();
                foreach (RecipeDef recipe in thingForMedBills.def.AllRecipes.Where((RecipeDef r) => r.IsAvailableOnlyForPrototyping(true) && r.AvailableOnNow(pawn2)).OrderByDescending((RecipeDef p) => p.label).ToList())
                {
                    try
                    {
                        List<ThingDef> list = recipe.PotentiallyMissingIngredients(null, thingForMedBills.Map).ToList();
                        if (list.Any((ThingDef x) => x.isTechHediff) || list.Any((ThingDef x) => x.IsDrug) || (list.Any() && recipe.dontShowIfAnyIngredientMissing))
                        {
                            continue;
                        }
                        if (recipe.targetsBodyPart)
                        {
                            foreach (BodyPartRecord bodyPart in recipe.Worker.GetPartsToApplyOn(pawn, recipe))
                            {
                                if (recipe.AvailableOnNow(pawn, bodyPart))
                                {
                                    string text = recipe.Worker.GetLabelWhenUsedOn(pawn2, bodyPart).CapitalizeFirst();
                                    if (bodyPart != null && !recipe.hideBodyPartNames)
                                    {
                                        text = text + "\n(" + bodyPart.Label + ")";
                                    }
                                    BillsToListCache.Add(new BillEntry(text, recipe, bodyPart));
                                }
                            }
                        }
                        else
                        {
                            string s = recipe.Worker.GetLabelWhenUsedOn(pawn2, null).CapitalizeFirst();
                            BillsToListCache.Add(new BillEntry(s, recipe, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        //Widgets.Label(rect, "Oops, something went wrong!");
                        Log.Warning(ex.ToString());
                    }
                }
            }
            foreach (BillEntry billEntry in BillsToListCache.Where((BillEntry x) => parc(x.def, x.label)))
            {
                try
                {

                    var yBefore = lister.CurHeight;
                    method_GenerateListing(pawn, billEntry.def, billEntry.part);
                    var yAfter = lister.CurHeight;
                    var height = yAfter - yBefore;

                    var rect = lister.GetRect(0);
                    var actualRect = new Rect(rect.x, rect.y - height, rect.width, height);

                    var font = Text.Font;
                    var anchor = Text.Anchor;
                    var color = GUI.color;

                    Text.Font = GameFont.Small;
                    var protoLabel = "RR_ExperimentalSurgeryLabel".Translate();
                    var labelRect = new Rect(actualRect.x + 2f, actualRect.y, actualRect.width - 4f, 20f);
                    GUI.color = CyanishTransparentBG;
                    GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
                    Text.Anchor = TextAnchor.UpperCenter;
                    GUI.color = CyanishTransparent;
                    Widgets.Label(labelRect, protoLabel);

                    Text.Font = font;
                    Text.Anchor = anchor;
                    GUI.color = color;


                    lister.GapLine(2f);
                }
                catch (Exception ex2)
                {
                    //Widgets.Label(rect, "Oops, something went wrong!");
                    Log.Warning(ex2.ToString());
                }
            }
        }

        private static bool DubsContains(this string source, string toCheck, StringComparison comp)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return source.IndexOf(toCheck, comp) >= 0;
            }
            return false;
        }

        public static bool parc(RecipeDef p, string label)
        {
            if (!string.IsNullOrEmpty(label) && label.DubsContains(searchString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (p.modContentPack != null && !string.IsNullOrEmpty(p.modContentPack.Name) && p.modContentPack.Name.DubsContains(searchString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
