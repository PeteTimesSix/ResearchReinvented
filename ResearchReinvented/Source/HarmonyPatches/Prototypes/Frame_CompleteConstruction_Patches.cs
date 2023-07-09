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
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    public static class Frame_CompleteConstruction_Patches
    {

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FullTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var moddedInstructions = TranspilerForQuality(instructions);
            moddedInstructions = TranspilerSpawn(moddedInstructions);
            moddedInstructions = TranspilerPostTerrainSet(moddedInstructions);
            return moddedInstructions;
        }

        public static IEnumerable<CodeInstruction> TranspilerForQuality(IEnumerable<CodeInstruction> instructions) 
        {
            var enumerator = instructions.GetEnumerator();

            var finally_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(SkillDefOf), nameof(SkillDefOf.Construction))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) }))
            };

            var add_prototype_decrease_instructions = new CodeInstruction[] {
                //value is on stack
                new CodeInstruction(OpCodes.Ldloc_S, (byte)4),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostQuality))),
            };


            var iteratedOver = TranspilerUtils.IterateTo(enumerator, finally_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            foreach (var instruction in iteratedOver)
                yield return instruction;

            if (!found)
            {
                Log.Warning("RR: Frame_CompleteConstruction_Patches - TranspilerForQuality - failed to apply patch (instructions not found)");
                goto finalize;
            }
            else
            {
                foreach (var extraInstruction in add_prototype_decrease_instructions)
                    yield return extraInstruction;

                goto finalize;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static QualityCategory PostQuality(QualityCategory category, Thing product, Pawn worker)
        {
            return PrototypeUtilities.DoPrototypeQualityDecreaseThing(category, worker, product, null);
        }

        public static IEnumerable<CodeInstruction> TranspilerSpawn(IEnumerable<CodeInstruction> instructions)
        {
            var enumerator = instructions.GetEnumerator();

            byte localIndex = 4;

            var spawn_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_S, localIndex),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Position))),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Rotation))),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.Spawn), new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool)})),
                new CodeInstruction(OpCodes.Pop)
            };

            var add_prespawn_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, localIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PreSpawn))),
            };

            var add_postspawn_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, localIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostSpawn))),
            };

            var iteratedOver = TranspilerUtils.IterateTo(enumerator, spawn_instructions, out CodeInstruction[] matchedInstructions, out bool found);


            if (!found)
            {
                Log.Warning("RR: Frame_CompleteConstruction_Patches - TranspilerSpawn - failed to apply patch (instructions not found)");
                foreach (var instruction in iteratedOver)
                    yield return instruction;

                goto finalize;
            }
            else
            {
                foreach (var instruction in iteratedOver.Take(iteratedOver.Count() - matchedInstructions.Count()))
                    yield return instruction;

                foreach (var extraInstruction in add_prespawn_instructions)
                    yield return extraInstruction;

                foreach (var instruction in matchedInstructions)
                    yield return instruction;

                foreach (var extraInstruction in add_postspawn_instructions)
                    yield return extraInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static bool checkedIsPrototype = false;
        private static Thing checkedProduct = null; // sanity check

        private static void PreSpawn(Frame frame, Thing product, Pawn worker)
        {
            checkedProduct = product;
            checkedIsPrototype = product.def.IsAvailableOnlyForPrototyping() || PrototypeKeeper.Instance.IsPrototype(frame);
            if (checkedIsPrototype)
            {
                PrototypeUtilities.DoPrototypeHealthDecrease(product, null);
                PrototypeKeeper.Instance.UnmarkAsPrototype(frame);
            }
        }

        private static void PostSpawn(Frame frame, Thing product, Pawn worker)
        {
            if(checkedProduct == product && checkedIsPrototype)
            {
                PrototypeUtilities.DoPrototypeBadComps(product, null);
                PrototypeKeeper.Instance.MarkAsPrototype(product);
                PrototypeUtilities.DoPostFinishThingResearch(worker, frame.WorkToBuild, product, null);
            }
        }

        public static IEnumerable<CodeInstruction> TranspilerPostTerrainSet(IEnumerable<CodeInstruction> instructions)
        {
            var enumerator = instructions.GetEnumerator();

            var set_terrain_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Map), nameof(Map.terrainGrid))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Position))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.entityDefToBuild))),
                new CodeInstruction(OpCodes.Castclass, typeof(TerrainDef)),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.SetTerrain), new Type[] { typeof(IntVec3), typeof(TerrainDef) }))
            };

            var add_terrain_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Frame), nameof(Frame.def))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.entityDefToBuild))),
                new CodeInstruction(OpCodes.Castclass, typeof(TerrainDef)),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostSetTerrain))),
            };

            var iteratedOver = TranspilerUtils.IterateTo(enumerator, set_terrain_instructions, out CodeInstruction[] matchedInstructions, out bool found);

            foreach (var instruction in iteratedOver)
                yield return instruction;

            if (!found)
            {
                Log.Warning("Frame_CompleteConstruction_Patches - TranspilerPostTerrainSet - failed to apply patch (instructions not found)");
                goto finalize;
            }
            else
            {
                foreach (var extraInstruction in add_terrain_instructions)
                    yield return extraInstruction;
            }

        finalize:
            //output remaining instructions
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static void PostSetTerrain(Map map, Frame frame, TerrainDef terrainDef, Pawn worker)
        {
            bool isPrototype = terrainDef.IsAvailableOnlyForPrototyping(true) || PrototypeKeeper.Instance.IsPrototype(frame);
            if (isPrototype)
            {
                PrototypeKeeper.Instance.MarkTerrainAsPrototype(frame.Position, map, terrainDef);
                PrototypeUtilities.DoPostFinishTerrainResearch(worker, frame.WorkToBuild, terrainDef);
                PrototypeKeeper.Instance.UnmarkAsPrototype(frame);
            }
            else
            {
                PrototypeKeeper.Instance.UnmarkTerrainAsPrototype(frame.Position, map);
            }
        }
    }
}
