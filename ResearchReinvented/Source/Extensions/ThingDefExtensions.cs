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
        public static bool IsRawFood(this ThingDef x)
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
	}
}
