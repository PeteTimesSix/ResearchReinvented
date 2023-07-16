using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class ThingDefExtensions
    {
        public static IEnumerable<ResearchOpportunity> PrototypeOpportunities => ResearchOpportunityManager.Instance.CurrentProjectOpportunities.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_Prototype));

        public static ResearchProjectDef cacheBuiltForProject = null;
        public static List<ResearchOpportunity> _prototypeOpportunitiesCache = new List<ResearchOpportunity>();
        public static Dictionary<ThingDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = new Dictionary<ThingDef, ResearchOpportunity>();
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

        public static Dictionary<ThingDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
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

        public static bool IsTrulyRawFood(this ThingDef x)
        {
            return x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && x.ingestible.preferability < FoodPreferability.MealAwful;
        }

        public static bool IsInstantBuild(this ThingDef x)
        {
            if (x is BuildableDef asBuildable)
            {
                if (asBuildable.GetStatValueAbstract(StatDefOf.WorkToBuild) == 0f)
                    return true;
            }
            return false;
        }

        public static bool IsAvailableOnlyForPrototyping(this ThingDef def)
		{
			if (def.researchPrerequisites != null)
			{
                var unfinishedPreregs = def.researchPrerequisites.Where((ResearchProjectDef r) => !r.IsFinished);
				if (!unfinishedPreregs.Any())
					return false;
				if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.currentProj != r))
					return false;

				return true;
			}
			return false;
		}

        public static bool IsAvailableOnlyForPrototyping(this ThingDef def, bool evenIfFinished = true)
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
        public static ResearchOpportunity FindPrototypeOpportunity(this ThingDef def)
        {
            return PrototypeOpportunitiesCached.FirstOrDefault(o => o.requirement.MetBy(def));
        }
    }
}
