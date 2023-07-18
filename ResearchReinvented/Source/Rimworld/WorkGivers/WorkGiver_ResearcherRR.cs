using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Rimworld.Jobs;
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
	public class WorkGiver_ResearcherRR : WorkGiver_Scanner
	{
		public static Type DriverClass = typeof(JobDriver_ResearchRR);

		private static ResearchProjectDef _matchingOpportunitiesCachedFor;
		private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		public static IEnumerable<ResearchOpportunity> MatchingOpportunities
		{
			get
			{
				if (_matchingOpportunitiesCachedFor != Find.ResearchManager.currentProj)
				{
					_matchingOpportunitesCache = ResearchOpportunityManager.Instance
						.GetFilteredOpportunities(null, HandlingMode.Job_Theory, DriverClass).ToArray();
						//.GetCurrentlyAvailableOpportunities(true)
						//.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Theory) && o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass)).ToArray();
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
				return ThingRequest.ForGroup(ThingRequestGroup.ResearchBench);
			}
		}

		public override bool Prioritized
		{
			get
			{
				return true;
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return Find.ResearchManager.currentProj == null;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
			{
				return false;
			}
			var opportunity = MatchingOpportunities.FirstOrDefault();
			if (opportunity == null)
			{
				//Log.Warning("found no research opportunities when looking for a research job on a research bench => the basic research should always be available!");
				return false;
			}
			Building_ResearchBench building_ResearchBench = t as Building_ResearchBench;
			return
				building_ResearchBench != null &&
				opportunity != null &&
				currentProj.CanBeResearchedAt(building_ResearchBench, false) &&
				!building_ResearchBench.IsForbidden(pawn) &&
				pawn.CanReserve(t, 1, -1, null, forced) && 
				(!t.def.hasInteractionCell || pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)) && 
				new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			var opportunity = MatchingOpportunities.FirstOrDefault();

			var jobDef = opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, t, expiryInterval: 3000, checkOverrideOnExpiry: true);
			//ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			return job;
		}

		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			return t.Thing.GetStatValue(StatDefOf.ResearchSpeedFactor, true);
		}

		private static int cacheBuiltOnTick = -1;
		private static ResearchOpportunity _opportunityCache;

		public static ResearchOpportunity OpportunityCache
		{
			get
			{
				if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
				{
					_opportunityCache = MatchingOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available).FirstOrDefault();
				}
				return _opportunityCache;
			}
		}
	}
}
