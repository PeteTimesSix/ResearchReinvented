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
		public static IEnumerable<ResearchOpportunity> PrototypeOpportunities => ResearchOpportunityManager.Instance.CurrentProjectOpportunities.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_Prototype));

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
            if (def.researchPrerequisites != null && def.researchPrerequisites.Count > 0)
            {
                var unfinishedPreregs = def.researchPrerequisites.Where((ResearchProjectDef r) => !r.IsFinished);
                if (!unfinishedPreregs.Any())
                    return false;
                if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.currentProj != r))
                    return false;

                if (!PrototypeOpportunitiesMappedCache.ContainsKey(def))
                {
                    PrototypeOpportunitiesMappedCache[def] = FindPrototypeOpportunity(def);
                }
                var opportunity = PrototypeOpportunitiesMappedCache[def];

                if (opportunity == null)
                    return false;
                if (!evenIfFinished)
                    return opportunity.CurrentAvailability == OpportunityAvailability.Available;
                else
                    return opportunity.CurrentAvailability == OpportunityAvailability.Available || opportunity.CurrentAvailability == OpportunityAvailability.Finished || opportunity.CurrentAvailability == OpportunityAvailability.CategoryFinished;
            }
            return false;


		}

		public static ResearchOpportunity FindPrototypeOpportunity(this BuildableDef def) 
		{
			return PrototypeOpportunitiesCached.FirstOrDefault(o => o.requirement.MetBy(def));
		}
	}
}
