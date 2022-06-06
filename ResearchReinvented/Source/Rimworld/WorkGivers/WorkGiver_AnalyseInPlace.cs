using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using PeteTimesSix.ResearchReinvented.Rimworld.MiscData;
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

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
			var analysableThings = opportunities.Select(o => (o.requirement as ROComp_RequiresThing).targetDef);
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

			/*Log.Message("has prereqs: "+ currentProj.HasAnyPrerequisites());
			Log.Message("found kits: " + FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Count());
			foreach(var kit in FieldResearchHelper.GetValidResearchKits(pawn, currentProj))
				Log.Message("kit: " + kit.LabelCap);*/

			if (currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Any())
			{
				//Log.Message("skipped due to lack of kit");
				return true;
			}

			var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
			var analysableThings = opportunities.Select(o => (o.requirement as ROComp_RequiresThing).targetDef);
			return !analysableThings.Any();
		}

		public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return false;

			var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
			var opportunity = opportunities.FirstOrDefault(o => (o.requirement as ROComp_RequiresThing).targetDef == thing.def);
			if (opportunity == null)
				return false;
			else
				return
					pawn.CanReserve(thing, 1, -1, null, forced) &&
					(!thing.def.hasInteractionCell || pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)) &&
					new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
			var opportunity = opportunities.FirstOrDefault(o => (o.requirement as ROComp_RequiresThing).targetDef == thing.def);
			if (opportunity == null)
			{
				//Log.Warning($"Found a job at {thing.Label} but then could not create it!");
				return null;
			}
			else
			{
				Job job = JobMaker.MakeJob(opportunity.def.jobDef, thing, expiryInterval: 1500, checkOverrideOnExpiry: true);
				ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
				return job;
			}
		}

		public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var opportunities = ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.jobDef.driverClass == DriverClass).Where(o => !o.IsFinished);
			var opportunity = opportunities.First(o => (o.requirement as ROComp_RequiresThing).targetDef == target.Thing.def);

			return pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).researchSpeedMultiplier;
		}
	}
}
