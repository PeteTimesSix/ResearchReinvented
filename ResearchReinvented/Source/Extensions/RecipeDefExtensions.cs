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
		public static bool IsAvailableOnlyForPrototyping(this RecipeDef recipe) 
		{
			return !recipe.AvailableNow && recipe.IsAvailableForPrototyping();
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
