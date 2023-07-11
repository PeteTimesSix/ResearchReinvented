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
    public static class DMM_Patch_BillStack_DoListing_Patches 
    {
        public delegate void DoRowDelegate(RecipeDef recipe, HashSet<Building> selectedTables, Precept_Building shittyPrecept = null);

        public static DoRowDelegate method_DoRow;
        public static FieldRef<Rect> GizmoListRect { get; set; }

        public static IEnumerable<CodeInstruction> Doink_Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            var type = AccessTools.TypeByName("DubsMintMenus.Patch_BillStack_DoListing");
            var listerField = type.GetField("lister", BindingFlags.Static | BindingFlags.NonPublic);
            method_DoRow = type.GetMethod("DoRow").CreateDelegate(typeof(DoRowDelegate)) as DoRowDelegate;

            var closeout_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldsfld, listerField),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Listing), nameof(Listing.CurHeight))),
                new CodeInstruction(OpCodes.Stsfld, type.GetField("RecipesScrollHeight", BindingFlags.Static | BindingFlags.NonPublic)),
                new CodeInstruction(OpCodes.Ldsfld, listerField),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Listing), nameof(Listing.End))),
                new CodeInstruction(OpCodes.Ret),
            };

            var add_prototypes_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.AddPrototypeRows))),
                new CodeInstruction(OpCodes.Ldsfld, listerField),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, closeout_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            if (!found)
            {
                Log.Warning("RR: DMM_Patch_BillStack_DoListing_Patches - Doink - failed to apply patch (instructions not found)");

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

        public static IEnumerable<CodeInstruction> DoRow_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var enumerator = instructions.GetEnumerator();

            /*var type = AccessTools.TypeByName("DubsMintMenus.Patch_BillStack_DoListing");
            var listerField = type.GetField("lister", BindingFlags.Static | BindingFlags.NonPublic);
            method_DoRow = type.GetMethod("DoRow").CreateDelegate(typeof(DoRowDelegate)) as DoRowDelegate;*/

            var invisbutton_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonInvisible))),
                new CodeInstruction(OpCodes.Brfalse)
            };

            var check_run_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.ShouldDoNormalButton))),
                new CodeInstruction(OpCodes.Brfalse),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, invisbutton_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            if (!found)
            {
                Log.Warning("RR: DMM_Patch_BillStack_DoListing_Patches - DoRow - failed to apply patch (instructions not found)");

                foreach (var instruction in iteratedOver)
                    yield return instruction;

                goto finalize;
            }
            else
            {
                foreach (var instruction in iteratedOver.Take(iteratedOver.Count() - matchedInstructions.Count()))
                    yield return instruction;

                var remainingInstructions = iteratedOver.Skip(iteratedOver.Count() - matchedInstructions.Count()); 

                //copy jump target
                check_run_instructions.First(i => i.opcode == OpCodes.Brfalse).operand = remainingInstructions.First(i => i.opcode == OpCodes.Brfalse).operand;

                foreach (var checkInstruction in check_run_instructions)
                    yield return checkInstruction;

                foreach (var instruction in remainingInstructions)
                    yield return instruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        
        private static bool staticHack_shouldDoNormalButton = true;
        public static bool ShouldDoNormalButton() 
        {
            return staticHack_shouldDoNormalButton;
        }

        private static Color CyanishTransparentBG = new Color(0.5f, 0.75f, 1f, 0.5f);
        private static Color CyanishTransparent = new Color(0.5f, 1f, 1f, 0.8f);

        private static void AddPrototypeRows(Listing_Standard lister, HashSet<Building> selectedTables)
        {
            HashSet<RecipeDef> prototypeRecipes = new HashSet<RecipeDef>();
            foreach(var building in selectedTables)
            {
                IBillGiver billGiver = building as IBillGiver;
                if (billGiver == null)
                    continue;

                foreach (var recipe in building.def.AllRecipes)
                {
                    if (recipe.IsAvailableOnlyForPrototyping() && recipe.AvailableOnNow(building, null))
                    {
                        prototypeRecipes.Add(recipe);
                    }
                }
            }
            foreach (var recipe in prototypeRecipes) 
            {
                DoPrototypeRow(lister, selectedTables, recipe, null);
                foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
                {
                    foreach (Precept_Building precept_Building in ideo.cachedPossibleBuildings)
                    {
                        if (precept_Building.ThingDef == recipe.ProducedThingDef)
                        {
                            DoPrototypeRow(lister, selectedTables, recipe, precept_Building);
                        }
                    }
                }
            }

            return;
        }

        private static void DoPrototypeRow(Listing_Standard lister, HashSet<Building> selectedTables, RecipeDef recipe, Precept_Building precept_Building)
        {
            var yBefore = lister.CurHeight;
            staticHack_shouldDoNormalButton = false;
            method_DoRow(recipe, selectedTables, null);
            staticHack_shouldDoNormalButton = true;
            var yAfter = lister.CurHeight;
            var height = yAfter - yBefore;

            var rect = lister.GetRect(0);
            var actualRect = new Rect(rect.x, rect.y - height, rect.width, height);

            if (!actualRect.Overlaps(GizmoListRect.Invoke()))
            {
                return;
            }

            var font = Text.Font;
            var anchor = Text.Anchor;
            var color = GUI.color;

            Text.Font = GameFont.Small;
            var protoLabel = "RR_PrototypeLabel".Translate();
            var labelRect = new Rect(actualRect.x + 2f, actualRect.y, actualRect.width - 4f, 20f);
            GUI.color = CyanishTransparentBG;
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.UpperCenter;
            GUI.color = CyanishTransparent;
            Widgets.Label(labelRect, protoLabel);

            Text.Font = font;
            Text.Anchor = anchor;
            GUI.color = color;

            if(Widgets.ButtonInvisible(actualRect))
            {
                if (selectedTables.Any())
                {
                    foreach (Building building in selectedTables)
                    {
                        IBillGiver billGiver = building as IBillGiver;
                        if (building.def.AllRecipes.Contains(recipe))
                        {
                            billGiver.BillStack.AddBill(recipe.MakeNewBill(precept_Building));
                            /*MainTabWindow_MintBillMenu instance = MainTabWindow_MintBillMenu._instance;
                            if (instance != null)
                            {
                                instance.OpenTables.Add(building);
                            }*/
                        }
                    }
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }
                else
                {
                    Messages.Message("Mint.SelectABenchToAddBills".Translate(), MessageTypeDefOf.NegativeEvent, false);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }
            }

            lister.GapLine(2f);
        }
    }
}
