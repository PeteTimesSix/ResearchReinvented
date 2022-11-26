using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
    [StaticConstructorOnStartup]
    public static partial class DebugMenuEntries
    {
        private const string CATEGORY = "Research Reinvented";

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void ToggleDebugPrintouts() 
        {
            ResearchReinventedMod.Settings.debugPrintouts = !ResearchReinventedMod.Settings.debugPrintouts;
            string state = ResearchReinventedMod.Settings.debugPrintouts ? "on" : "off";
            Log.Message($"Toggled debug printouts {state}");
        }

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void ResetResearchProgress()
        {
            Find.ResearchManager.ResetAllProgress();
        }

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void RemoveAllResearch() 
        {
            var researchManager = Find.ResearchManager;
            researchManager.ResetAllProgress();

            var progressDict = AccessTools.Field(typeof(ResearchManager), "progress").GetValue(researchManager) as Dictionary<ResearchProjectDef, float>;

            if (progressDict != null)
            {
                progressDict.Clear();
                foreach (var allDef in DefDatabase<ResearchProjectDef>.AllDefs) 
                {
                    progressDict.Add(allDef, 0); 
                }
            }
            researchManager.ReapplyAllMods();
        }
    }
}
