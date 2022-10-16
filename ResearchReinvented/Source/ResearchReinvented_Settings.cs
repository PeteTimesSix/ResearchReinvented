using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Utilities;
using PeteTimesSix.ResearchReinvented.Utilities.CustomWidgets;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented
{
    public enum SettingTab
    {
        PRESETS,
        GLOBAL_CONFIG,
        CATEGORY_CONFIG
    }

    public class ResearchReinvented_Settings : ModSettings
    {
        public const float HEADER_HEIGHT = 35f;
        public const float HEADER_GAP_HEIGHT = 10f;

        public float prototypeResearchSpeedFactor = 0.5f;
        public bool debugPrintouts = false;

        public bool defaultCompactMode = false;

        public bool kitlessResearch = true;
        //public float theoryResearchSpeedMult = 0.85f;

        public SettingTab temp_activeTab = SettingTab.PRESETS;
        public ResearchOpportunityCategoryDef temp_selectedCategory = null;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref debugPrintouts, "debugPrintouts", false);
            Scribe_Values.Look(ref prototypeResearchSpeedFactor, "prototypeResearchSpeedFactor", 0.5f);

            Scribe_Values.Look(ref defaultCompactMode, "defaultCompactMode", false);

            Scribe_Values.Look(ref kitlessResearch, "kitlessResearch", true);
            //Scribe_Values.Look(ref theoryResearchSpeedMult, "theoryResearchSpeedMult", 0.85f);
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            Color preColor = GUI.color;
            var anchor = Text.Anchor;

            var headerRect = inRect.TopPartPixels(HEADER_HEIGHT);
            var restOfRect = inRect.BottomPartPixels(inRect.height - (HEADER_HEIGHT + HEADER_GAP_HEIGHT));

            //Widgets.DrawWindowBackground(headerRect);
            //Widgets.DrawWindowBackground(restOfRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(headerRect);

            listingStandard.EnumSelector(null, ref temp_activeTab, "RR_setting_tab_", lineHeightOverride: HEADER_HEIGHT);

            listingStandard.End();

            switch (temp_activeTab)
            {
                case SettingTab.PRESETS:
                    DoPresetTab(restOfRect);
                    break;
                case SettingTab.GLOBAL_CONFIG:
                    DoGlobalConfigTab(restOfRect);
                    break;
                case SettingTab.CATEGORY_CONFIG:
                    DoCategoriesConfigTab(restOfRect);
                    break;
            }

            Text.Anchor = anchor;
            GUI.color = preColor;
        }

        private void DoPresetTab(Rect inRect)
        {
        }

        private void DoGlobalConfigTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float maxWidth = listingStandard.ColumnWidth;

            listingStandard.CheckboxLabeled("RR_setting_debugPrintouts".Translate(), ref debugPrintouts, "RR_setting_debugPrintouts_tooltip".Translate());
            listingStandard.CheckboxLabeled("RR_setting_defaultCompactMode".Translate(), ref defaultCompactMode, "RR_setting_defaultCompactMode_tooltip".Translate());

            listingStandard.CheckboxLabeled("RR_setting_kitlessResearch".Translate(), ref kitlessResearch, "RR_setting_kitlessResearch_tooltip".Translate());
            //listingStandard.SliderLabeled("RR_setting_basicResearchSpeedMult".Translate(), ref theoryResearchSpeedMult, 0.1f, 1f, 100, 0, "%", "RR_setting_basicResearchSpeedMult_tooltip".Translate());

            float remainingHeight = inRect.height - listingStandard.CurHeight;

            listingStandard.End();
        }

        private void DoCategoriesConfigTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float remainingHeight = inRect.height - listingStandard.CurHeight;
            var sectionListing = listingStandard.BeginHiddenSection(out float maxHeightAccumulator);
            {
                float sectionMaxWidth = listingStandard.ColumnWidth;

                sectionListing.ColumnWidth = (sectionMaxWidth - ListingExtensions.ColumnGap) / 2f;
                var visualizerRect = sectionListing.GetRect(remainingHeight);
                //Widgets.DrawWindowBackgroundTutor(visualizerRect);

                CategoriesVisualizer.DrawCategories(visualizerRect);

                sectionListing.NewHiddenColumn(ref maxHeightAccumulator);
                if (sectionListing.ButtonText("RR_setting_category_selector".Translate()))
                {
                    var options = new List<FloatMenuOption>();
                    foreach (var category in DefDatabase<ResearchOpportunityCategoryDef>.AllDefsListForReading.OrderByDescending(d => d.priority))
                    {
                        options.Add(new FloatMenuOption(category.LabelCap, () => { temp_selectedCategory = category; }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }

                if (temp_selectedCategory != null)
                {
                    sectionListing.Gap();

                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = temp_selectedCategory.color;
                    sectionListing.Label(temp_selectedCategory.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = Color.white;

                    sectionListing.Gap();

                    sectionListing.CheckboxLabeled("RR_setting_category_enabled".Translate(), ref temp_selectedCategory.enabled, "RR_setting_category_enabled_tooltip".Translate());

                    if (temp_selectedCategory.enabled)
                    {
                        sectionListing.FloatRangeLabeled("RR_setting_category_availableAtOverallProgress".Translate(), ref temp_selectedCategory.availableAtOverallProgress, min: 0, max: 1, roundTo: 0.01f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_availableAtOverallProgress_tooltip".Translate());
                        
                        sectionListing.SliderLabeled("RR_setting_category_targetIterations".Translate(), ref temp_selectedCategory.targetIterations, min: 1, max: 30, roundTo: 0.25f, decimalPlaces: 2, tooltip: "RR_setting_category_targetIterations_tooltip".Translate());

                        sectionListing.SliderLabeled("RR_setting_category_targetFractionMultiplier".Translate(), ref temp_selectedCategory.targetFractionMultiplier, min: 0, max: 5, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_targetFractionMultiplier_tooltip".Translate());
                        sectionListing.CheckboxLabeled("RR_setting_category_infiniteOverflow".Translate(), ref temp_selectedCategory.infiniteOverflow, "RR_setting_category_infiniteOverflow_tooltip".Translate());
                        if (!temp_selectedCategory.infiniteOverflow)
                            sectionListing.SliderLabeled("RR_setting_category_extraFractionMultiplier".Translate(), ref temp_selectedCategory.extraFractionMultiplier, min: 0, max: 5, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_extraFractionMultiplier_tooltip".Translate());
                        
                        sectionListing.SliderLabeled("RR_setting_category_researchSpeedMultiplier".Translate(), ref temp_selectedCategory.researchSpeedMultiplier, min: 0, max: 5, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_researchSpeedMultiplier_tooltip".Translate());
                        //sectionListing.Spinner("RR_setting_category_priority".Translate(), ref temp_selectedCategory.priority, 1, 100, 1500, "RR_setting_category_priority".Translate());
                    }
                }

                listingStandard.EndHiddenSection(sectionListing, maxHeightAccumulator);
            }

            listingStandard.End();
        }
    }
}