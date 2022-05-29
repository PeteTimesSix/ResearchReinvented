using PeteTimesSix.ResearchReinvented.Rimworld;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    public class ResearchReinvented_Settings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.End();
        }
    }
}