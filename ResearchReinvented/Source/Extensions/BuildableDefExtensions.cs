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

		public static bool IsAvailableOnlyForPrototyping(this BuildableDef def)
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
