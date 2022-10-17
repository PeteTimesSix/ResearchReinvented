using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.HarmonyPatches;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Rimworld.Comps.CompProperties;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.Comps;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility;

namespace PeteTimesSix.ResearchReinvented
{
    public class ResearchReinventedMod : Mod
    {
        public static ResearchReinventedMod ModSingleton { get; private set; }
        public static ResearchReinvented_Settings Settings { get; internal set; }

        public static Harmony Harmony { get; internal set; }

        public ResearchReinventedMod(ModContentPack content) : base(content)
        {
            ModSingleton = this;

            Harmony = new Harmony("PeteTimesSix.ResearchReinvented");
            Harmony.PatchAll();

            OptionalPatches.Patch(Harmony);
        }

        public override string SettingsCategory()
        {
            return "ResearchReinvented_ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect) 
        {
            Settings.DoSettingsWindowContents(inRect);
        }
	}

    [StaticConstructorOnStartup]
    public static class ResearchReinvented_PostInit
    {

        static ResearchReinvented_PostInit()
        {
            OptionalPatches.PatchDelayed(ResearchReinventedMod.Harmony);

            AddRarityModExtensions();
            AssociateKitsWithResearchProjects();

            ResearchReinventedMod.Settings = ResearchReinventedMod.ModSingleton.GetSettings<ResearchReinvented_Settings>();
        }

        private static void AssociateKitsWithResearchProjects()
        {
            var researchKits = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.HasComp(typeof(Comp_ResearchKit)));
            foreach (var kit in researchKits)
            {
                var kitComp = kit.GetCompProperties<CompProperties_ResearchKit>();

                var prerequisites = new HashSet<ResearchProjectDef>();

                if (kitComp.substitutedResearchBench.researchPrerequisites != null)
                    prerequisites.AddRange(kitComp.substitutedResearchBench.researchPrerequisites);

                if (kitComp.remotesThrough != null && kitComp.remotesThrough.researchPrerequisites != null)
                    prerequisites.AddRange(kitComp.remotesThrough.researchPrerequisites);

                if (kitComp.substitutedFacilities != null) 
                {
                    foreach (var facility in kitComp.substitutedFacilities)
                    {
                        if (facility.researchPrerequisites != null)
                            prerequisites.AddRange(facility.researchPrerequisites);
                    }
                }

                if (prerequisites.Any())
                {
                    if (kit.researchPrerequisites == null)
                        kit.researchPrerequisites = new List<ResearchProjectDef>();

                    kit.researchPrerequisites.AddRange(prerequisites); 

                    if(kit.recipeMaker != null)
                    {
                        if (kit.recipeMaker.researchPrerequisites == null)
                            kit.recipeMaker.researchPrerequisites = new List<ResearchProjectDef>();

                        kit.recipeMaker.researchPrerequisites.AddRange(prerequisites);
                    }

                    //except the recipeMaker has probably already made the recipes, so lets fix those up too
                    foreach(var recipe in DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.ProducedThingDef == kit))
                    {
                        if (recipe.researchPrerequisites == null)
                            recipe.researchPrerequisites = new List<ResearchProjectDef>();

                        recipe.researchPrerequisites.AddRange(prerequisites);
                    }
                }
            }
        }

        private static void AddRarityModExtensions() 
        {
            foreach (var raresList in DefDatabase<RaresListDef>.AllDefsListForReading) 
            {
                if(raresList.things != null)
                {
                    foreach (var thing in raresList.things)
                        MarkAsRare(thing);
                }
            }
            foreach (var alternates in DefDatabase<AlternateResearchSubjectsDef>.AllDefsListForReading.Where(a => a.markAsRare))
            {
                if (alternates.alternates != null)
                {
                    foreach (var thing in alternates.alternates)
                        MarkAsRare(thing);
                }
            }
            /*foreach (var special in DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(a => a.markAsRare))
            {
                if (special.alternates != null)
                {
                    foreach (var thing in special.alternates)
                        MarkAsRare(thing);
                }
                if (special.alternateTerrains != null)
                {
                    foreach (var terrain in special.alternateTerrains)
                        MarkAsRare(terrain);
                }
            }*/
        }

        private static void MarkAsRare(Def def)
        {
            if (def.modExtensions == null)
                def.modExtensions = new List<DefModExtension>();
            if (!def.HasModExtension<RarityMarker>())
                def.modExtensions.Add(new RarityMarker());
        }
    }
}
