using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
    public static class ResearchManagerAccess
    {
        private static ResearchManager activeInstance;

        private static Dictionary<ResearchProjectDef, float> cached_progress;
        public static Dictionary<ResearchProjectDef, float> Field_progress
        {
            get{
                if(activeInstance == null || activeInstance != Find.ResearchManager || cached_progress == null)
                {
                    activeInstance = Find.ResearchManager;
                    cached_progress = AccessTools.Field(typeof(ResearchManager), "progress").GetValue(activeInstance) as Dictionary<ResearchProjectDef, float>;
                }
                return cached_progress;
            }
        }
    }
}
