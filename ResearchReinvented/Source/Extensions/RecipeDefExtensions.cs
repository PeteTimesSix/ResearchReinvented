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


        public static bool IsAvailableForPrototyping(this RecipeDef recipe) 
        {
			if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished && Find.ResearchManager.currentProj != recipe.researchPrerequisite)
			{
				return false;
			}
			if (recipe.memePrerequisitesAny != null)
			{
				bool flag = false;
				foreach (MemeDef memeDef in recipe.memePrerequisitesAny)
				{
					if (Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(memeDef))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (recipe.researchPrerequisites != null)
			{
				if (recipe.researchPrerequisites.Any((ResearchProjectDef r) => !r.IsFinished && Find.ResearchManager.currentProj != recipe.researchPrerequisite))
				{
					return false;
				}
			}
			if (recipe.factionPrerequisiteTags != null)
			{
				if (recipe.factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
				{
					if (!somethingSomethingIdeo(recipe))
						return false;
				}
			}
			return !recipe.fromIdeoBuildingPreceptOnly || (ModsConfig.IdeologyActive && IdeoUtility.PlayerHasPreceptForBuilding(recipe.ProducedThingDef));
		}

		private static bool somethingSomethingIdeo(this RecipeDef recipe) 
		{
			foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
			{
				foreach (Precept_Role precept_Role in ideo.RolesListForReading)
				{
					if (precept_Role.apparelRequirements != null)
					{
						foreach (PreceptApparelRequirement preceptApparelRequirement in precept_Role.apparelRequirements)
						{
							ThingDef thingDef = preceptApparelRequirement.requirement.AllRequiredApparel(Gender.None).FirstOrDefault<ThingDef>();
							if (thingDef == null)
							{
								Log.Error("Apparel requirement for role " + precept_Role.Label + " is null");
							}
							using (List<ThingDefCountClass>.Enumerator enumerator4 = recipe.products.GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									if (enumerator4.Current.thingDef == thingDef)
									{
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}
    }
}
