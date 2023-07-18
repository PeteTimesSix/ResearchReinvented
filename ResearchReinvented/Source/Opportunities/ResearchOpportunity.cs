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

    [Flags]
    public enum OpportunityAvailability 
    {
        Available = 1,
        Disabled = 1 << 1,
        Finished = 1 << 2,
        ResearchTooLow = 1 << 3,
        ResearchTooHigh = 1 << 4,
        CategoryFinished = 1 << 5,
        ResearchPrerequisitesNotMet = 1 << 6,
        UnavailableReasonUnknown = 1 << 7
    }

    public static class ResearchOpportunityChecker 
    {
        public static bool IsValid(this ResearchOpportunity opportunity)
        {
            return opportunity != null && opportunity.def != null && opportunity.project != null && opportunity.requirement != null && opportunity.requirement.IsValid;
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
        public float MaximumProgress
        {
            get
            {
                var category = def.GetCategory(relation);
                if (category.maxAsRemaining && category.Settings.infiniteOverflow)
                    return project.baseCost - project.ProgressReal + Progress;
                else
                    return maximumProgress;
            }
        }

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
                var categoryAvailable = def.GetCategory(relation).GetCurrentAvailability(this.project);
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

        private bool ResearchPerformed(float amount, Pawn researcher, string moteSubjectName = null, float moteOffsetHint = 0f)
        {
            amount *= Find.Storyteller.difficulty.researchSpeedFactor; 
            amount *= def.GetCategory(relation).Settings.researchSpeedMultiplier;
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

                if (ResearchReinventedMod.Settings.showProgressMotes && amount >= 1)
                {
                    DoResearchProgressMote(researcher, (int)amount, moteSubjectName, moteOffsetHint);
                }
            }

            float total = Find.ResearchManager.GetProgress(project);
            total += amount;
            ResearchManagerAccess.Field_progress[project] = total;
            if (project.IsFinished)
            {
                ResearchOpportunityManager.Instance.FinishProject(project, true, researcher);
            }

            return project.IsFinished || this.IsFinished;
        }

        public bool ResearchTickPerformed(float amount, Pawn researcher, int tickModulo = 1)
        {
            if (Find.ResearchManager.currentProj == null) //current project either unset or finished this tick
                return true;

            researcher.skills.Learn(SkillDefOf.Intellectual, 0.1f * tickModulo);

            amount *= 0.00825f * tickModulo;
            return ResearchPerformed(amount, researcher);
        }

        public bool ResearchChunkPerformed(Pawn researcher, HandlingMode mode, float amount, float modifier, float xp, string moteSubjectName = null, float moteOffsetHint = 0f)
        {
            var startAmount = amount;
            if (Find.ResearchManager.currentProj == null) //current project either unset or finished this tick
                return true;

            if (!researcher.WorkTypeIsDisabled(WorkTypeDefOf.Research))
            {
                researcher.skills.Learn(SkillDefOf.Intellectual, xp);

                amount *= modifier;
                amount = Math.Min(amount, MaximumProgress);

                if (ResearchReinvented_Debug.debugPrintouts)
                    Log.Message($"performing research chunk for {ShortDesc}: modifier: {modifier} startAmount: {startAmount} amount {amount} ({amount * def.GetCategory(relation).Settings.researchSpeedMultiplier} after speedmult) (of {MaximumProgress})");

                return ResearchPerformed(amount, researcher, moteSubjectName, moteOffsetHint);
            }
            else 
            {
                Log.Warning($"RR: Pawn {researcher} tried to do research chunk despite being incapable of research");
                return false;
            }
        }

        public void DoResearchProgressMote(Pawn pawn, float amount, string moteSubjectName = null, float moteOffsetHint = 0f)
        {
            var pos = pawn.DrawPos;
            pos.z += moteOffsetHint;
            MoteMaker.ThrowText(pos, pawn.Map, $"{def.GetHeaderCap(relation)} {(moteSubjectName != null ? $"({moteSubjectName})" : "")}: {Math.Round(amount)}", def.GetCategory(relation).color);
        }

        public override string ToString()
        {
            return ShortDesc;
        }
        public string GetUniqueLoadID()
        {
            return "ResearchOpportunity_" + this.loadID;
        }

		public void FinishImmediately()
		{
            ResearchPerformed(MaximumProgress, null, null);
		}
	}
}
