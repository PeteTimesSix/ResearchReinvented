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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
    [HarmonyBefore(new string[]{ "OskarPotocki.VEF", "Uuugggg.rimworld.Replace_Stuff.main", "WorldbuilderMod" })]
    public static class Frame_CompleteConstruction_Patches
    {

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FullTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var thingLocal = FindThingLocal(instructions);
            if(thingLocal == null)
            {
                Log.Warning("RR: Frame_CompleteConstruction_Patches - failed to apply patches (could not locate Thing local index)");
                return instructions;
            }

            //return instructions;
            var moddedInstructions = TranspilerForQuality(instructions, thingLocal);
            moddedInstructions = TranspilerSpawn(moddedInstructions, thingLocal);
            moddedInstructions = TranspilerPostFoundationSet(moddedInstructions, thingLocal);
            moddedInstructions = TranspilerPostTerrainSet(moddedInstructions, thingLocal);
            return moddedInstructions;
        }

        private static object FindThingLocal(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher thingLocalFinder = new CodeMatcher(instructions);
            var thing_using_instructions = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ThingCompUtility), nameof(ThingCompUtility.TryGetComp), new Type[] { typeof(Thing) }).MakeGenericMethod(typeof(CompQuality))),
            };
            thingLocalFinder.MatchStartForward(thing_using_instructions);
            if (thingLocalFinder.IsInvalid)
            {
                return null;
            }
            var thingLocal = thingLocalFinder.Instruction.operand;
            return thingLocal;
        }

        public static IEnumerable<CodeInstruction> TranspilerForQuality(IEnumerable<CodeInstruction> instructions, object thingLocal) 
        {
            var finally_instructions = new CodeMatch[] {
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(SkillDefOf), nameof(SkillDefOf.Construction))),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef), typeof(bool) }))
            };

            var add_prototype_decrease_instructions = new CodeInstruction[] {
                //value is on stack
                new CodeInstruction(OpCodes.Ldloc_S, (byte)5),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostQuality))),
            };

            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchEndForward(finally_instructions);
            if (codeMatcher.IsInvalid)
                goto invalid;
            codeMatcher.Advance(1);
            codeMatcher.Insert(add_prototype_decrease_instructions);
            //codeMatcher.End();

            return codeMatcher.InstructionEnumeration();

        invalid:
            Log.Warning("RR: Frame_CompleteConstruction_Patches - TranspilerForQuality - failed to apply patch (instructions not found)");
            return instructions;
        }

        private static QualityCategory PostQuality(QualityCategory category, Thing product, Pawn worker)
        {
            return PrototypeUtilities.DoPrototypeQualityDecreaseThing(category, worker, product, null);
        }

        public static IEnumerable<CodeInstruction> TranspilerSpawn(IEnumerable<CodeInstruction> instructions, object thingLocal)
        {
            var spawn_instructions = new CodeMatch[] {
                new CodeMatch(OpCodes.Ldloc_S, thingLocal),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Position))),
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Rotation))),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.Spawn), new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool)})),
                new CodeMatch(OpCodes.Pop)
            };

            var add_prespawn_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, thingLocal),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PreSpawn))),
            };

            var add_postspawn_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, thingLocal),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostSpawn))),
            };

            var codeMatcher = new CodeMatcher(instructions);

            int invalidStage;
            codeMatcher.MatchStartForward(spawn_instructions);
            if (codeMatcher.IsInvalid)
            {
                invalidStage = 1;
                goto invalid;
            }
            codeMatcher.Insert(add_prespawn_instructions);
            codeMatcher.MatchEndForward(spawn_instructions);
            if (codeMatcher.IsInvalid)
            {
                invalidStage = 2;
                goto invalid;
            }
            codeMatcher.Advance(1);
            codeMatcher.Insert(add_postspawn_instructions);
            //codeMatcher.End();

            return codeMatcher.InstructionEnumeration();

        invalid:
            Log.Warning($"RR: Frame_CompleteConstruction_Patches - TranspilerSpawn - failed to apply patch (instructions not found, stage {invalidStage})");
            return instructions;
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
                PrototypeKeeper.Instance.MarkAsPrototype(product);  //now unmarked, so 
                PrototypeUtilities.DoPostFinishThingResearch(worker, frame.WorkToBuild * PrototypeUtilities.PROTOTYPE_WORK_MULTIPLIER, product, null);
            }
        }

        public static IEnumerable<CodeInstruction> TranspilerPostTerrainSet(IEnumerable<CodeInstruction> instructions, object thingLocal)
        {
            var set_terrain_instructions = new CodeMatch[] {
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.SetTerrain), new Type[] { typeof(IntVec3), typeof(TerrainDef) }))
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

            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchEndForward(set_terrain_instructions); 
            if (codeMatcher.IsInvalid)
                goto invalid;
            codeMatcher.Advance(1);
            codeMatcher.Insert(add_terrain_instructions);
            //codeMatcher.End();

            foreach (var instruction in codeMatcher.Instructions())
                yield return instruction;

            yield break;

        invalid:
            Log.Warning("Frame_CompleteConstruction_Patches - TranspilerPostTerrainSet - failed to apply patch (instructions not found)");
            foreach (var instruction in instructions)
                yield return instruction;
        }

        public static IEnumerable<CodeInstruction> TranspilerPostFoundationSet(IEnumerable<CodeInstruction> instructions, object thingLocal)
        {
            var set_terrain_instructions = new CodeMatch[] {
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.SetFoundation), new Type[] { typeof(IntVec3), typeof(TerrainDef) }))
            };

            var add_terrain_instructions = new CodeInstruction[] {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Frame), nameof(Frame.def))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ThingDef), nameof(ThingDef.entityDefToBuild))),
                new CodeInstruction(OpCodes.Castclass, typeof(TerrainDef)),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Frame_CompleteConstruction_Patches), nameof(Frame_CompleteConstruction_Patches.PostSetFoundation))),
            };

            var codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchEndForward(set_terrain_instructions);
            if (codeMatcher.IsInvalid)
                goto invalid;
            codeMatcher.Advance(1);
            codeMatcher.Insert(add_terrain_instructions);
            //codeMatcher.End();

            foreach (var instruction in codeMatcher.Instructions())
                yield return instruction;

            yield break;

        invalid:
            Log.Warning("Frame_CompleteConstruction_Patches - TranspilerPostFoundationSet - failed to apply patch (instructions not found)");
            foreach (var instruction in instructions)
                yield return instruction;
        }

        private static void PostSetTerrain(Map map, Frame frame, TerrainDef terrainDef, Pawn worker)
        {
            if (terrainDef.temporary)
                return;

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

        private static void PostSetFoundation(Map map, Frame frame, TerrainDef terrainDef, Pawn worker)
        {
            if (terrainDef.temporary)
                return;

            bool isPrototype = terrainDef.IsAvailableOnlyForPrototyping(true) || PrototypeKeeper.Instance.IsPrototype(frame);
            if (isPrototype)
            {
                PrototypeKeeper.Instance.MarkFoundationTerrainAsPrototype(frame.Position, map, terrainDef);
                PrototypeUtilities.DoPostFinishTerrainResearch(worker, frame.WorkToBuild, terrainDef);
                PrototypeKeeper.Instance.UnmarkAsPrototype(frame);
            }
            else
            {
                PrototypeKeeper.Instance.UnmarkFoundationTerrainAsPrototype(frame.Position, map);
            }
        }
    }
}
