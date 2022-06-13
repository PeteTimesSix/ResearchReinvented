using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
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

	public class WorkGiver_AnalyseTerrain : WorkGiver_Scanner
	{
		public override bool AllowUnreachable => false;
		public override PathEndMode PathEndMode => PathEndMode.Touch;
		public override bool Prioritized => true;

        public static Type DriverClass = typeof(JobDriver_AnalyseTerrain);

		private static IEnumerable<ResearchOpportunity> MatchingOpportunities => ResearchOpportunityManager.instance.CurrentOpportunities.Where(o => o.def.handledBy == HandlingMode.Job && o.def.jobDef?.driverClass == DriverClass).Where(o => !o.IsFinished);

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			var analysableTerrains = MatchingOpportunities.Select(o => (o.requirement as ROComp_RequiresTerrain).terrainDef);
			return pawn.Map.areaManager.Home.ActiveCells.Where(c => (analysableTerrains.Contains(c.GetTerrain(pawn.Map))));
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return true;

			if (currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Any())
				return true;

			var analysableTerrains = MatchingOpportunities.Select(o => (o.requirement as ROComp_RequiresTerrain).terrainDef);
			return !analysableTerrains.Any();
		}

        public override bool HasJobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return false;

			var opportunity = MatchingOpportunities.FirstOrDefault(o => (o.requirement as ROComp_RequiresTerrain).terrainDef == cell.GetTerrain(pawn.Map));
			if (opportunity == null)
				return false;
			else
				return
					pawn.CanReserve(cell, 1, -1, null, forced) &&
					new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			var opportunity = MatchingOpportunities.FirstOrDefault(o => (o.requirement as ROComp_RequiresTerrain).terrainDef == cell.GetTerrain(pawn.Map));
			if (opportunity == null)
			{
				//Log.Warning($"Found a job at {thing.Label} but then could not create it!");
				return null;
			}
			else
			{
				Job job = JobMaker.MakeJob(opportunity.def.jobDef, cell, expiryInterval: 1500, checkOverrideOnExpiry: true);
				ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
				return job;
			}
		}

		public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var opportunity = MatchingOpportunities.First(o => (o.requirement as ROComp_RequiresTerrain).terrainDef == target.Cell.GetTerrain(pawn.Map));

			var cell = target.Cell;
			var dist = cell.DistanceTo(pawn.Position);

			if (dist < 4f)
				dist = (4f - dist) + 4f; //decrease priority for very nearby cells

			var prio = 1f / dist - (4f - 1f); //start at 1, approach 0 at infinity
			prio += Rand.Range(0f, 0.25f); //randomize a bit

			return prio * pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).researchSpeedMultiplier;
		}
	}
}
