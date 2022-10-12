using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
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

        private static IEnumerable<ResearchOpportunity> MatchingOpportunities => ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities().Where(o => o.def.handledBy == HandlingMode.Job && o.def.jobDef?.driverClass == DriverClass);

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

            return !MatchingOpportunities.Any();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return false;

            if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
            {
                BuildCache();
            }

            if (!opportunityCache.ContainsKey(thing.def))
                return false;

            var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));
            if (!researchBenches.Any())
                return false;
            var bestBench = researchBenches.First();

            if (!(pawn.CanReserveSittableOrSpot(bestBench.InteractionCell, forced) && 
                pawn.CanReserve(thing, 1, -1, null, forced) && 
                new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job()))
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
            {
                BuildCache();
            }

            var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));
            var bestBench = researchBenches.First();

            var opportunity = opportunityCache[thing.def].First();

            Job job = JobMaker.MakeJob(opportunity.def.jobDef, thing, expiryInterval: 20000, checkOverrideOnExpiry: true);
            job.targetB = bestBench;
            ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
            job.count = 1;
            return job;
        }

        //cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
        public static int cacheBuiltOnTick = -1;
        public static Dictionary<Pawn, List<Building_ResearchBench>> benchCache = new Dictionary<Pawn, List<Building_ResearchBench>>();
        public static Dictionary<ThingDef, HashSet<ResearchOpportunity>> opportunityCache = new Dictionary<ThingDef, HashSet<ResearchOpportunity>>();

        public static void BuildCache()
        {
            benchCache.Clear();
            opportunityCache.Clear();
            if (Find.ResearchManager.currentProj == null)
                return;

            foreach (var opportunity in MatchingOpportunities.Where(o => o.requirement is ROComp_RequiresThing))
            {
                var thingDef = (opportunity.requirement as ROComp_RequiresThing)?.thingDef;
                if(thingDef == null) 
                {
                    Log.ErrorOnce($"RR: current research project {Find.ResearchManager.currentProj} generated a WorkGiver_Analyze opportunity with null requirement!", Find.ResearchManager.currentProj.debugRandomId);
                    continue;
                }
                if (!opportunityCache.ContainsKey(thingDef))
                    opportunityCache[thingDef] = new HashSet<ResearchOpportunity>();

                opportunityCache[thingDef].Add(opportunity);
            }

            cacheBuiltOnTick = Find.TickManager.TicksAbs;
        }


        public static List<Building_ResearchBench> GetUsableResearchBenches(Pawn pawn)
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return new List<Building_ResearchBench>();

            if (!benchCache.ContainsKey(pawn))
            {
                var benches = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.ResearchBench)
                    .Cast<Building_ResearchBench>()
                    .Where(bench => currentProj.CanBeResearchedAt(bench, false))
                    .Where(bench => pawn.CanReach(new LocalTargetInfo(bench), PathEndMode.InteractionCell, Danger.Unspecified))
                    .OrderByDescending(bench => bench.GetStatValue(StatDefOf.ResearchSpeedFactor))
                    .ToList();
                benchCache[pawn] = benches;
                return benches;
            }
            return benchCache[pawn];
        }

        /*private List<ThingDef> GetAnalyzables()
        {
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
                return new List<ThingDef>();

            if (analyzablesCachedOnTick != Find.TickManager.TicksGame)
            {
                cachedAnalyzables.Clear();
                
                var analysableThings = MatchingOpportunities.Select(o => (o.requirement as ROComp_RequiresThing).thingDef);

                analyzablesCachedOnTick = Find.TickManager.TicksGame;
                cachedAnalyzables = analysableThings.ToList();
            }

            return cachedAnalyzables;
        }*/
    }
}
