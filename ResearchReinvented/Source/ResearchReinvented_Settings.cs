using PeteTimesSix.ResearchReinvented.Rimworld;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    public class ResearchReinvented_Settings : ModSettings
    {
        public float prototypeResearchSpeedFactor = 0.5f;
        internal bool debugPrintouts = false;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref debugPrintouts, "debugPrintouts", false);
            Scribe_Values.Look(ref prototypeResearchSpeedFactor, "prototypeResearchSpeedFactor", 0.5f);
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.End();
        }
    }
}