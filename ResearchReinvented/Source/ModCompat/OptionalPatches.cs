using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Rimworld.WorkGivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.ModCompat
{
    public static class OptionalPatches
    {
        public static void Patch(Harmony harmony) 
        {
            //Log.Warning("Doing optional patches...");
        }

        public static void PatchDelayed(Harmony harmony)
        {
            //Log.Warning("Doing delayed optional patches...");
            if (CombatExtended.active)
            {
                CombatExtended.PatchDelayed(harmony);
            }
            if (DubsMintMenus.active)
            {
                DubsMintMenus.PatchDelayed(harmony);
            }
			if (HumanoidAlienRaces.active)
			{
				HumanoidAlienRaces.PatchDelayed(harmony);
            }
            if (ResearchData.active)
            {
                ResearchData.PatchDelayed(harmony);
            }
        }

	}
}
