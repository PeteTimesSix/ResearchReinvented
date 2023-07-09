using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Extensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.Prototypes
{
	[HarmonyPatch(typeof(Bill_Medical), nameof(Bill_Medical.Notify_IterationCompleted))]
	public static class Bill_Medical_Notify_IterationCompleted_Patches
	{
		[HarmonyPostfix]
		public static void Postfix(Bill_Medical __instance, Pawn billDoer)
		{
			var recipe = __instance.recipe;
            bool isPrototype = recipe != null && recipe.IsAvailableOnlyForPrototyping(true);
            if (isPrototype)
            {
                PrototypeUtilities.DoPostFinishSurgeryResearch(__instance.GiverPawn, billDoer, recipe.WorkAmountTotal(null), recipe);
            }
		}
	}
}
