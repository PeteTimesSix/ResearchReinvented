using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.ModCompat;
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
					_matchingOpportunitesCache = ResearchOpportunityManager.Instance
						.GetCurrentlyAvailableOpportunities(true, HandlingMode.Job_Analysis, DriverClass).ToArray();
						//.GetCurrentlyAvailableOpportunities(true)
                        //.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Analysis) & o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass)).ToArray();
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

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (Find.ResearchManager.currentProj == null)
                return Enumerable.Empty<Thing>();

            return ThingsForMap(pawn.MapHeld);
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (Find.ResearchManager.currentProj == null)
				return true;

			return !MatchingOpportunities.Any(o => !o.IsFinished);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn))
                return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;

			if (PrototypeKeeper.Instance.IsPrototype(t))
            {
                JobFailReason.Is(StringsCache.JobFail_IsPrototype, null);
                return false;
            }

            var researchBench = GetBestResearchBench(pawn);
			if (researchBench == null)
			{
				JobFailReason.Is(StringsCache.JobFail_NeedResearchBench, null);
				return false;
			}

			if (!(pawn.CanReserveSittableOrSpot(researchBench.InteractionCell, forced) &&
				new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job()))
				return false;

			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var researchBench = GetBestResearchBench(pawn);

			var thingDef = MinifyUtility.GetInnerIfMinified(t).def;

			if(!OpportunityCache.ContainsKey(thingDef)) 
			{
				Log.Warning($"opportunity cache did not contain {thingDef} for thing {t}");
				return null;
			}

			var opportunity = OpportunityCache[thingDef].First();

			JobDef jobDef = opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, t, expiryInterval: 20000, checkOverrideOnExpiry: true);
			job.targetB = researchBench;
			//ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			job.count = 1;
			return job;
		}

		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		public static int cacheBuiltOnTick = -1;
		public static Dictionary<Map, List<Building_ResearchBench>> _goodBenchCache = new Dictionary<Map, List<Building_ResearchBench>>();
		public static Dictionary<Pawn, Building_ResearchBench> _benchCache = new Dictionary<Pawn, Building_ResearchBench>();
		public static Dictionary<ThingDef, HashSet<ResearchOpportunity>> _opportunityCache = new Dictionary<ThingDef, HashSet<ResearchOpportunity>>();
		public static Dictionary<Map, List<Thing>> _things = new Dictionary<Map, List<Thing>>();

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

		public static List<Thing> ThingsForMap(Map map) 
		{
			if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
			{
				BuildCache();
			}
			return _things[map];
        }

        public Building_ResearchBench GetBestResearchBench(Pawn pawn)
        {
            if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
            {
                BuildCache();
            }

            if (!_benchCache.ContainsKey(pawn))
            {
                var benches = _goodBenchCache[pawn.MapHeld];
				bool found = false;
                foreach (var researchBench in benches)
                {
                    if (researchBench.IsForbidden(pawn))
                        continue;

                    if (!pawn.CanReserveAndReach(new LocalTargetInfo(researchBench), PathEndMode.InteractionCell, Danger.Unspecified))
                        continue;

					if (ResearchData.active && ResearchReinventedMod.Settings.researchDataCompatMode == ResearchData.ResearchDataCompatMode.AllBenchResearch)
					{
						if (researchBench.TryGetComp<CompRefuelable>() is CompRefuelable comp && comp.HasFuel is false)
							continue;
					}

					found = true;
                    _benchCache[pawn] = researchBench;
                    break;
                }
				if (!found)
					_benchCache[pawn] = null;
            }

            return _benchCache[pawn];
        }

        public static void BuildCache()
		{
			_goodBenchCache.Clear();
			_benchCache.Clear();
			_opportunityCache.Clear();
			_things.Clear();

			if (Find.ResearchManager.currentProj == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available && o.requirement is ROComp_RequiresThing))
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

			foreach (var map in Find.Maps) 
			{
				{
                    var list = new List<Thing>();
                    _things[map] = list;
                    foreach (var thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                    {
                        if (!thing.FactionAllowsAnalysis())
                            continue;

                        var unminifiedThing = thing.GetInnerIfMinified();
                        var thingDef = unminifiedThing.def;

                        if (_opportunityCache.ContainsKey(thingDef))
                            list.Add(thing);
                    }
                }
                {
                    ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
                    var list = new List<Building_ResearchBench>();
                    foreach (var benchThing in map.listerThings.ThingsInGroup(ThingRequestGroup.ResearchBench))
                    {
                        if (!(benchThing is Building_ResearchBench researchBench))
                            continue;

                        if (!currentProj.CanBeResearchedAt(researchBench, false))
                            continue;

                        list.Add(researchBench);
                    }
					_goodBenchCache[map] = list;
                }
			}

			cacheBuiltOnTick = Find.TickManager.TicksAbs;
		}

	}
}
