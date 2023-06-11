using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
    [HarmonyPatch(typeof(Frame), nameof(Frame.FailConstruction))]
    public static class Frame_FailConstruction_Patches
    {
        [HarmonyPostfix]
        public static void Frame_FailConstruction_Postfix(Frame __instance, Pawn worker)
        {
            Frame frame = __instance;
            if(frame.def.entityDefToBuild is TerrainDef terrainDef)
            {
                PrototypeUtilities.DoPostFailToFinishTerrainResearch(worker, frame.WorkToBuild, frame.workDone, terrainDef);
            }
            else if(frame.def.entityDefToBuild is ThingDef thingDef)
            {

                PrototypeUtilities.DoPostFailToFinishThingResearch(worker, frame.WorkToBuild, frame.workDone, thingDef, null);
            }
        }
    }
}
