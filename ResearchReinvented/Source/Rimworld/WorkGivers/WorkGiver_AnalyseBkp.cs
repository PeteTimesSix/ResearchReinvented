using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers
{
    public class WorkGiver_AnalyseBkp : WorkGiver_Scanner
    {
        public override bool Prioritized => true;

        public static Type DriverClass = typeof(JobDriver_Analyse);

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (Find.ResearchManager.currentProj == null)
                {
                    return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.ResearchBench);
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (Find.ResearchManager.currentProj == null)
                return true;
            var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
            var analysableThings = opportunities.Select(o => (o.requirement as ROComp_RequiresThing).targetDef);
            return !analysableThings.Any();
        }


        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var researchBench = thing as Building_ResearchBench;
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;

            var canDo =
                researchBench != null &&
                currentProj.CanBeResearchedAt(researchBench, false) &&
                pawn.CanReserve(thing, 1, -1, null, forced) &&
                (!thing.def.hasInteractionCell || pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)) &&
                new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();

            if (!canDo)
                return null;
            else 
            {
                var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
                var analysableThings = opportunities.Select(o => (o.requirement as ROComp_RequiresThing).targetDef);
                Thing targetThing = ClosestSuitableAnalysableThing(analysableThings, thing, pawn);
                if (targetThing != null && pawn.CanReserve(targetThing, 1, 1, null, false))
                {
                    var opportunity = opportunities.First(o => (o.requirement as ROComp_RequiresThing).targetDef == targetThing.def);
                    Job job = JobMaker.MakeJob(opportunity.def.jobDef, thing, expiryInterval: 1500, checkOverrideOnExpiry: true);
                    job.targetB = targetThing;
                    ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
                    //ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
                    job.count = 1;
                    return job;
                }
            }

            return null;
        }

        public Thing ClosestSuitableAnalysableThing(IEnumerable<ThingDef> analysableThings, Thing researchBench, Pawn pawn)
        {
            ThingRequest request = ThingRequest.ForGroup(ThingRequestGroup.ResearchBench);
            TraverseParms traverseParms = new TraverseParms()
            {
                pawn = pawn,
                mode = TraverseMode.ByPawn,
                maxDanger = Danger.Unspecified,
                fenceBlocked = false,
                canBashFences = false,
                canBashDoors = false
            };
            return GenClosest.ClosestThingReachable(researchBench.Position, researchBench.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, traverseParms, validator: (Thing checkedThing) => { 
                return !checkedThing.IsForbidden(pawn) && analysableThings.Contains(checkedThing.def); 
            });
            //return GenClosest.ClosestThingReachable(thing.Position, thing.Map, request, PathEndMode.Touch, traverseParms);
        }

        public override float GetPriority(Pawn pawn, TargetInfo target)
        {
            return target.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
        }
    }
}
