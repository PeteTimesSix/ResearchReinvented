using HarmonyLib;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
	[HarmonyPatch(typeof(DesignationCategoryDef), nameof(DesignationCategoryDef.Visible), MethodType.Getter)]
	public static class DesignationCategoryDef_Visible_Patches
	{
		[HarmonyPostfix]
		public static void Postfix(DesignationCategoryDef __instance, ref bool __result)
		{
			if (__result == false)
			{
				if (__instance.researchPrerequisites == null)
				{
					//Log.ErrorOnce("designationCategoryDef " + __instance.defName + " is not visible, yet has no prerequisites, this is UNHANDLED", 6652244);
				}
				else
				{
					foreach (ResearchProjectDef researchPrerequisiteProject in __instance.researchPrerequisites)
					{
						if (!researchPrerequisiteProject.IsFinished)
						{
							if (researchPrerequisiteProject != Find.ResearchManager.GetProject())
								return;
							OpportunityAvailability prototypingAvailability = ResearchOpportunityCategoryDefOf.Prototyping.GetCurrentAvailability(researchPrerequisiteProject);
							bool prototypeable = prototypingAvailability == OpportunityAvailability.Available || prototypingAvailability == OpportunityAvailability.Finished || prototypingAvailability == OpportunityAvailability.CategoryFinished;
							if (!prototypeable)
								return;
						}
					}
					__result = true;
				}
			}
		}
	}
}
