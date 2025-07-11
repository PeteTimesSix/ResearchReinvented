using HarmonyLib;
using LudeonTK;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Rimworld.UI.Dialogs;
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

            researchManager.ReapplyAllMods();
        }


        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void OpenAlternatesMapper()
        {
            Find.WindowStack.Add(new Dialog_AlternateMapper());
        }

        [DebugOutput(category = CATEGORY, name = "List special opportunities")]
        public static void SpecialOpportunitiesListing()
        {
            IEnumerable<SpecialResearchOpportunityDef> allDefs = DefDatabase<SpecialResearchOpportunityDef>.AllDefs;
            var entries = new List<TableDataGetter<SpecialResearchOpportunityDef>>
            {
                new TableDataGetter<SpecialResearchOpportunityDef>("defName", (SpecialResearchOpportunityDef d) => d.defName),
                new TableDataGetter<SpecialResearchOpportunityDef>("label", (SpecialResearchOpportunityDef d) => d.label),
                new TableDataGetter<SpecialResearchOpportunityDef>("type", (SpecialResearchOpportunityDef d) => d.opportunityType?.defName ?? "NULL"),
                new TableDataGetter<SpecialResearchOpportunityDef>("project", (SpecialResearchOpportunityDef d) => d.project?.defName ?? "NULL"),
                new TableDataGetter<SpecialResearchOpportunityDef>("rare", (SpecialResearchOpportunityDef d) => d.rare),
                new TableDataGetter<SpecialResearchOpportunityDef>("freebie", (SpecialResearchOpportunityDef d) => d.freebie),
                new TableDataGetter<SpecialResearchOpportunityDef>("thiCount", (SpecialResearchOpportunityDef d) => d.things == null ? "0" : d.things.Count),
                new TableDataGetter<SpecialResearchOpportunityDef>("terCount", (SpecialResearchOpportunityDef d) => d.terrains == null ? "0" : d.terrains.Count),
                new TableDataGetter<SpecialResearchOpportunityDef>("recCount", (SpecialResearchOpportunityDef d) => d.recipes == null ? "0" : d.recipes.Count),
                new TableDataGetter<SpecialResearchOpportunityDef>("things", (SpecialResearchOpportunityDef d) => d.things == null ? "NULL" : string.Join(", ", d.things.Select(a => a.defName))),
                new TableDataGetter<SpecialResearchOpportunityDef>("terrains", (SpecialResearchOpportunityDef d) => d.terrains == null ? "NULL" : string.Join(", ", d.terrains.Select(a => a.defName))),
                new TableDataGetter<SpecialResearchOpportunityDef>("recipes", (SpecialResearchOpportunityDef d) => d.recipes == null ? "NULL" : string.Join(", ", d.recipes.Select(a => a.defName))),
            };
            DebugTables.MakeTablesDialog<SpecialResearchOpportunityDef>(allDefs, entries.ToArray());
        }

        [DebugOutput(category = CATEGORY, name = "List alternates")]
        public static void AlternatesListing()
        {
            IEnumerable<AlternateResearchSubjectsDef> allDefs = DefDatabase<AlternateResearchSubjectsDef>.AllDefs;
            var entries = new List<TableDataGetter<AlternateResearchSubjectsDef>>
            {
                new TableDataGetter<AlternateResearchSubjectsDef>("defName", (AlternateResearchSubjectsDef d) => d.defName),
                new TableDataGetter<AlternateResearchSubjectsDef>("label", (AlternateResearchSubjectsDef d) => d.label),
                new TableDataGetter<AlternateResearchSubjectsDef>("originals", (AlternateResearchSubjectsDef d) => d.originals == null ? "NULL" : string.Join(", ", d.originals.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("originalTerrains", (AlternateResearchSubjectsDef d) => d.originalTerrains == null ? "NULL" : string.Join(", ", d.originalTerrains.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("altEqCount", (AlternateResearchSubjectsDef d) => d.alternatesEquivalent == null ? "0" : d.alternatesEquivalent.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("altSiCount", (AlternateResearchSubjectsDef d) => d.alternatesSimilar == null ? "0" : d.alternatesSimilar.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("altEqCountTer", (AlternateResearchSubjectsDef d) => d.alternateEquivalentTerrains == null ? "0" : d.alternateEquivalentTerrains.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("altSCountTer", (AlternateResearchSubjectsDef d) => d.alternateSimilarTerrains == null ? "0" : d.alternateSimilarTerrains.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("altEqCountRec", (AlternateResearchSubjectsDef d) => d.alternateEquivalentRecipes == null ? "0" : d.alternateEquivalentRecipes.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("altSiCountRec", (AlternateResearchSubjectsDef d) => d.alternateSimilarRecipes == null ? "0" : d.alternateSimilarRecipes.Count),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternatesEquivalent", (AlternateResearchSubjectsDef d) => d.alternatesEquivalent == null ? "NULL" : string.Join(", ", d.alternatesEquivalent.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternatesSimilar", (AlternateResearchSubjectsDef d) => d.alternatesSimilar == null ? "NULL" : string.Join(", ", d.alternatesSimilar.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternateEquivalentTerrains", (AlternateResearchSubjectsDef d) => d.alternateEquivalentTerrains == null ? "NULL" : string.Join(", ", d.alternateEquivalentTerrains.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternateSimilarTerrains", (AlternateResearchSubjectsDef d) => d.alternateSimilarTerrains == null ? "NULL" : string.Join(", ", d.alternateSimilarTerrains.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternateEquivalentRecipes", (AlternateResearchSubjectsDef d) => d.alternateEquivalentRecipes == null ? "NULL" : string.Join(", ", d.alternateEquivalentRecipes.Select(a => a.defName))),
                new TableDataGetter<AlternateResearchSubjectsDef>("alternateSimilarRecipes", (AlternateResearchSubjectsDef d) => d.alternateSimilarRecipes == null ? "NULL" : string.Join(", ", d.alternateSimilarRecipes.Select(a => a.defName))),
            };
            DebugTables.MakeTablesDialog<AlternateResearchSubjectsDef>(allDefs, entries.ToArray());
        }
    }
}
