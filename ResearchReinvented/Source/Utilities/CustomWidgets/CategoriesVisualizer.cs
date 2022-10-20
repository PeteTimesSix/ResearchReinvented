using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Rimworld;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PeteTimesSix.ResearchReinvented.Utilities.CustomWidgets
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [StaticConstructorOnStartup]
    [HotSwappable]
    public static class CategoriesVisualizer
    {
        private static readonly Texture2D FadeGradient = ContentFinder<Texture2D>.Get("UI/fadeGradient", true);

        private static readonly Color WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;

        private static float INFINITE_EXTRA_STANDIN = 0.25f;

        public static void DrawCategories(Rect rect) 
        {
            var anchor = Text.Anchor;
            var color = GUI.color;

            float totalFractionMultiplier = 0;
            float totalExtraFractionMultiplier = 0;
            float totalAllFractionMultiplier = 0;

            float maxFractionMultiplier = 1;
            float maxExtraFractionMultiplier = 0;
            float maxAllFractionMultiplier = 1;
            float maxResearchSpeedMultiplier = 0;

            var categories = DefDatabase<ResearchOpportunityCategoryDef>.AllDefsListForReading.OrderByDescending(d => d.priority).ToList();

            foreach (var category in categories)
            {
                if (!category.Settings.enabled)
                    continue;

                totalFractionMultiplier += category.Settings.targetFractionMultiplier;
                var limitedOverflowMultiplier = category.Settings.infiniteOverflow ? INFINITE_EXTRA_STANDIN : category.Settings.extraFractionMultiplier;
                totalExtraFractionMultiplier += limitedOverflowMultiplier;
                totalAllFractionMultiplier += category.Settings.targetFractionMultiplier + limitedOverflowMultiplier;

                if(category.Settings.targetFractionMultiplier > maxFractionMultiplier)
                    maxFractionMultiplier = category.Settings.targetFractionMultiplier;
                if(limitedOverflowMultiplier > maxExtraFractionMultiplier)
                    maxExtraFractionMultiplier = limitedOverflowMultiplier;
                if (category.Settings.targetFractionMultiplier + limitedOverflowMultiplier > maxAllFractionMultiplier)
                    maxAllFractionMultiplier = category.Settings.targetFractionMultiplier + limitedOverflowMultiplier;
                if (category.Settings.researchSpeedMultiplier > maxResearchSpeedMultiplier)
                    maxResearchSpeedMultiplier = category.Settings.researchSpeedMultiplier;
            }

            var graphRect = rect.ContractedBy(10f);
            var botAxis = graphRect.BottomPartPixels(20f);
            graphRect.height -= 30f;
            var fractionsRect = graphRect.LeftPart(0.6f);
            var researchSpeedRect = graphRect.RightPart(0.4f);
            float fractionWidthBase = fractionsRect.width / maxAllFractionMultiplier;
            var fractionHeight = fractionsRect.height / categories.Count;

            var centerLine = fractionsRect.x + fractionsRect.width;

            GUI.color = WindowBGBorderColor;
            Widgets.DrawBox(rect, 2, null);
            var hundredPercentOffset = centerLine + (researchSpeedRect.width * (1f / maxResearchSpeedMultiplier));

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawBoxSolid(botAxis.TopPartPixels(2f), GUI.color);
            Rect fractionAxis = botAxis.LeftPart(0.6f).OffsetBy(0f, 2f);
            Rect researchSpeedAxis = botAxis.RightPart(0.4f).OffsetBy(0f, 2f);
            Widgets.DrawBoxSolid(fractionAxis.LeftPartPixels(2f).OffsetBy(-1f, -1f), GUI.color);
            Widgets.DrawBoxSolid(researchSpeedAxis.RightPartPixels(2f).OffsetBy(1f, -1f), GUI.color);

            Widgets.DrawBoxSolid(new Rect(hundredPercentOffset - 1f, rect.y, 2f, botAxis.y - rect.y + 1f), GUI.color);


            Widgets.Label(fractionAxis, "RR_setting_category_fractionAxis".Translate());
            Widgets.Label(researchSpeedAxis, "RR_setting_category_researchSpeedAxis".Translate());

            GUI.color = Color.white;

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                var categoryY = fractionsRect.y + (i * fractionHeight);

                if (!category.Settings.enabled)
                {
                    GUI.color = Color.white;
                    Rect categoryOffRect = new Rect(researchSpeedRect.x , categoryY, fractionHeight, fractionHeight).ContractedBy(3f);
                    GUI.DrawTexture(categoryOffRect, Textures.forbiddenOverlayTex);
                }
                else
                {
                    var widthNorm = fractionWidthBase * category.Settings.targetFractionMultiplier;
                    if (category.Settings.infiniteOverflow)
                    {
                        var widthExtra = (fractionWidthBase * maxAllFractionMultiplier) - widthNorm;
                        Rect categoryFadeoutRect = new Rect(centerLine - (widthExtra + widthNorm), categoryY, widthExtra, fractionHeight);
                        GUI.color = Color.Lerp(category.color, Color.black, .75f);
                        GUI.DrawTextureWithTexCoords(categoryFadeoutRect, FadeGradient, new Rect(0, 0, 1, 1));
                    }
                    else
                    {
                        var widthExtra = fractionWidthBase * category.Settings.extraFractionMultiplier;
                        Rect categoryExtraRect = new Rect(centerLine - (widthNorm + widthExtra), categoryY, widthExtra, fractionHeight);
                        GUI.color = Color.Lerp(category.color, Color.black, .5f);
                        Widgets.DrawBoxSolidWithOutline(categoryExtraRect, Color.Lerp(category.color, Color.black, .9f), Color.Lerp(category.color, Color.black, .75f));
                    }

                    if (widthNorm > 0)
                    {
                        Rect categoryNormRect = new Rect(centerLine - widthNorm, categoryY, widthNorm, fractionHeight);
                        Widgets.DrawBoxSolidWithOutline(categoryNormRect, Color.Lerp(category.color, Color.black, .8f), Color.Lerp(category.color, Color.black, .5f));

                        float iterations = category.Settings.targetIterations;
                        float offset = 0f;
                        var outerColor = Color.Lerp(category.color, Color.black, .6f);
                        var innerColor = Color.Lerp(category.color, Color.black, .7f);
                        for (; iterations > 0; iterations -= 1f)
                        {
                            var width = Math.Min(1f, iterations) * (widthNorm / category.Settings.targetIterations);
                            var iterRect = categoryNormRect.RightPartPixels(width);
                            iterRect.x -= offset;
                            GUI.color = outerColor;
                            Widgets.DrawBox(iterRect.ContractedBy(1f));
                            offset += width;
                        }
                    }

                    if (category.Settings.enabled)
                    {
                        var widthSpeed = researchSpeedRect.width * (category.Settings.researchSpeedMultiplier / maxResearchSpeedMultiplier);
                        Rect speedRect = new Rect(researchSpeedRect.x, categoryY + (fractionHeight / 2f) - 2f, widthSpeed, 4f);
                        Widgets.DrawBoxSolid(speedRect, WindowBGBorderColor);
                        Rect speedEndCapRect = new Rect(researchSpeedRect.x + widthSpeed - 1f, categoryY + (fractionHeight / 2f) - 5f, 2f, 10f);
                        Widgets.DrawBoxSolid(speedEndCapRect, WindowBGBorderColor);
                        Rect speedLabelRect = new Rect(researchSpeedRect.x + 10f, categoryY, researchSpeedRect.width - 10f, fractionHeight);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        GUI.color = Color.white;
                        Widgets.Label(speedLabelRect, $"{(category.Settings.researchSpeedMultiplier * 100).ToString($"F0")}%");
                    }
                }

                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = category.color;
                Rect categoryLabelRect = new Rect(fractionsRect.x, categoryY, fractionsRect.width - 10f, fractionHeight);
                Widgets.Label(categoryLabelRect, category.LabelCap);

                Rect categoryRect = new Rect(graphRect.x, categoryY, graphRect.width, fractionHeight);
                if(Widgets.ButtonInvisible(categoryRect))
                {
                    ResearchReinventedMod.Settings.temp_activeTab = SettingTab.CATEGORY_CONFIG;
                    ResearchReinventedMod.Settings.temp_selectedCategory = category;
                    SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                }
            }

            GUI.color = WindowBGBorderColor;
            Widgets.DrawBoxSolid(new Rect(centerLine - 1f, rect.y, 2f, rect.height), GUI.color);

            Text.Anchor = anchor;
            GUI.color = color;
        }
    }
}
