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
    public class WorkGiver_Analyse : WorkGiver_Scanner
    {
        //public override bool Prioritized => true;

        public static Type DriverClass = typeof(JobDriver_Analyse);

        private int benchesCachedOnTick = -1;
        private int analyzablesCachedOnTick = -1;
        private Dictionary<Pawn, List<Building_ResearchBench>> cachedBenches = new Dictionary<Pawn, List<Building_ResearchBench>>();
        private List<ThingDef> cachedAnalyzables = new List<ThingDef>();

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (Find.ResearchManager.currentProj == null)
                {
                    return ThingRequest.ForGroup(ThingRequestGroup.Nothing);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.HaulableEver); //ThingRequest.ForGroup(ThingRequestGroup.Everything);
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

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return false;
            var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));

            if (!researchBenches.Any())
                return false;

            var analysableThings = GetAnalyzables();

            if (!analysableThings.Contains(thing.def))
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return null;
            var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));
            if (!researchBenches.Any())
                return null;

            var analysableThings = cachedAnalyzables;

            if (!analysableThings.Contains(thing.def))
                return null;

            var bestBench = researchBenches.First();

            //var closestBench = GenClosest.ClosestThing_Global(thing.Position, researchBenches) as Building_ResearchBench;

            var canDo =
                pawn.CanReserveSittableOrSpot(bestBench.InteractionCell, forced) &&
                pawn.CanReserve(thing, 1, -1, null, forced) &&
                new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();

            if (!canDo)
                return null;
            else
            {
                var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
                var opportunity = opportunities.First(o => (o.requirement as ROComp_RequiresThing).targetDef == thing.def);

                Log.Message("making job for " + thing.def.defName);

                Job job = JobMaker.MakeJob(opportunity.def.jobDef, thing, expiryInterval: 1500, checkOverrideOnExpiry: true);
                job.targetB = bestBench;
                ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
                //ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
                job.count = 1;
                return job;
            }
        }

        /*public override float GetPriority(Pawn pawn, TargetInfo target)
        {
            return target.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
        }*/

        /*public Thing ClosestSuitableAnalysableThing(IEnumerable<ThingDef> analysableThings, Thing researchBench, Pawn pawn)
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
        }*/


        private List<Building_ResearchBench> GetUsableResearchBenches(Pawn pawn)
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return new List<Building_ResearchBench>();

            if (benchesCachedOnTick != Find.TickManager.TicksGame)
            {
                cachedBenches.Clear();
                benchesCachedOnTick = Find.TickManager.TicksGame;

            }
            if (!cachedBenches.ContainsKey(pawn))
            {
                var benches = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.ResearchBench)
                    .Cast<Building_ResearchBench>()
                    .Where(bench => currentProj.CanBeResearchedAt(bench, false))
                    .Where(bench => pawn.CanReach(new LocalTargetInfo(bench), PathEndMode.InteractionCell, Danger.Unspecified))
                    .OrderByDescending(bench => bench.GetStatValue(StatDefOf.ResearchSpeedFactor))
                    .ToList();
                cachedBenches[pawn] = benches;
                return benches;
            }
            return cachedBenches[pawn];
        }


        private List<ThingDef> GetAnalyzables()
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return new List<ThingDef>();

            if (analyzablesCachedOnTick != Find.TickManager.TicksGame)
            {
                cachedAnalyzables.Clear();
                
                var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
                var analysableThings = opportunities.Select(o => (o.requirement as ROComp_RequiresThing).targetDef);

                analyzablesCachedOnTick = Find.TickManager.TicksGame;
                cachedAnalyzables = analysableThings.ToList();
            }

            return cachedAnalyzables;
        }

    }
}
