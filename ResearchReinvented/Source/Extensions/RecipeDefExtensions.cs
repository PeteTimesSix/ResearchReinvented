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
		public static ResearchProjectDef cacheBuiltForProject = null;
		public static Dictionary<RecipeDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = new Dictionary<RecipeDef, ResearchOpportunity>();

		public static Dictionary<RecipeDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
		{
			get
			{
				if (cacheBuiltForProject != Find.ResearchManager.GetProject())
				{
					_prototypeOpportunitiesMappedCache.Clear();
                    foreach (var op in PrototypeUtilities.PrototypeOpportunities)
                    {
                        if (op.requirement is ROComp_RequiresRecipe requiresRecipe)
                            _prototypeOpportunitiesMappedCache[requiresRecipe.recipeDef] = op;
                    }
					cacheBuiltForProject = Find.ResearchManager.GetProject();
				}
				return _prototypeOpportunitiesMappedCache;
			}
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
                if (unfinishedPreregs.Any((ResearchProjectDef r) => Find.ResearchManager.GetProject() != r))
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

		public static bool PassesIdeoCheck(this RecipeDef recipe)
		{
			if (!ModsConfig.IdeologyActive)
				return true;
			if (recipe.memePrerequisitesAny == null)
				return true;
			return recipe.memePrerequisitesAny.Any(mp => Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(mp));
		}

        /*private static bool IsAvailableForPrototyping(this RecipeDef recipe) 
        {
			if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished && Find.ResearchManager.currentProj != recipe.researchPrerequisite)
			{
				return false;
			}
			if (recipe.memePrerequisitesAny != null)
			{
				bool memePreregMet = false;
				foreach (MemeDef memeDef in recipe.memePrerequisitesAny)
				{
					if (Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(memeDef))
					{
						memePreregMet = true;
						break;
					}
				}
				if (!memePreregMet)
				{
					return false;
				}
			}
			if (recipe.researchPrerequisites != null)
			{
				if (recipe.researchPrerequisites.Any((ResearchProjectDef r) => !r.IsFinished && Find.ResearchManager.currentProj != r))
				{
					return false;
				}
			}
			if (recipe.factionPrerequisiteTags != null)
			{
				if (recipe.factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
				{
					if (!UnlockedByIdeology(recipe))
						return false;
				}
			}
			return !recipe.fromIdeoBuildingPreceptOnly || (ModsConfig.IdeologyActive && IdeoUtility.PlayerHasPreceptForBuilding(recipe.ProducedThingDef));
		}

		private static bool UnlockedByIdeology(this RecipeDef recipe)
		{
			foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
			{
				foreach (Precept_Role precept_Role in ideo.RolesListForReading)
				{
					if (precept_Role.apparelRequirements != null)
					{
						foreach (PreceptApparelRequirement preceptApparelRequirement in precept_Role.apparelRequirements)
						{
							ThingDef thingDef = preceptApparelRequirement.requirement.AllRequiredApparel(Gender.None).FirstOrDefault();
							if (thingDef == null)
							{
								Log.Error("Apparel requirement for role " + precept_Role.Label + " is null");
							}
							foreach (var product in recipe.products)
							{
								if (product.thingDef == thingDef)
									return true;
							}
						}
					}
				}
			}
			return false;
		}*/
    }
}
