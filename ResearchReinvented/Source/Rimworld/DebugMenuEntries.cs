using HarmonyLib;
using LudeonTK;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
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
        static void ListPrototypes()
        {
            Log.Message("Prototypes: " + string.Join(", ", PrototypeKeeper.Instance.Prototypes));
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

            var pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
            foreach (Pawn pawn in pawns) //for LIFE support
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }

            researchManager.ReapplyAllMods();
        }

        [DebugOutput(category = CATEGORY, name = "Special opportunities")]
        public static void ResearchProjects()
        {
            IEnumerable<SpecialResearchOpportunityDef> allDefs = DefDatabase<SpecialResearchOpportunityDef>.AllDefs;
            var entries = new List<TableDataGetter<SpecialResearchOpportunityDef>>
            {
                new TableDataGetter<SpecialResearchOpportunityDef>("defName", (SpecialResearchOpportunityDef d) => d.defName),
                new TableDataGetter<SpecialResearchOpportunityDef>("label", (SpecialResearchOpportunityDef d) => d.label),
                new TableDataGetter<SpecialResearchOpportunityDef>("type", (SpecialResearchOpportunityDef d) => d.opportunityType?.defName ?? "NULL"),
                new TableDataGetter<SpecialResearchOpportunityDef>("project", (SpecialResearchOpportunityDef d) => d.originalProject?.defName ?? "NULL"),
                new TableDataGetter<SpecialResearchOpportunityDef>("rare", (SpecialResearchOpportunityDef d) => d.rareForThis),
                new TableDataGetter<SpecialResearchOpportunityDef>("alternate", (SpecialResearchOpportunityDef d) => d.markAsAlternate),
                new TableDataGetter<SpecialResearchOpportunityDef>("originals", (SpecialResearchOpportunityDef d) => d.originals == null ? "NULL" : string.Join(",", d.originals.Select(a => a.defName))),
                new TableDataGetter<SpecialResearchOpportunityDef>("alternates", (SpecialResearchOpportunityDef d) => d.alternates == null ? "NULL" : string.Join(",", d.alternates.Select(a => a.defName))),
                new TableDataGetter<SpecialResearchOpportunityDef>("alternates (terrain)", (SpecialResearchOpportunityDef d) => d.alternateTerrains == null ? "NULL" : string.Join(",", d.alternateTerrains.Select(a => a.defName)))
            };
            DebugTables.MakeTablesDialog<SpecialResearchOpportunityDef>(allDefs, entries.ToArray());
        }
    }
}
