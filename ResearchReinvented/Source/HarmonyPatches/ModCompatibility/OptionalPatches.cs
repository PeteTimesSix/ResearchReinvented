using HarmonyLib;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility.Dubs_Mint_Menus;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }


        private static void Patch_DubsMintMenus(Harmony harmony)
        {
            Type type = AccessTools.TypeByName("DubsMintMenus.Patch_BillStack_DoListing");

            DMM_Patch_BillStack_DoListing_Patches.GizmoListRect = AccessTools.StaticFieldRefAccess<Rect>(AccessTools.Field(type, "GizmoListRect"));
            harmony.Patch(AccessTools.Method(type, "Doink"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.Doink_Transpiler))));
            harmony.Patch(AccessTools.Method(type, "DoRow"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.DoRow_Transpiler))));
        }
    }
}
