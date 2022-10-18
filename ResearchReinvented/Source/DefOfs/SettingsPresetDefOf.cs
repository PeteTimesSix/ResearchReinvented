using PeteTimesSix.ResearchReinvented.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.DefOfs
{
    [DefOf]
    public static class SettingsPresetDefOf
    {
        public static SettingsPresetDef SettingsPreset_RR_Default;

        static SettingsPresetDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SettingsPresetDefOf));
        }
    }
}
