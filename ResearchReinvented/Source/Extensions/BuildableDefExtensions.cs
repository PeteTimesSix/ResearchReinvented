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
        public static Dictionary<BuildableDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = null;

        public static Dictionary<BuildableDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
        {
            get
			{
				if (_prototypeOpportunitiesMappedCache == null)
				{
                    _prototypeOpportunitiesMappedCache = new();
                    foreach (var op in PrototypeUtilities.PrototypeOpportunities)
                    {
                        if ((op.requirement is ROComp_RequiresThing requiresThing && requiresThing.thingDef is BuildableDef buildableDef1))
                            _prototypeOpportunitiesMappedCache[buildableDef1] = op;
                        else if((op.requirement is ROComp_RequiresTerrain requiresTerrain && requiresTerrain.terrainDef is BuildableDef buildableDef2))
                            _prototypeOpportunitiesMappedCache[buildableDef2] = op;
                    }
				}
				return _prototypeOpportunitiesMappedCache;
			}
        }

        public static void ClearPrototypeOpportunityCache()
        {
            _prototypeOpportunitiesMappedCache = null;
        }

        public static bool IsAvailableOnlyForPrototyping(this BuildableDef def, bool evenIfFinished)
        {
            if (def.researchPrerequisites != null && def.researchPrerequisites.Count > 0)
            {
                var unfinishedPreregs = def.researchPrerequisites.Where((ResearchProjectDef r) => !r.IsFinished);
                if (!unfinishedPreregs.Any())
                    return false;
                //if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.GetProject() != r))
                //    return false;
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
	}
}
