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
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.FinishProject))]
    public static class ResearchManager_FinishProject_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(ResearchProjectDef proj)
        {
            if (proj == null)
                return;

            ResearchOpportunityManager.instance.PostFinishProject(proj);
        }
    }

    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResetAllProgress))]
    public static class ResearchManager_ResetAllProgress_Patches
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ResearchOpportunityManager.instance.ResetAllProgress();
        }
    }
}
