using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;

namespace PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers
{
    public class WorkGiver_Warden_Interrogate : WorkGiver_Warden
    {
        public static Type DriverClass = typeof(JobDriver_InterrogatePrisoner);

        private static ResearchProjectDef _matchingOpportunitiesCachedFor;
        private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
        public static IEnumerable<ResearchOpportunity> MatchingOpportunities
        {
            get
            {
                if (_matchingOpportunitiesCachedFor != Find.ResearchManager.currentProj)
                {
                    _matchingOpportunitesCache = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunities(true)
                        .Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Social)).ToArray();
                    _matchingOpportunitiesCachedFor = Find.ResearchManager.currentProj;
                }
                return _matchingOpportunitesCache;
            }
        }
        public static void ClearMatchingOpportunityCache()
        {
            _matchingOpportunitiesCachedFor = null;
            _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (base.ShouldSkip(pawn, forced))
                return true;

            if (Find.ResearchManager.currentProj == null)
                return true;

            return !MatchingOpportunities.Any(o => !o.IsFinished);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn prisoner = (Pawn)thing; 
            if (!base.ShouldTakeCareOfPrisoner(pawn, prisoner, false))
                return false;
            PrisonerInteractionModeDef interactionMode = prisoner.guest.interactionMode;
            if (interactionMode != PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation)
                return false;

            if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Research))
            {
                JobFailReason.Is(StringsCache.JobFail_IncapableOfResearch, null);
                return false;
            }
            if (!pawn.workSettings?.WorkIsActive(WorkTypeDefOf.Research) ?? true)
            {
                JobFailReason.Is("NotAssignedToWorkType".Translate(WorkTypeDefOf.Research.gerundLabel).CapitalizeFirst(), null);
                return false;
            }
            if ((!prisoner.guest.ScheduledForInteraction))
            {
                JobFailReason.Is("PrisonerInteractedTooRecently".Translate(), null);
                return false;
            }
            if (!MatchingOpportunities.Any(o => o.requirement.MetBy(prisoner)))
            {
                Log.Message("no match opportunity");
                return false;
            }
            
            return base.HasJobOnThing(pawn, thing, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn prisoner = (Pawn)thing;
            if (!base.ShouldTakeCareOfPrisoner(pawn, prisoner, false))
                return null;
            PrisonerInteractionModeDef interactionMode = prisoner.guest.interactionMode;
            if (interactionMode != PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation || !prisoner.guest.ScheduledForInteraction || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || (prisoner.Downed && !prisoner.InBed()) || !pawn.CanReserve(prisoner, 1, -1, null, false) || !prisoner.Awake())
            {
                return null;
            }
            return JobMaker.MakeJob(JobDefOf_Custom.RR_InterrogatePrisoner, prisoner);
        }
    }
}
