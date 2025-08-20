using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using PeteTimesSix.ResearchReinvented.DefOfs;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
    public static class Pawn_GuestTracker_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_GuestTracker __instance, GuestStatus guestStatus)
        {
            if( guestStatus == GuestStatus.Prisoner && ResearchReinventedMod.Settings.enableScienceInterrogationByDefault )
                __instance.ToggleNonExclusiveInteraction(PrisonerInteractionModeDefOf_Custom.RR_ScienceInterrogation, true);
        }
    }
}
