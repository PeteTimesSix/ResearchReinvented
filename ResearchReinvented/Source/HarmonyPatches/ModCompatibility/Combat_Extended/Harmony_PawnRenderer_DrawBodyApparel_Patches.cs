using PeteTimesSix.ResearchReinvented.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.HarmonyPatches.ModCompatibility.Combat_Extended
{
    public static class Harmony_PawnRenderer_DrawBodyApparel_Patches
    {
        public static bool IsVisibleLayerPostfix(bool __result, ApparelLayerDef def) 
        {
            if(__result == false)
                return false;
            if(def == ApparelLayerDefOf_Custom.Satchel)
                return false;
            return true;
        }

    }
}
