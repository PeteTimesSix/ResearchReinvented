using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes;
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
		public static ResearchProjectDef cacheBuiltForProject = null; 
		public static Dictionary<BuildableDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = new Dictionary<BuildableDef, ResearchOpportunity>();

		public static Dictionary<BuildableDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
        {
            get
			{
				if (cacheBuiltForProject != Find.ResearchManager.currentProj)
				{
					_prototypeOpportunitiesMappedCache.Clear();
                    foreach(var op in PrototypeUtilities.PrototypeOpportunities)
                    {
                        if (op.requirement is ROComp_RequiresThing requiresThing && requiresThing.thingDef is BuildableDef buildableDef)
                            _prototypeOpportunitiesMappedCache[buildableDef] = op;
                    }
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
                    return false;

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

		/*public static ResearchOpportunity FindPrototypeOpportunity(this BuildableDef def) 
		{
			return PrototypeUtilities.PrototypeOpportunities.FirstOrDefault(o => o.requirement.MetBy(def));
		}*/
	}
}
