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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers
{
	public enum TerrainLayer { TEMP, TOP, FOUNDATION }

	public class WorkGiver_AnalyseTerrain : WorkGiver_Scanner
	{
		public override bool AllowUnreachable => false;
		public override PathEndMode PathEndMode => PathEndMode.Touch;
		public override bool Prioritized => true;

		public static Type DriverClass = typeof(JobDriver_AnalyseTerrain);


		private static ResearchProjectDef _matchingOpportunitiesCachedFor;
		private static ResearchOpportunity[] _matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		private static IEnumerable<ResearchOpportunity> MatchingOpportunities
		{
			get
			{
				if (_matchingOpportunitiesCachedFor != Find.ResearchManager.GetProject())
				{
					_matchingOpportunitesCache = ResearchOpportunityManager.Instance
						.GetFilteredOpportunities(null, HandlingMode.Job_Analysis, DriverClass).ToArray();
						//.GetCurrentlyAvailableOpportunities(true)
						//.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Job_Analysis) && o.JobDefs != null && o.JobDefs.Any(job => job.driverClass == DriverClass)).ToArray();
					_matchingOpportunitiesCachedFor = Find.ResearchManager.GetProject();
				}
				return _matchingOpportunitesCache;
			}
		}
		public static void ClearMatchingOpportunityCache()
		{
			_matchingOpportunitiesCachedFor = null;
			_matchingOpportunitesCache = Array.Empty<ResearchOpportunity>();
		}


		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			if (Find.ResearchManager.GetProject() == null)
				return Enumerable.Empty<IntVec3>();

			return pawn.Map.areaManager.Home.ActiveCells;//.Where(c => (analysableTerrains.Contains(c.GetTerrain(pawn.Map))));
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.GetProject();
			if (currentProj == null)
				return true;

			return !MatchingOpportunities.Any();
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			ResearchProjectDef currentProj = Find.ResearchManager.GetProject();
			if (currentProj == null)
				return false;

			if (cacheBuiltOnTick != Find.TickManager.TicksAbs)
			{
				BuildCache();
			}

			var opportunity = FindOpportunityAt(pawn, cell);

			if (opportunity == null)
				return false;

			if (currentProj.HasAnyPrerequisites() && !FieldResearchHelper.GetValidResearchKits(pawn, currentProj).Any())
			{
				JobFailReason.Is(StringsCache.JobFail_NeedResearchKit, null);
				return false;
			}

			if (opportunity.Value.opportunity.relation != ResearchRelation.Ancestor)
			{
				var isPrototype = false;
				switch (opportunity.Value.type)
				{
					case TerrainLayer.TEMP:
						break;
					case TerrainLayer.TOP:
						isPrototype = PrototypeKeeper.Instance.IsTerrainPrototype(cell, pawn.Map);
						break;
					case TerrainLayer.FOUNDATION:
						isPrototype = PrototypeKeeper.Instance.IsFoundationTerrainPrototype(cell, pawn.Map);
						break;

				}

				if (isPrototype)
				{
					JobFailReason.Is(StringsCache.JobFail_IsPrototype, null);
					return false;
				}
			}

			return
				!cell.IsForbidden(pawn) &&
				pawn.CanReserve(cell, 1, -1, null, forced) &&
				new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
		{
			var opportunityAt = FindOpportunityAt(pawn, cell);

			if (opportunityAt == null)
			{
				Log.Warning($"JobOnCell could not find opportunity at {cell.ToString()} but HasJobOnCell approved it!");
				return null;
			}

			var jobDef = opportunityAt.Value.opportunity.JobDefs.First(j => j.driverClass == DriverClass);
			Job job = JobMaker.MakeJob(jobDef, cell, expiryInterval: 1500, checkOverrideOnExpiry: true);
			//ResearchOpportunityManager.instance.AssociateJobWithOpportunity(pawn, job, opportunity);
			return job;
		}

		public override float GetPriority(Pawn pawn, TargetInfo target)
		{
			var cell = target.Cell;

            var opportunityAt = FindOpportunityAt(pawn, cell);

            if (opportunityAt == null)
            {
                Log.Warning($"GetPriority could not find opportunity at {cell.ToString()} but HasJobOnCell approved it!");
                return 0;
            }
			var opportunity = opportunityAt.Value.opportunity;

            var dist = cell.DistanceTo(pawn.Position);

			if (dist < 4f)
				dist = (4f - dist) + 4f; //decrease priority for very nearby cells

			var prio = 1f / dist - (4f - 1f); //start at 1, approach 0 at infinity
			prio += Rand.Range(0f, 0.25f); //randomize a bit

			var retVal = prio * pawn.GetStatValue(StatDefOf_Custom.FieldResearchSpeedMultiplier, true) * opportunity.def.GetCategory(opportunity.relation).Settings.researchSpeedMultiplier;

			return retVal;
		}

        public static (TerrainLayer type, ResearchOpportunity opportunity)? FindOpportunityAt(Pawn pawn, IntVec3 cell)
		{
			{
				var tempTerrainAt = pawn.Map.terrainGrid.TempTerrainAt(pawn.Map.cellIndices.CellToIndex(cell));
				if (tempTerrainAt != null)
				{
					var opportunity = FilterCacheFor(tempTerrainAt, pawn)?.FirstOrDefault();
					if (opportunity != null)
						return (TerrainLayer.TEMP, opportunity);
				}
			}
			{
				var topTerrainAt = pawn.Map.terrainGrid.TopTerrainAt(pawn.Map.cellIndices.CellToIndex(cell));
				if (topTerrainAt != null)
				{
					var opportunity = FilterCacheFor(topTerrainAt, pawn)?.FirstOrDefault();
					if (opportunity != null)
						return (TerrainLayer.TOP, opportunity);
				}
			}
			{
				var foundationAt = pawn.Map.terrainGrid.FoundationAt(pawn.Map.cellIndices.CellToIndex(cell));
				if (foundationAt != null)
				{
					var opportunity = FilterCacheFor(foundationAt, pawn)?.FirstOrDefault();
					if (opportunity != null)
						return (TerrainLayer.FOUNDATION, opportunity);
				}
			}
			return null;
		}


		//cache is built once per tick, to avoid working on already finished opportunities or opportunities from a different project
		private static int cacheBuiltOnTick = -1;
		private static Dictionary<TerrainDef, HashSet<ResearchOpportunity>> _opportunityCache = new Dictionary<TerrainDef, HashSet<ResearchOpportunity>>();

		public static Dictionary<TerrainDef, HashSet<ResearchOpportunity>> OpportunityCache
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

		public static void BuildCache()
		{
			_opportunityCache.Clear();
			if (Find.ResearchManager.GetProject() == null)
				return;

			foreach (var opportunity in MatchingOpportunities.Where(o => o.CurrentAvailability == OpportunityAvailability.Available && o.requirement is ROComp_RequiresTerrain))
			{
				var terrainDefs = (opportunity.requirement as ROComp_RequiresTerrain)?.AllTerrains;
				if (terrainDefs == null || terrainDefs.Length == 0)
				{
					Log.ErrorOnce($"RR: current research project {Find.ResearchManager.GetProject()} generated a WorkGiver_Analyze opportunity with null or empty requirement!", Find.ResearchManager.GetProject().debugRandomId);
					continue;
				}
				foreach(var terrainDef in terrainDefs)
				{
					if (!_opportunityCache.ContainsKey(terrainDef))
						_opportunityCache[terrainDef] = new HashSet<ResearchOpportunity>();

					_opportunityCache[terrainDef].Add(opportunity);
				}
			}
			cacheBuiltOnTick = Find.TickManager.TicksAbs;
			//Log.Message("built terrain opportunity cache on tick " + cacheBuiltOnTick);
		}

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static HashSet<ResearchOpportunity> FilterCacheFor(TerrainDef terrainDef, Pawn pawn)
        {
            return OpportunityCache.TryGetValue(terrainDef);
        }
    }
}
