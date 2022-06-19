using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
    [StaticConstructorOnStartup]
    public static partial class DebugMenuEntries
    {
        private const string CATEGORY = "Research roundabout";

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void ToggleDebugPrintouts() 
        {
            ResearchReinventedMod.Settings.debugPrintouts = !ResearchReinventedMod.Settings.debugPrintouts;
            string state = ResearchReinventedMod.Settings.debugPrintouts ? "on" : "off";
            Log.Message($"Toggled debug printouts {state}");
        }
    }
}
