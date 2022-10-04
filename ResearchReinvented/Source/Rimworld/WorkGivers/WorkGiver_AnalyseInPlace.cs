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

		private static IEnumerable<ResearchOpportunity> MatchingOpportunities => ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities().Where(o => o.def.handledBy == HandlingMode.Job && o.def.jobDef?.driverClass == DriverClass);

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			var analysableThings = MatchingOpportunities.Select(o => (o.requirement as ROComp_RequiresThing).thingDef);
			var lister = pawn.Map.listerThings;

			List<Thing> things = new List<Thing>();
			foreach(var def in analysableThings)
			{
				things.AddRange(lister.ThingsOfDef(def).Where(t => !t.IsForbidden(pawn)));
			}

			return things;
        }

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
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
			else
			{
				if (!pawn.CanReserve(thing, 1, -1, null, forced))
					return false;

				if (thing.def.hasInteractionCell)
				{
					if (!pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced))
						return false;
				}
				else
				{
					var reachable = AdjacencyHelper.GenReachableAdjacentCells(thing, pawn);
					if (!reachable.Any())
						return false;
				}
				
				if (currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Any())
				{
					JobFailReason.Is("RR_jobFail_needResearchKit".Translate(), null);
					return false;
				}
				else
					return new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
			{
				BuildCache();
			}

			var opportunity = opportunityCache[thing.def].First();

			Job job = JobMaker.MakeJob(opportunity.def.jobDef, thing, expiryInterval: 1500, checkOverrideOnExpiry: true);
			ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			return job;
		}

		public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var opportunity = opportunityCache[target.Thing.def].First();

			return pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).researchSpeedMultiplier;
		}

		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		public static int cacheBuiltOnTick = -1;
		public static Dictionary<ThingDef, HashSet<ResearchOpportunity>> opportunityCache = new Dictionary<ThingDef, HashSet<ResearchOpportunity>>();

		public static void BuildCache()
		{
			opportunityCache.Clear();
			if (Find.ResearchManager.currentProj == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => o.requirement is ROComp_RequiresThing))
			{
				var thingDef = (opportunity.requirement as ROComp_RequiresThing).thingDef;
				if (!opportunityCache.ContainsKey(thingDef))
					opportunityCache[thingDef] = new HashSet<ResearchOpportunity>();

				opportunityCache[thingDef].Add(opportunity);
			}

			cacheBuiltOnTick = Find.TickManager.TicksAbs;
		}
	}
}
