using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(WorkGiver_Researcher), nameof(WorkGiver_Researcher.ShouldSkip))]
    public static class WorkGiver_Researcher_Patches
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result) 
        {
            __result = true;   
            return false;
        }
    }
}
