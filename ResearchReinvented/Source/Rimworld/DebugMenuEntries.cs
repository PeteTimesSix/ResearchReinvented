using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.Random;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
    [StaticConstructorOnStartup]
    public static partial class DebugMenuEntries
    {
        private const string CATEGORY = "Research Reinvented";

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void ToggleDebugPrintouts() 
        {
            ResearchReinvented_Debug.debugPrintouts = !ResearchReinvented_Debug.debugPrintouts;
            Log.Message($"Toggled debug printouts {(ResearchReinvented_Debug.debugPrintouts ? "on" : "off")}");
        }


        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void TogglePrototypeGridDrawing()
        {
            ResearchReinvented_Debug.drawPrototypeGrid = !ResearchReinvented_Debug.drawPrototypeGrid;
            Log.Message($"Toggled drawing of PrototypeGrid {(ResearchReinvented_Debug.drawPrototypeGrid ? "on" : "off")}");
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
