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
		public static IEnumerable<ResearchOpportunity> PrototypeOpportunities => ResearchOpportunityManager.instance.AllCurrentOpportunities/*.GetCurrentlyAvailableOpportunities(true)*/.Where(o => o.def.handledBy == HandlingMode.Special_Prototype);

		public static ResearchProjectDef cacheBuiltForProject = null; 
		public static List<ResearchOpportunity> _prototypeOpportunitiesCache = new List<ResearchOpportunity>();
		public static Dictionary<BuildableDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = new Dictionary<BuildableDef, ResearchOpportunity>();
		public static IEnumerable<ResearchOpportunity> PrototypeOpportunitiesCached 
		{
            get 
			{
				if (cacheBuiltForProject != Find.ResearchManager.currentProj)
				{
					_prototypeOpportunitiesCache.Clear();
					_prototypeOpportunitiesMappedCache.Clear();
					_prototypeOpportunitiesCache = PrototypeOpportunities.ToList();
					cacheBuiltForProject = Find.ResearchManager.currentProj;
				}
				return _prototypeOpportunitiesCache;
			}
		}

		public static Dictionary<BuildableDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
        {
            get
			{
				if (cacheBuiltForProject != Find.ResearchManager.currentProj)
				{
					_prototypeOpportunitiesCache.Clear();
					_prototypeOpportunitiesMappedCache.Clear();
					_prototypeOpportunitiesCache = PrototypeOpportunities.ToList();
					cacheBuiltForProject = Find.ResearchManager.currentProj;
				}
				return _prototypeOpportunitiesMappedCache;
			}
        }

		public static bool IsAvailableOnlyForPrototyping(this BuildableDef def, bool evenIfFinished)
		{
			if (!PrototypeOpportunitiesMappedCache.ContainsKey(def))
			{
				PrototypeOpportunitiesMappedCache[def] = FindPrototypeOpportunity(def);
			}
			var opportunity = PrototypeOpportunitiesMappedCache[def];
			//Log.Message("all non-null opportunities: " + string.Join(",", PrototypeOpportunitiesMappedCache.Where(kv => kv.Value != null).Select((kv) => kv.Key.ToString() + " " + kv.Value?.ToString())));
			//Log.Message("opportunity: " + opportunity);
			if (opportunity == null)
				return false;
			if (!evenIfFinished)
				return opportunity.CurrentAvailability == OpportunityAvailability.Available;
			else
				return opportunity.CurrentAvailability == OpportunityAvailability.Available || opportunity.CurrentAvailability == OpportunityAvailability.Finished || opportunity.CurrentAvailability == OpportunityAvailability.CategoryFinished;
		}

		public static ResearchOpportunity FindPrototypeOpportunity(this BuildableDef def) 
		{
			/*if (def.researchPrerequisites == null)
				return null;

			var unfinishedPreregs = def.researchPrerequisites.Where((ResearchProjectDef r) => !r.IsFinished);
			if (!unfinishedPreregs.Any())
				return null;
			if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.currentProj != r))
				return null;*/

			return PrototypeOpportunitiesCached.FirstOrDefault(o => o.requirement.MetBy(def));
		}
	}
}
