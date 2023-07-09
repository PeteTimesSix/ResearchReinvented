using HarmonyLib;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility.Combat_Extended;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility.Dubs_Mint_Menus;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility
{
    public static class OptionalPatches
    {
        public static void Patch(Harmony harmony) 
        {
            //Log.Warning("Doing optional patches...");
        }

        public static void PatchDelayed(Harmony harmony)
        {
            //Log.Warning("Doing delayed optional patches...");
            if (ModLister.GetActiveModWithIdentifier("dubwise.dubsmintmenus") != null)
            {
                //Log.Warning("Doing Dubs Mint menu patches...");
                Patch_DubsMintMenus(harmony);
            }
            if (ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null)
            {
                //Log.Warning("Doing Dubs Mint menu patches...");
                Patch_CombatExtended(harmony);
            }
			if (ModLister.GetActiveModWithIdentifier("erdelf.HumanoidAlienRaces") != null)
			{
				//Log.Warning("Doing Dubs Mint menu patches...");
				Patch_HumanoidAlienRaces(harmony);
			}
		}

        public static void Patch_DubsMintMenus(Harmony harmony)
        {
            Type billStack_DoListing = AccessTools.TypeByName("DubsMintMenus.Patch_BillStack_DoListing");

            DMM_Patch_BillStack_DoListing_Patches.GizmoListRect = AccessTools.StaticFieldRefAccess<Rect>(AccessTools.Field(billStack_DoListing, "GizmoListRect"));
            harmony.Patch(AccessTools.Method(billStack_DoListing, "Doink"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.Doink_Transpiler))));
            harmony.Patch(AccessTools.Method(billStack_DoListing, "DoRow"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.DoRow_Transpiler))));

            Type healthCardUtility = AccessTools.TypeByName("DubsMintMenus.Patch_HealthCardUtility");

            DMM_Patch_HealthCardUtility_Patches.searchString = AccessTools.StaticFieldRefAccess<string>(AccessTools.Field(healthCardUtility, "searchString"));
            harmony.Patch(AccessTools.Method(healthCardUtility, "Postfix"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_HealthCardUtility_Patches), nameof(DMM_Patch_HealthCardUtility_Patches.Postfix_Transpiler))));
        }

        public static void Patch_CombatExtended(Harmony harmony)
        {
            Type type = AccessTools.TypeByName("CombatExtended.HarmonyCE.Harmony_PawnRenderer").GetNestedType("Harmony_PawnRenderer_DrawBodyApparel");

            harmony.Patch(AccessTools.Method(type, "IsVisibleLayer"), postfix: new HarmonyMethod(AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel_Patches), nameof(Harmony_PawnRenderer_DrawBodyApparel_Patches.IsVisibleLayerPostfix))));
        }

		public static void Patch_HumanoidAlienRaces(Harmony harmony)
		{
			var raceCheck = new HarmonyMethod(AccessTools.TypeByName("AlienRace.HarmonyPatches").GetMethod("ShouldSkipResearchPostfix"));
			harmony.Patch(AccessTools.Method(typeof(WorkGiver_Analyse), nameof(WorkGiver_Analyse.ShouldSkip)), postfix: raceCheck);
			harmony.Patch(AccessTools.Method(typeof(WorkGiver_AnalyseInPlace), nameof(WorkGiver_AnalyseInPlace.ShouldSkip)), postfix: raceCheck);
			harmony.Patch(AccessTools.Method(typeof(WorkGiver_AnalyseTerrain), nameof(WorkGiver_AnalyseTerrain.ShouldSkip)), postfix: raceCheck);
			harmony.Patch(AccessTools.Method(typeof(WorkGiver_ResearcherRR), nameof(WorkGiver_ResearcherRR.ShouldSkip)), postfix: raceCheck);
		}
	}
}
