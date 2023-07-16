using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Managers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{


    [HarmonyPatch(typeof(DoorsDebugDrawer), nameof(DoorsDebugDrawer.DrawDebug))]
    public static class DoorsDebugDrawer_DrawDebug_Patch
    {
        [HarmonyPostfix]
        public static void AddictionalDebugDrawingInjection()
        {
            PrototypeKeeper.Instance.DebugDrawOnMap();
        }
    }
}
