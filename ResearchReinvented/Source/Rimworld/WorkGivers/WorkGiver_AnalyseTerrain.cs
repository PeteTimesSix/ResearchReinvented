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

		private static IEnumerable<ResearchOpportunity> MatchingOpportunities =>
			ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities()
			.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Analysis) && o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass));


		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			//var analysableTerrains = MatchingOpportunities.Select(o => (o.requirement as ROComp_RequiresTerrain).terrainDef);
			return pawn.Map.areaManager.Home.ActiveCells;//.Where(c => (analysableTerrains.Contains(c.GetTerrain(pawn.Map))));
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return true;

			if (currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Any())
				return true;

			return !MatchingOpportunities.Any();
		}

        public override bool HasJobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
			ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
			if (currentProj == null)
				return false;

			if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
			{
				BuildCache();
			}

			var terrainAt = cell.GetTerrain(pawn.Map);
			if (!opportunityCache.ContainsKey(terrainAt))
				return false;
			//var opportunity = opportunityCache[terrainAt].First();
			/*var opportunity = opportunityCache[terrainAt].FirstOrDefault(o => o.CurrentAvailability == OpportunityAvailability.Available);
			if (opportunity == null)
				return false;
			else*/
			return
				!cell.IsForbidden(pawn) &&
				pawn.CanReserve(cell, 1, -1, null, forced) &&
				new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			if(cacheBuiltOnTick != Find.TickManager.TicksAbs)
            {
				BuildCache();
            }

			var terrainAt = cell.GetTerrain(pawn.Map);
			var opportunity = opportunityCache[terrainAt].First();
			//var opportunity = opportunityCache[terrainAt].FirstOrDefault(o => o.CurrentAvailability == OpportunityAvailability.Available);
			//var opportunity = MatchingOpportunities.FirstOrDefault(o => o.requirement.MetBy(terrainAt));//MatchingOpportunities.FirstOrDefault(o => (o.requirement as ROComp_RequiresTerrain).terrainDef == cell.GetTerrain(pawn.Map));

			var jobDef = opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, cell, expiryInterval: 1500, checkOverrideOnExpiry: true);
			ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			return job;
		}

		public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var terrainAt = target.Cell.GetTerrain(pawn.Map);
			var opportunity = opportunityCache[terrainAt].First();
			//var opportunity = MatchingOpportunities.First(o => o.requirement.MetBy(terrainAt));

			var cell = target.Cell;
			var dist = cell.DistanceTo(pawn.Position);

			if (dist < 4f)
				dist = (4f - dist) + 4f; //decrease priority for very nearby cells

			var prio = 1f / dist - (4f - 1f); //start at 1, approach 0 at infinity
			prio += Rand.Range(0f, 0.25f); //randomize a bit

			return prio * pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;
		}


		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		public static int cacheBuiltOnTick = -1;
		public static Dictionary<TerrainDef, HashSet<ResearchOpportunity>> opportunityCache = new Dictionary<TerrainDef, HashSet<ResearchOpportunity>>();

		public static void BuildCache()
		{
			opportunityCache.Clear();
			if (Find.ResearchManager.currentProj == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => o.requirement is ROComp_RequiresTerrain))
			{
				var terrainDef = (opportunity.requirement as ROComp_RequiresTerrain)?.terrainDef;
				if (terrainDef == null)
				{
					Log.ErrorOnce($"RR: current research project {Find.ResearchManager.currentProj} generated a WorkGiver_Analyze opportunity with null requirement!", Find.ResearchManager.currentProj.debugRandomId);
					continue;
				}
				if (!opportunityCache.ContainsKey(terrainDef))
					opportunityCache[terrainDef] = new HashSet<ResearchOpportunity>();

				opportunityCache[terrainDef].Add(opportunity);
			}
			/*foreach(var terrain in opportunityCache.Keys) 
			{
				Log.Warning($"{terrain.defName} maps to: {string.Join(", ", opportunityCache[terrain].Select(o => $"{o.ToString()} {o.CurrentAvailability}"))}");
			}*/
			cacheBuiltOnTick = Find.TickManager.TicksAbs;
			//Log.Message("built terrain opportunity cache on tick " + cacheBuiltOnTick);
		}
	}
}
