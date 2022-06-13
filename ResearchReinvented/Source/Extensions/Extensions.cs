using PeteTimesSix.ResearchReinvented.Rimworld.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class SmallExtensions
    {
        public static Rect OffsetBy(this Rect rect, float x, float y)
        {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }

        public static bool CanUseNow(this Building_ResearchBench bench) 
        {
            var powerComp = bench.GetComp<CompPowerTrader>();
            var forbidComp = bench.GetComp<CompForbiddable>();

            return bench.Spawned && (powerComp == null || powerComp.PowerOn) && (forbidComp == null || !forbidComp.Forbidden) && bench.Faction == Faction.OfPlayer;
		}
	}

    /*public static class ReipeDefExtensions 
    {
		public static bool HasNoResearchPrerequisites(this RecipeDef recipe) 
		{
			return recipe.researchPrerequisite == null && recipe.researchPrerequisites == null;
		}

		public static bool UnlocksOnlyWith(this RecipeDef recipe, IEnumerable<ThingDef> recipeHolders) 
		{
			//Log.Message($"checking if recipe is only used by things in list, recipe is {recipe.defName}, things are {string.Join(",", recipeHolders.Select(t => t.defName))}");
			//Log.Message($"all recipe users are: {string.Join(",", recipe.AllRecipeUsers)}");
			return recipe.AllRecipeUsers.Any(u => !recipeHolders.Contains(u)) == false;
		}

		public static bool UnlockedSpecificallyBy(this RecipeDef recipe, ResearchProjectDef project)
		{
			if (recipe.HasNoResearchPrerequisites())
				return false;
			if (recipe.researchPrerequisite != null)
            {
				if(recipe.researchPrerequisite != project)
					return false;
			}
			else if (recipe.researchPrerequisites != null)
			{
				if (!recipe.researchPrerequisites.Any((ResearchProjectDef r) => r == project))
					return false;
			}
			return true;
			/*if (recipe.memePrerequisitesAny != null)
			{
				bool memePreregMet = false;
				foreach (MemeDef meme in recipe.memePrerequisitesAny)
				{
					if (Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(meme))
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
			if (recipe.factionPrerequisiteTags != null)
			{
				if (recipe.factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
				{
					if (!recipe.UnlockedByIdeology())
						return false;
				}
			}
			return !recipe.fromIdeoBuildingPreceptOnly || (ModsConfig.IdeologyActive && IdeoUtility.PlayerHasPreceptForBuilding(recipe.ProducedThingDef));*/
		/*}

		public static bool AvailableWith(this RecipeDef recipe, ResearchProjectDef project)
		{
			if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished && recipe.researchPrerequisite != project)
			{
				return false;
			}
			if (recipe.memePrerequisitesAny != null)
			{
				bool memePreregMet = false;
				foreach (MemeDef meme in recipe.memePrerequisitesAny)
				{
					if (Faction.OfPlayer.ideos.HasAnyIdeoWithMeme(meme))
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
				if (recipe.researchPrerequisites.Any((ResearchProjectDef r) => !r.IsFinished && r != project))
				{
					return false;
				}
			}
			if (recipe.factionPrerequisiteTags != null)
			{
				if (recipe.factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
				{
					if (!recipe.UnlockedByIdeology())
						return false;
				}
			}
			return !recipe.fromIdeoBuildingPreceptOnly || (ModsConfig.IdeologyActive && IdeoUtility.PlayerHasPreceptForBuilding(recipe.ProducedThingDef));
		}*/

		/*private static bool UnlockedByIdeology(this RecipeDef recipe) 
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
							foreach(var product in recipe.products) 
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
	}*/
}
