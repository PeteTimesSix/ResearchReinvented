using PeteTimesSix.ResearchReinvented.Data;
using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.ModCompat;
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
        GLOBAL_CONFIG,
        CATEGORY_PRESETS,
        CATEGORY_CONFIG,
        MOD_INTEGRATIONS
    }

    public class ResearchReinvented_Settings : ModSettings
    {
        public const float HEADER_HEIGHT = 35f;
        public const float HEADER_GAP_HEIGHT = 10f;

        public SettingsPresetDef activePreset;

        public bool defaultCompactMode = false;
        public bool showProgressMotes = true;

        public bool kitlessResearch = false;
        public bool kitlessNeolithicResearch = true;


        public List<CategorySettingsChanges> categorySettingChanges = new List<CategorySettingsChanges>();
        public List<CategorySettingsFinal> categorySettings = new List<CategorySettingsFinal>();

        public SettingTab temp_activeTab = SettingTab.CATEGORY_PRESETS;
        public ResearchOpportunityCategoryDef temp_selectedCategory = null;

        public ResearchData.ResearchDataCompatMode researchDataCompatMode = ResearchData.ResearchDataCompatMode.AllBenchResearch;

        private static Color LightGreen = new Color(0.7f, 1f, 0.7f);

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref activePreset, "activePreset");

            Scribe_Values.Look(ref defaultCompactMode, "defaultCompactMode", false);
            Scribe_Values.Look(ref showProgressMotes, "showProgressMotes", true);

            Scribe_Values.Look(ref kitlessResearch, "kitlessResearch", false);
            Scribe_Values.Look(ref kitlessNeolithicResearch, "kitlessNeolithicResearch", true);

            Scribe_Collections.Look(ref categorySettingChanges, "categorySettingChanges", LookMode.Deep);

            Scribe_Values.Look(ref researchDataCompatMode, "researchDataCompatMode", ResearchData.ResearchDataCompatMode.AllBenchResearch);
            //do not save/load categorySettings, let them generate
        }

        public void DoSettingsWindowContents(Rect inRect)
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
                case SettingTab.GLOBAL_CONFIG:
                    DoGlobalConfigTab(restOfRect);
                    break;
                case SettingTab.CATEGORY_PRESETS:
                    DoPresetTab(restOfRect);
                    break;
                case SettingTab.CATEGORY_CONFIG:
                    DoCategoriesConfigTab(restOfRect);
                    break;
                case SettingTab.MOD_INTEGRATIONS:
                    DoModIntegrationTab(restOfRect);
                    break;
            }

            Text.Anchor = anchor;
            GUI.color = preColor;
        }

        private void DoGlobalConfigTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float maxWidth = listingStandard.ColumnWidth;

            listingStandard.Indent(maxWidth / 4f);
            listingStandard.ColumnWidth = maxWidth / 2f;
            listingStandard.CheckboxLabeled("RR_setting_defaultCompactMode".Translate(), ref defaultCompactMode, "RR_setting_defaultCompactMode_tooltip".Translate());
            listingStandard.CheckboxLabeled("RR_setting_showProgressMotes".Translate(), ref showProgressMotes, "RR_setting_defaultCompactMode_showProgressMotes_tooltip".Translate());

            listingStandard.CheckboxLabeled("RR_setting_kitlessResearch".Translate(), ref kitlessResearch, "RR_setting_kitlessResearch_tooltip".Translate());
            listingStandard.CheckboxLabeled("RR_setting_kitlessNeolithicResearch".Translate(), ref kitlessNeolithicResearch, "RR_setting_kitlessNeolithicResearch".Translate());

            float remainingHeight = inRect.height - listingStandard.CurHeight;
            listingStandard.Gap(remainingHeight - Text.LineHeight * 1.5f);

            listingStandard.End();
        }

        private void DoPresetTab(Rect inRect)
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
                
                sectionListing.Gap();
                Text.Anchor = TextAnchor.MiddleCenter;
                sectionListing.Label(activePreset.LabelCap);
                sectionListing.Gap();
                Text.Anchor = TextAnchor.MiddleLeft;
                sectionListing.Label(activePreset.description);
                sectionListing.GapLine();
                sectionListing.Gap();

                SettingsPresetDef hoveredPreset = null;
                foreach (var preset in DefDatabase<SettingsPresetDef>.AllDefsListForReading.OrderByDescending(p => p.priority))
                {
                    var heightPre = sectionListing.CurHeight;
                    if (sectionListing.RadioButton(preset.LabelCap, activePreset == preset))
                    {
                        activePreset = preset;
                        foreach (var categoryDef in DefDatabase<ResearchOpportunityCategoryDef>.AllDefsListForReading)
                            categoryDef.ClearCachedData();
                        categorySettingChanges.Clear();
                        categorySettings.Clear();
                    }
                    var heightPost = sectionListing.CurHeight;
                    var height = heightPost - heightPre;
                    var hoverRect = sectionListing.GetRect(0f);
                    hoverRect.height = height;
                    hoverRect.y -= height;
                    if (Mouse.IsOver(hoverRect)) 
                    {
                        hoveredPreset = preset;
                    }
                }

                if(hoveredPreset != null && hoveredPreset != activePreset) 
                {
                    sectionListing.Gap();
                    sectionListing.GapLine();
                    GUI.color = Color.gray;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    sectionListing.Label(hoveredPreset.LabelCap);
                    sectionListing.Gap();
                    Text.Anchor = TextAnchor.MiddleLeft;
                    sectionListing.Label(hoveredPreset.description);
                    GUI.color = Color.white;    
                }

                sectionListing.End();
            }

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

                    var temp_categorySettings = temp_selectedCategory.Settings;
                    var temp_categoryChanges = GetCategorySettingsChanges(temp_selectedCategory);

                    GUI.color = temp_categoryChanges.enabled.HasValue ? Color.yellow : Color.white;
                    sectionListing.CheckboxLabeled("RR_setting_category_enabled".Translate(), ref temp_categorySettings.enabled, "RR_setting_category_enabled_tooltip".Translate());

                    if (temp_categorySettings.enabled)
                    {
                        GUI.color = temp_categoryChanges.availableAtOverallProgress.HasValue ? Color.yellow : Color.white;
                        sectionListing.FloatRangeLabeled("RR_setting_category_availableAtOverallProgress".Translate(), ref temp_categorySettings.availableAtOverallProgress, min: 0, max: 1, roundTo: 0.01f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_availableAtOverallProgress_tooltip".Translate());

                        GUI.color = temp_categoryChanges.importanceMultiplier.HasValue ? Color.yellow : Color.white;
                        sectionListing.SliderLabeled("RR_setting_category_importanceMultiplier".Translate(), ref temp_categorySettings.importanceMultiplier, min: 0, max: 5, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_importanceMultiplier_tooltip".Translate());

                        GUI.color = temp_categoryChanges.importanceMultiplierCounted.HasValue ? Color.yellow : Color.white;
                        sectionListing.SliderLabeled("RR_setting_category_importanceMultiplierCounted".Translate(), ref temp_categorySettings.importanceMultiplierCounted, min: 0, max: temp_categorySettings.importanceMultiplier, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_importanceMultiplierCounted_tooltip".Translate());

                        GUI.color = temp_categoryChanges.targetIterations.HasValue ? Color.yellow : Color.white;
                        sectionListing.SliderLabeled("RR_setting_category_targetIterations".Translate(), ref temp_categorySettings.targetIterations, min: 1, max: 30, roundTo: 0.25f, decimalPlaces: 2, tooltip: "RR_setting_category_targetIterations_tooltip".Translate());

                        GUI.color = temp_categoryChanges.infiniteOverflow.HasValue ? Color.yellow : Color.white;
                        sectionListing.CheckboxLabeled("RR_setting_category_infiniteOverflow".Translate(), ref temp_categorySettings.infiniteOverflow, "RR_setting_category_infiniteOverflow_tooltip".Translate());

                        GUI.color = temp_categoryChanges.researchSpeedMultiplier.HasValue ? Color.yellow : Color.white;
                        sectionListing.SliderLabeled("RR_setting_category_researchSpeedMultiplier".Translate(), ref temp_categorySettings.researchSpeedMultiplier, min: 0.05f, max: 5, roundTo: 0.05f, displayMult: 100, valueSuffix: "%", tooltip: "RR_setting_category_researchSpeedMultiplier_tooltip".Translate());

                        //sectionListing.Spinner("RR_setting_category_priority".Translate(), ref temp_selectedCategory.priority, 1, 100, 1500, "RR_setting_category_priority".Translate());
                    }

                    temp_categoryChanges.UpdateChanges(GetActivePreset().GetCategoryPreset(temp_selectedCategory), temp_categorySettings);

                    GUI.color = Color.white;
                    if (sectionListing.ButtonText("RR_setting_category_resetToDefault".Translate()))
                    {
                        temp_selectedCategory.ClearCachedData();
                        categorySettingChanges.Remove(temp_categoryChanges);
                        categorySettings.Remove(temp_categorySettings);
                    }

                    sectionListing.Gap();
                    sectionListing.Label(temp_categorySettings.category.description);
                }

                listingStandard.EndHiddenSection(sectionListing, maxHeightAccumulator);
            }

            listingStandard.End();
        }

        public SettingsPresetDef GetActivePreset() 
        {
            if(activePreset == null) 
            {
                activePreset = SettingsPresetDefOf.SettingsPreset_RR_Default;
            }
            return activePreset;
        }

        public CategorySettingsChanges GetCategorySettingsChanges(ResearchOpportunityCategoryDef category)
        {
            if (categorySettingChanges == null)
                categorySettingChanges = new List<CategorySettingsChanges>();
            var changes = categorySettingChanges.FirstOrDefault(cs => cs.category == category);
            if(changes == null) 
            {
                changes = new CategorySettingsChanges() { category = category };
                categorySettingChanges.Add(changes);
            }
            return changes;
        }

        public CategorySettingsFinal GetCategorySettings(ResearchOpportunityCategoryDef category)
        {
            if (categorySettings == null)
                categorySettings = new List<CategorySettingsFinal>();
            var settings = categorySettings.FirstOrDefault(cs => cs.category == category);
            if (settings == null)
            {
                settings = new CategorySettingsFinal() { category = category };
                settings.Update(GetActivePreset().GetCategoryPreset(category), GetCategorySettingsChanges(category));
                categorySettings.Add(settings);
            }
            return settings;
        }

        private void DoModIntegrationTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float maxWidth = listingStandard.ColumnWidth;

            listingStandard.Indent(maxWidth / 4f);
            listingStandard.ColumnWidth = maxWidth / 2f;

            //active
            if(CombatExtended.active)
            {
                GUI.color = LightGreen;
                listingStandard.Label("RR_setting_modCompat_detected_combatExtended".Translate());
            }
            if (DubsMintMenus.active)
            {
                GUI.color = LightGreen;
                listingStandard.Label("RR_setting_modCompat_detected_dubsMintMenus".Translate());
            }
            if (HumanoidAlienRaces.active)
            {
                GUI.color = LightGreen;
                listingStandard.Label("RR_setting_modCompat_detected_humanoidAlienRaces".Translate());
            }
            if (ResearchData.active)
            {
                GUI.color = LightGreen;
                listingStandard.Label("RR_setting_modCompat_detected_researchData".Translate());
                listingStandard.EnumSelector("RR_setting_modCompat_researchData_mode".Translate(), ref researchDataCompatMode, "RR_modCompat_researchData_mode_");
            }

            //inactive
            if (!CombatExtended.active)
            {
                GUI.color = Color.gray;
                listingStandard.Label("RR_setting_modCompat_notDetected_combatExtended".Translate());
            }
            if (!DubsMintMenus.active)
            {
                GUI.color = Color.gray;
                listingStandard.Label("RR_setting_modCompat_notDetected_dubsMintMenus".Translate());
            }
            if (!HumanoidAlienRaces.active)
            {
                GUI.color = Color.gray;
                listingStandard.Label("RR_setting_modCompat_notDetected_humanoidAlienRaces".Translate());
            }
            if (!ResearchData.active)
            {
                GUI.color = Color.gray;
                listingStandard.Label("RR_setting_modCompat_notDetected_researchData".Translate());
            }

            listingStandard.End();
        }
    }
}