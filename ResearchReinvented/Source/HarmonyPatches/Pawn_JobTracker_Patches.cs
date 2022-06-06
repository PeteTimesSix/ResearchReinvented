using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    public static class Pawn_JobTracker_CleanupCurrentJob_Prefix
    {
        [HarmonyPrefix]
        public static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (__instance == null || ___pawn == null || __instance.curJob?.def?.defName == null)
                return;
            if (__instance.curJob.def.defName.Contains("RR_"))
            {
                ResearchOpportunityManager.instance.ClearAssociatedJobWithOpportunity(___pawn, __instance.curJob);
            }
        }
    }
}
