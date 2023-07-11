using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    [StaticConstructorOnStartup]
    public static class CombatExtended
    {
        public static bool active = false;

        static CombatExtended()
        {
            active = ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null;
        }

        public static void PatchDelayed(Harmony harmony)
        {
            Type type = AccessTools.TypeByName("CombatExtended.HarmonyCE.Harmony_PawnRenderer").GetNestedType("Harmony_PawnRenderer_DrawBodyApparel");

            harmony.Patch(AccessTools.Method(type, "IsVisibleLayer"), postfix: new HarmonyMethod(AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel_Patches), nameof(Harmony_PawnRenderer_DrawBodyApparel_Patches.IsVisibleLayerPostfix))));
        }
    }
}
