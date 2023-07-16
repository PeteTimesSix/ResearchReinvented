using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
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

	public class WorkGiver_AnalyseInPlace : WorkGiver_Scanner
	{
		public override bool Prioritized => true;

		public static Type DriverClass = typeof(JobDriver_AnalyseInPlace);

		private static ResearchProjectDef _matchingOpportunitiesCachedFor;
		private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		private static IEnumerable<ResearchOpportunity> MatchingOpportunities
		{
			get
			{
				if (_matchingOpportunitiesCachedFor != Find.ResearchManager.currentProj)
				{
					_matchingOpportunitesCache = ResearchOpportunityManager.Instance.GetCurrentlyAvailableOpportunitiesFiltered(true, HandlingMode.Job_Analysis, DriverClass).ToArray();
						//.GetCurrentlyAvailableOpportunities(true)
						//.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Analysis) && o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass)).ToArray();
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
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return true;

            if(Find.ResearchManager.currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, Find.ResearchManager.currentProj).Any())
            {
                JobFailReason.Is(StringsCache.JobFail_NeedResearchKit, null);
                return false;
            }

			return !MatchingOpportunities.Any(o => !o.IsFinished);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
            // Dont actually do this. We want minified things to be examined at benches.
            //var unminifiedThing = thing.GetInnerIfMinified();
            //var thingDef = unminifiedThing.def;

            if (thing.IsForbidden(pawn))
                return false;

            if (!pawn.CanReserve(thing, 1, -1, null, forced))
                return false;

            if (PrototypeKeeper.Instance.IsPrototype(thing))
            {
                JobFailReason.Is(StringsCache.JobFail_IsPrototype, null);
                return false;
            }

            if (thing.def.hasInteractionCell)
            {
                if (!pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced))
                    return false;
            }
            else
            {
                var reachable = AdjacencyHelper.GenReachableAdjacentCells(thing, pawn, true);
                if (!reachable.Any())
                    return false;
            }

            return new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
        }

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			var opportunity = OpportunityCache[thing.def].First();

			var jobDef = opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, thing, expiryInterval: 1500, checkOverrideOnExpiry: true);
			//ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			return job;
		}

		/*public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var opportunity = OpportunityCache[target.Thing.def].First();

			return pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;
		}*/

		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		private static int cacheBuiltOnTick = -1;
		private static Dictionary<ThingDef, HashSet<ResearchOpportunity>> _opportunityCache = new Dictionary<ThingDef, HashSet<ResearchOpportunity>>();
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

        public static void BuildCache()
		{
			_opportunityCache.Clear();
            _things.Clear();

            if (Find.ResearchManager.currentProj == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available && o.requirement is ROComp_RequiresThing))
			{
				var thingDef = (opportunity.requirement as ROComp_RequiresThing)?.thingDef;
				if (thingDef == null)
				{
					Log.ErrorOnce($"RR: current research project {Find.ResearchManager.currentProj} generated a WorkGiver_AnalyzeInPlace opportunity with null requirement!", Find.ResearchManager.currentProj.debugRandomId);
					continue;
				}
				if (!_opportunityCache.ContainsKey(thingDef))
					_opportunityCache[thingDef] = new HashSet<ResearchOpportunity>();

				_opportunityCache[thingDef].Add(opportunity);
            }

            var defsToFind = _opportunityCache.Keys.ToList();

            foreach (var map in Find.Maps)
            {
                var list = new List<Thing>();

                _things[map] = list;
                foreach (var thingDef in defsToFind)
                {
                    var things = map.listerThings.ThingsOfDef(thingDef);
                    foreach (var thing in things)
                    {
                        if (!thing.FactionAllowsAnalysis())
                            continue;

                        list.Add(thing);
                    }

                }
            }

            cacheBuiltOnTick = Find.TickManager.TicksAbs;
		}
	}
}
