using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    public class ResearchReinvented_Settings : ModSettings
    {
        public float prototypeResearchSpeedFactor = 0.5f;
        public bool debugPrintouts = false;

        public bool defaultCompactMode = false;

        public bool kitlessResearch = true;
        public float theoryResearchSpeedMult = 0.85f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref debugPrintouts, "debugPrintouts", false);
            Scribe_Values.Look(ref prototypeResearchSpeedFactor, "prototypeResearchSpeedFactor", 0.5f);

            Scribe_Values.Look(ref defaultCompactMode, "defaultCompactMode", false);

            Scribe_Values.Look(ref kitlessResearch, "kitlessResearch", true);
            Scribe_Values.Look(ref theoryResearchSpeedMult, "theoryResearchSpeedMult", 0.85f);
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("RR_setting_defaultCompactMode".Translate(), ref defaultCompactMode, "RR_setting_defaultCompactMode_tooltip".Translate());

            listingStandard.CheckboxLabeled("RR_setting_kitlessResearch".Translate(), ref kitlessResearch, "RR_setting_kitlessResearch_tooltip".Translate());
            listingStandard.SliderLabeled("RR_setting_basicResearchSpeedMult".Translate(), ref theoryResearchSpeedMult, 0.1f, 1f, 100, 0, "%", "RR_setting_basicResearchSpeedMult_tooltip".Translate());

            listingStandard.End();
        }
    }
}