using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Opportunities
{

    public enum ResearchRelation 
    {
        Direct,
        Ancestor,
        Descendant
    }

    public class ResearchOpportunity : IExposable, ILoadReferenceable
    {
        public ResearchProjectDef project;
        public ResearchOpportunityTypeDef def;

        public ResearchOpportunityComp requirement;

        private float maximumProgress = 0;
        private float currentProgress = 0;

        public float importance;

        public float Progress => currentProgress;

        public int loadID = -1;

        public ResearchRelation relation = ResearchRelation.Direct;

        public float MaximumProgress => def.GetCategory(relation).infiniteOverflow ? (project.baseCost - project.ProgressReal + Progress) : maximumProgress;
        public float ProgressFraction => Progress / MaximumProgress;
        public bool IsFinished => ProgressFraction >= 1f;

        public bool rare;

        public ResearchOpportunity() 
        {
            //deserialization only
        }

        public ResearchOpportunity(ResearchProjectDef project, ResearchOpportunityTypeDef def, ResearchRelation relation, ResearchOpportunityComp requirement, float importance = 1f, bool? rareOverride = null)
        {
            this.project = project;
            this.def = def;
            this.requirement = requirement;
            this.relation = relation;
            this.loadID = RR_UniqueIDsManager.instance.GetNextResearchOpportunityID();
            this.importance = importance;
            if (rareOverride.HasValue)
                this.rare = rareOverride.Value;
            else
                this.rare = requirement.IsRare;
        }

        public void AdjustMaxProgress(float fractionMultiplierTotal, float importanceCategoryTotal) 
        {
            float categoryTargetFraction = def.GetCategory(relation).targetFractionMultiplier / fractionMultiplierTotal;
            categoryTargetFraction *= def.GetCategory(relation).overflowMultiplier;
            var maxProgress = def.GetCategory(relation).infiniteOverflow ? float.MaxValue : project.baseCost * categoryTargetFraction;
            if (!this.rare)
            {
                maxProgress /= Mathf.Min(importanceCategoryTotal, def.GetCategory(relation).targetIterations);
                maxProgress *= importance;
            }
            maximumProgress = Mathf.Min(maxProgress, project.baseCost);
        }

        public TaggedString ShortDesc 
        {
            get 
            {
                return $"{def.label}: {requirement.ShortDesc} ({currentProgress} / {maximumProgress})";
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref project, "project");
            Scribe_Defs.Look(ref def, "def");

            Scribe_Deep.Look(ref requirement, "requirement");

            Scribe_Values.Look(ref maximumProgress, "maximumProgress");
            Scribe_Values.Look(ref currentProgress, "currentProgress");

            Scribe_Values.Look(ref loadID, "loadID");

            Scribe_Values.Look(ref relation, "relation");
        }

        public bool ResearchPerformed(float amount, Pawn researcher)
        {
            if (Find.ResearchManager.currentProj == null) //current project either unset or finished this tick
                return true;
            amount *= 0.00825f;
            amount *= Find.Storyteller.difficulty.researchSpeedFactor;
            if (researcher != null && researcher.Faction != null)
            {
                amount /= project.CostFactor(researcher.Faction.def.techLevel);
            }
            if (DebugSettings.fastResearch)
            {
                amount *= 500f;
            }

            if (currentProgress + amount >= maximumProgress)
                amount = maximumProgress - currentProgress;
            currentProgress += amount;
            if (researcher != null)
            {
                researcher.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
            }

            float total = Find.ResearchManager.GetProgress(project);
            total += amount;
            ResearchManagerAccess.Field_progress[project] = total;
            if (project.IsFinished)
            {
                ResearchOpportunityManager.instance.FinishProject(project, true, researcher);
            }

            return project.IsFinished || this.IsFinished;
        }

        public override string ToString()
        {
            return ShortDesc;
        }
        public string GetUniqueLoadID()
        {
            return "ResearchOpportunity_" + this.loadID;
        }
    }
}
