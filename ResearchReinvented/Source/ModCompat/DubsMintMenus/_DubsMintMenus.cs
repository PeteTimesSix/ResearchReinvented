﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    [StaticConstructorOnStartup]
    public static class DubsMintMenus
    {
        public static bool active = false;
        public static bool success = true;

        static DubsMintMenus() 
        {
            active = ModLister.GetActiveModWithIdentifier("dubwise.dubsmintmenus") != null;
        }

        public static void PatchDelayed(Harmony harmony)
        {
            try
            {
                Type billStack_DoListing = AccessTools.TypeByName("DubsMintMenus.Patch_BillStack_DoListing");

                DMM_Patch_BillStack_DoListing_Patches.GizmoListRect = AccessTools.StaticFieldRefAccess<Rect>(AccessTools.Field(billStack_DoListing, "GizmoListRect"));
                harmony.Patch(AccessTools.Method(billStack_DoListing, "Doink"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.Doink_Transpiler))));
                harmony.Patch(AccessTools.Method(billStack_DoListing, "DoRow"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.DoRow_Transpiler))));
            }
            catch (Exception e)
            {
                Log.Warning("RR: Failed to apply Dubs Mint Menus compatibility patch (important: bill stack listing): " + e.Message);
                success = false;
            }

            try
            {
                Type healthCardUtility = AccessTools.TypeByName("DubsMintMenus.Patch_HealthCardUtility");

                DMM_Patch_HealthCardUtility_Patches.searchString = AccessTools.StaticFieldRefAccess<string>(AccessTools.Field(healthCardUtility, "searchString"));
                harmony.Patch(AccessTools.Method(healthCardUtility, "Postfix"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_HealthCardUtility_Patches), nameof(DMM_Patch_HealthCardUtility_Patches.Postfix_Transpiler))));
            }
            catch (Exception e)
            {
                Log.Warning("RR: Failed to apply Dubs Mint Menus compatibility patch (important: health card listing): " + e.Message);
                success = false;
            }
        }
    }
}
