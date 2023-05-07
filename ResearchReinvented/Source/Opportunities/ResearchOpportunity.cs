using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.OpportunityJobPickers;
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

    public enum OpportunityAvailability 
    {
        Available,
        Disabled,
        Finished,
        ResearchTooLow,
        ResearchTooHigh,
        CategoryFinished,
        ResearchPrerequisitesNotMet,
        UnavailableReasonUnknown
    }

    public static class ResearchOpportunityChecker 
    {
        public static bool IsValid(this ResearchOpportunity opportunity)
        {
            return opportunity != null && opportunity.project != null && opportunity.requirement != null && opportunity.requirement.IsValid;
        }
    }

    public class ResearchOpportunity : IExposable, ILoadReferenceable
    {
        public ResearchProjectDef project;
        public ResearchOpportunityTypeDef def;

        public ResearchOpportunityComp requirement;
        //public List<ResearchProjectDef> otherResearchPrerequisites;
        //public List<MemeDef> memePrerequisites;

        public string debug_source;

        private float maximumProgress = 0;
        private float currentProgress = 0;
        private bool isForcedRare;
        private bool isAlternate;

        public ResearchRelation relation = ResearchRelation.Direct;
        public float importance;


        public int loadID = -1;


        public float Progress => currentProgress;
        public float MaximumProgress => def.GetCategory(relation).Settings.infiniteOverflow ? (project.baseCost - project.ProgressReal + Progress) : maximumProgress;
        public float ProgressFraction => Progress / MaximumProgress;
        public bool IsFinished => ProgressFraction >= 1f;

        public bool IsRare => isForcedRare ? true : requirement.IsRare;

        public bool IsAlternate => isAlternate;

        //public bool IsValid => requirement != null && requirement.IsValid;

        private List<JobDef> _jobDefsCached;
        public List<JobDef> JobDefs { 
            get 
            {
                if(_jobDefsCached == null)
                {
                    _jobDefsCached = JobPickerMaker.MakePicker(def.jobPickerClass ?? typeof(JobPicker_FromOpportunityDef)).PickJobs(this);
                }
                return _jobDefsCached;
            } 
        }

        public OpportunityAvailability CurrentAvailability
        {
            get
            {
                if (IsFinished)
                    return OpportunityAvailability.Finished;
                var categoryAvailable = def.GetCategory(relation).GetCurrentAvailability(this);
                return categoryAvailable;
                /*if(categoryAvailable == OpportunityAvailability.Available)
                {
                    if (def.handledBy == HandlingMode.Special_Prototype)
                    {
                        if (otherResearchPrerequisites.Any(r => !r.IsFinished))
                            return OpportunityAvailability.ResearchPrerequisitesNotMet;
                    }
                }
                return OpportunityAvailability.Available;*/
            }
        }

        public ResearchOpportunity() 
        {
            //deserialization only
        }

        public ResearchOpportunity(ResearchProjectDef project, ResearchOpportunityTypeDef def, ResearchRelation relation, ResearchOpportunityComp requirement, string debug_source, float importance = 1f, bool forceRare = false, bool isAlternate = false)
        {
            this.project = project;
            this.def = def;
            this.requirement = requirement;
            this.relation = relation;
            this.loadID = RR_UniqueIDsManager.instance.GetNextResearchOpportunityID();
            this.importance = importance;
            this.isForcedRare = forceRare;
            this.isAlternate = isAlternate;
            this.debug_source = debug_source; 
        }

        public void SetMaxProgress(float maxProgress) 
        {
            maximumProgress = maxProgress; 
        }

        /*public void AdjustMaxProgress(float fractionMultiplierTotal, float importanceCategoryTotal) 
        {
            float categoryTargetFraction = def.GetCategory(relation).targetFractionMultiplier / fractionMultiplierTotal;
            categoryTargetFraction *= def.GetCategory(relation).overflowMultiplier;
            var maxProgress = def.GetCategory(relation).infiniteOverflow ? float.MaxValue : project.baseCost * categoryTargetFraction;
            if (!this.isRare)
            {
                maxProgress /= Mathf.Min(importanceCategoryTotal, def.GetCategory(relation).targetIterations);
                maxProgress *= importance;
            }
            maximumProgress = Mathf.Min(maxProgress, project.baseCost);
        }*/

        public TaggedString ShortDesc 
        {
            get 
            {
                return $"[{project}] - [{def.GetCategory(relation).label}] - [{def.label}]: {requirement.ShortDesc} ({currentProgress} / {maximumProgress})";
            }
        }



        public TaggedString HintText
        {
            get
            {
                var text = def.GetShortDescCap(relation);
                string subject = requirement?.Subject;
                return text.Formatted(subject);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref project, "project");
            Scribe_Defs.Look(ref def, "def");

            Scribe_Deep.Look(ref requirement, "requirement");
            //Scribe_Collections.Look(ref otherResearchPrerequisites, "otherResearchPrerequisites", LookMode.Def);

            Scribe_Values.Look(ref maximumProgress, "maximumProgress");
            Scribe_Values.Look(ref currentProgress, "currentProgress");

            Scribe_Values.Look(ref loadID, "loadID", forceSave: true);

            Scribe_Values.Look(ref relation, "relation");
            Scribe_Values.Look(ref importance, "importance");

            Scribe_Values.Look(ref isForcedRare, "isForcedRare");
            Scribe_Values.Look(ref isAlternate, "isAlternate");
        }

        public bool ResearchTickPerformed(float amount, Pawn researcher)
        {
            if (Find.ResearchManager.currentProj == null) //current project either unset or finished this tick
                return true;
            amount *= 0.00825f;
            return ResearchPerformed(amount, researcher);
        }

        private bool ResearchPerformed(float amount, Pawn researcher)
        {
            amount *= Find.Storyteller.difficulty.researchSpeedFactor;
            if (researcher != null && researcher.Faction != null)
            {
                amount /= project.CostFactor(researcher.Faction.def.techLevel);
            }
            if (DebugSettings.fastResearch)
            {
                amount *= 500f;
            }

            if (currentProgress + amount >= MaximumProgress)
                amount = MaximumProgress - currentProgress;
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

        public bool ResearchChunkPerformed(Pawn researcher, string subjectName, HandlingMode mode, float moteOffsetHint = 0f)
        {
            if (Find.ResearchManager.currentProj == null) //current project either unset or finished this tick
                return true;

            if (!StatDefOf.ResearchSpeed.Worker.IsDisabledFor(researcher))
            {
                var amount = MaximumProgress * def.handlingModeModifiers.GetValueOrDefault(mode) * researcher.GetStatValue(StatDefOf.ResearchSpeed, true);
                amount = Math.Min(amount, MaximumProgress);
                //Log.Message($"permorming research chunk for {ShortDesc} : modifier: {mode} -> {def.handlingModeModifiers.GetValueOrDefault(mode)} amount {amount} (of {MaximumProgress})");
                if (ResearchReinventedMod.Settings.showProgressMotes && amount >= 1) 
                {
                    DoResearchProgressMote(researcher, subjectName, (int)amount, moteOffsetHint);
                }
                return ResearchPerformed(amount, researcher);
            }
            else 
            {
                Log.Warning($"RR: Pawn {researcher} tried to do research chunk despite being incapable of research");
                return false;
            }
        }

        public void DoResearchProgressMote(Pawn pawn, string subjectName, float amount, float moteOffsetHint)
        {
            var pos = pawn.DrawPos;
            pos.z += moteOffsetHint;
            MoteMaker.ThrowText(pos, pawn.Map, $"{def.GetHeaderCap(relation)} {(subjectName != null ? $"({subjectName})" : "")}: {Math.Round(amount)}", def.GetCategory(relation).color);
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
