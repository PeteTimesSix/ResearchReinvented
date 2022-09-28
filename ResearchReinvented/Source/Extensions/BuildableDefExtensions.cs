using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
	public static class BuildableDefExtensions
	{
		private static IEnumerable<ResearchOpportunity> PrototypeOpportunities => ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities(true).Where(o => o.def.handledBy == HandlingMode.Special_Prototype);

		public static int cacheBuiltOnTick = -1; 
		private static List<ResearchOpportunity> _prototypeOpportunitiesCache = new List<ResearchOpportunity>();
		private static IEnumerable<ResearchOpportunity> PrototypeOpportunitiesCached 
		{
            get 
			{
				if (cacheBuiltOnTick != Current.Game.tickManager.TicksAbs)
				{
					_prototypeOpportunitiesCache.Clear();
					_prototypeOpportunitiesCache = PrototypeOpportunities.ToList();
					cacheBuiltOnTick = Current.Game.tickManager.TicksAbs;
				}
				return _prototypeOpportunitiesCache;
			}
		}

		public static bool IsAvailableOnlyForPrototyping(this BuildableDef def)
		{
			if (def.researchPrerequisites != null)
			{
				var unfinishedPreregs = def.researchPrerequisites.Where((ResearchProjectDef r) => !r.IsFinished);
				if (!unfinishedPreregs.Any())
					return false;
				if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.currentProj != r))
					return false;

				return PrototypeOpportunitiesCached.Any(o => o.requirement.MetBy(def));
			}
			return false;
		}
	}
}
