using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class RecipeDefExtensions
	{
		public static Dictionary<RecipeDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = null;

		public static Dictionary<RecipeDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
		{
			get
			{
				if (_prototypeOpportunitiesMappedCache == null)
				{
					_prototypeOpportunitiesMappedCache = new();
                    foreach (var op in PrototypeUtilities.PrototypeOpportunities)
                    {
                        if (op.requirement is ROComp_RequiresRecipe requiresRecipe)
                            _prototypeOpportunitiesMappedCache[requiresRecipe.recipeDef] = op;
                    }
				}
				return _prototypeOpportunitiesMappedCache;
			}
        }

        public static void ClearPrototypeOpportunityCache()
        {
			_prototypeOpportunitiesMappedCache = null;
        }

        public static HashSet<ResearchProjectDef> AllResearchPrerequisites(this RecipeDef recipe)
		{
			HashSet<ResearchProjectDef> prerequisites = new HashSet<ResearchProjectDef>();
			if (recipe.researchPrerequisite != null)
				prerequisites.Add(recipe.researchPrerequisite);
			if (recipe.researchPrerequisites != null)
				prerequisites.AddRange(recipe.researchPrerequisites);
			return prerequisites;
		}

        public static bool IsAvailableOnlyForPrototyping(this RecipeDef def, bool evenIfFinished = true)
        {
			var preregs = new List<ResearchProjectDef>();
			if(def.researchPrerequisite != null)
				preregs.Add(def.researchPrerequisite);
			if(def.researchPrerequisites != null)
				preregs.AddRange(def.researchPrerequisites);

            if (preregs.Count > 0)
            {
                var unfinishedPreregs = preregs.Where((ResearchProjectDef r) => !r.IsFinished);
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

		public static bool PassesIdeoCheck(this RecipeDef recipe)
		{
			if (!ModsConfig.IdeologyActive)
				return true;
			if (recipe.memePrerequisitesAny == null)
				return true;
			return recipe.memePrerequisitesAny.Any(mp => Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(mp));
		}
    }
}
