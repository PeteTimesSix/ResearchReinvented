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
    public static class RecipeDefExtensions
	{
		public static IEnumerable<ResearchOpportunity> PrototypeOpportunities => ResearchOpportunityManager.instance.CurrentProjectOpportunities.Where(o => o.IsValid() && o.def.handledBy.HasFlag(HandlingMode.Special_Prototype));

		public static ResearchProjectDef cacheBuiltForProject = null;
		public static List<ResearchOpportunity> _prototypeOpportunitiesCache = new List<ResearchOpportunity>();
		public static Dictionary<RecipeDef, ResearchOpportunity> _prototypeOpportunitiesMappedCache = new Dictionary<RecipeDef, ResearchOpportunity>();
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

		public static Dictionary<RecipeDef, ResearchOpportunity> PrototypeOpportunitiesMappedCache
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

		public static HashSet<ResearchProjectDef> AllResearchPrerequisites(this RecipeDef recipe)
		{
			HashSet<ResearchProjectDef> prerequisites = new HashSet<ResearchProjectDef>();
			if (recipe.researchPrerequisite != null)
				prerequisites.Add(recipe.researchPrerequisite);
			if (recipe.researchPrerequisites != null)
				prerequisites.AddRange(recipe.researchPrerequisites);
			return prerequisites;
		}

		public static bool IsAvailableOnlyForPrototyping(this RecipeDef def, bool evenIfFinished)
		{
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

		public static ResearchOpportunity FindPrototypeOpportunity(this RecipeDef recipe)
		{
			bool canBePrototyped = !recipe.AvailableNow && recipe.IsAvailableForPrototyping();

			if (!canBePrototyped)
				return null;
			else
				return //ResearchOpportunityManager.instance.GetCurrentlyAvailableOpportunities(true)
					PrototypeOpportunitiesCached.FirstOrDefault(o => o.requirement.MetBy(recipe) || (recipe.ProducedThingDef != null && o.requirement.MetBy(recipe.ProducedThingDef)));
		}

		public static bool PassesIdeoCheck(this RecipeDef recipe)
		{
			if (!ModsConfig.IdeologyActive)
				return true;
			if (recipe.memePrerequisitesAny == null)
				return true;
			return recipe.memePrerequisitesAny.Any(mp => Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(mp));
		}

        private static bool IsAvailableForPrototyping(this RecipeDef recipe) 
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
		}
    }
}
