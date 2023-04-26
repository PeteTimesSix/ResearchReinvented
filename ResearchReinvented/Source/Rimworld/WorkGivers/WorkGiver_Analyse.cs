using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private static ResearchProjectDef _matchingOpportunitiesCachedFor;
		private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		public static IEnumerable<ResearchOpportunity> MatchingOpportunities
		{
			get
			{
				if (_matchingOpportunitiesCachedFor != Find.ResearchManager.currentProj)
				{
					_matchingOpportunitesCache = ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities(true)
						.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Analysis) & o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass)).ToArray();
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

			return !MatchingOpportunities.Any(o => !o.IsFinished);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return false;

			if (!thing.FactionAllowsAnalysis())
				return false;

			var unminifiedThing = thing.GetInnerIfMinified();
			var thingDef = unminifiedThing.def;

			if (!OpportunityCache.ContainsKey(thingDef))
				return false;

			var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));
			if (!researchBenches.Any())
			{
				JobFailReason.Is(StringsCache.JobFail_NeedResearchBench, null);
				return false;
			}
			var bestBench = researchBenches.First();

			if (!(pawn.CanReserveSittableOrSpot(bestBench.InteractionCell, forced) &&
				pawn.CanReserve(thing, 1, -1, null, forced) &&
				new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job()))
				return false;

			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			var researchBenches = GetUsableResearchBenches(pawn).Where(bench => pawn.CanReserve(bench));
			var bestBench = researchBenches.First();

			var thingDef = MinifyUtility.GetInnerIfMinified(thing).def;
			var opportunity = OpportunityCache[thingDef].First();

			JobDef jobDef = opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, thing, expiryInterval: 20000, checkOverrideOnExpiry: true);
			job.targetB = bestBench;
			//ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			job.count = 1;
			return job;
		}

		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		public static int cacheBuiltOnTick = -1;
		public static Dictionary<Pawn, List<Building_ResearchBench>> _benchCache = new Dictionary<Pawn, List<Building_ResearchBench>>();
		public static Dictionary<ThingDef, HashSet<ResearchOpportunity>> _opportunityCache = new Dictionary<ThingDef, HashSet<ResearchOpportunity>>();

		public static Dictionary<ThingDef, HashSet<ResearchOpportunity>> OpportunityCache
		{
			get
			{
				if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
				{
					BuildCache();
				}
				return _opportunityCache;
			}
		}
		public static Dictionary<Pawn, List<Building_ResearchBench>> BenchCache
		{
			get
			{
				if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
				{
					BuildCache();
				}
				return _benchCache;
			}
		}

		public static void BuildCache()
		{
			_benchCache.Clear();
			_opportunityCache.Clear();
			if (Find.ResearchManager.currentProj == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => !o.IsFinished && o.requirement is ROComp_RequiresThing))
			{
				var thingDef = (opportunity.requirement as ROComp_RequiresThing)?.thingDef;
				if (thingDef == null)
				{
					Log.ErrorOnce($"RR: current research project {Find.ResearchManager.currentProj} generated a WorkGiver_Analyze opportunity with null requirement!", Find.ResearchManager.currentProj.debugRandomId);
					continue;
				}
				if (!_opportunityCache.ContainsKey(thingDef))
					_opportunityCache[thingDef] = new HashSet<ResearchOpportunity>();

				_opportunityCache[thingDef].Add(opportunity);
			}

			cacheBuiltOnTick = Find.TickManager.TicksAbs;
		}


		public List<Building_ResearchBench> GetUsableResearchBenches(Pawn pawn)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return new List<Building_ResearchBench>();

			if (!_benchCache.ContainsKey(pawn))
			{
				var benches = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.ResearchBench)
					.Cast<Building_ResearchBench>()
					.Where(bench => currentProj.CanBeResearchedAt(bench, false))
					.Where(bench => !bench.IsForbidden(pawn) && pawn.CanReach(new LocalTargetInfo(bench), PathEndMode.InteractionCell, Danger.Unspecified))
					.OrderByDescending(bench => bench.GetStatValue(StatDefOf.ResearchSpeedFactor))
					.ToList();
				_benchCache[pawn] = benches;
				return benches;
			}
			return _benchCache[pawn];
		}
	}
}
