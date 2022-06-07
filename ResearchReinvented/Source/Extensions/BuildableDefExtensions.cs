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
		public static bool IsAvailableOnlyForPrototyping(this BuildableDef buildable)
		{
			return !buildable.IsResearchFinished && buildable.IsAvailableForPrototyping();
		}


		public static bool IsAvailableForPrototyping(this BuildableDef buildable)
		{
			if (buildable.researchPrerequisites != null)
			{
				return buildable.researchPrerequisites.Any(r => !r.IsFinished && Find.ResearchManager.currentProj != r);
			}
			return true;
		}
	}
}
