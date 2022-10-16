using PeteTimesSix.ResearchReinvented.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

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
                if (!category.enabled)
                    continue;

                totalFractionMultiplier += category.targetFractionMultiplier;
                var limitedOverflowMultiplier = category.infiniteOverflow ? INFINITE_EXTRA_STANDIN : category.extraFractionMultiplier;
                totalExtraFractionMultiplier += limitedOverflowMultiplier;
                totalAllFractionMultiplier += category.targetFractionMultiplier + limitedOverflowMultiplier;

                if(category.targetFractionMultiplier > maxFractionMultiplier)
                    maxFractionMultiplier = category.targetFractionMultiplier;
                if(limitedOverflowMultiplier > maxExtraFractionMultiplier)
                    maxExtraFractionMultiplier = limitedOverflowMultiplier;
                if (category.targetFractionMultiplier + limitedOverflowMultiplier > maxAllFractionMultiplier)
                    maxAllFractionMultiplier = category.targetFractionMultiplier + limitedOverflowMultiplier;
                if (category.researchSpeedMultiplier > maxResearchSpeedMultiplier)
                    maxResearchSpeedMultiplier = category.researchSpeedMultiplier;
            }

            var graphRect = rect.ContractedBy(10f);
            var botAxis = graphRect.BottomPartPixels(20f);
            graphRect.height -= 30f;
            var fractionsRect = graphRect.LeftPart(0.6f);
            var researchSpeedRect = graphRect.RightPart(0.4f);
            float fractionWidthBase = fractionsRect.width / maxAllFractionMultiplier;
            var fractionHeight = fractionsRect.height / categories.Count;

            var centerLine = fractionsRect.x + fractionsRect.width;

            for(int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                var categoryY = fractionsRect.y + (i * fractionHeight);

                if (!category.enabled)
                {
                    Rect categoryOffRect = new Rect(fractionsRect.x + fractionsRect.width - fractionHeight, categoryY, fractionHeight, fractionHeight).ContractedBy(3f);
                    Widgets.DrawBoxSolid(categoryOffRect, Color.red);
                }
                else
                {
                    var widthNorm = fractionWidthBase * category.targetFractionMultiplier;
                    if (category.infiniteOverflow)
                    {
                        var widthExtra = (fractionWidthBase * maxAllFractionMultiplier) - widthNorm;
                        Rect categoryFadeoutRect = new Rect(centerLine - (widthExtra + widthNorm), categoryY, widthExtra, fractionHeight);
                        GUI.color = Color.Lerp(category.color, Color.black, .75f);
                        GUI.DrawTextureWithTexCoords(categoryFadeoutRect, FadeGradient, new Rect(0, 0, 1, 1));
                    }
                    else
                    {
                        var widthExtra = fractionWidthBase * category.extraFractionMultiplier;
                        Rect categoryExtraRect = new Rect(centerLine - (widthNorm + widthExtra), categoryY, widthExtra, fractionHeight);
                        GUI.color = Color.Lerp(category.color, Color.black, .5f);
                        Widgets.DrawBoxSolidWithOutline(categoryExtraRect, Color.Lerp(category.color, Color.black, .9f), Color.Lerp(category.color, Color.black, .75f));
                    }

                    if (widthNorm > 0)
                    {
                        Rect categoryNormRect = new Rect(centerLine - widthNorm, categoryY, widthNorm, fractionHeight);
                        Widgets.DrawBoxSolidWithOutline(categoryNormRect, Color.Lerp(category.color, Color.black, .8f), Color.Lerp(category.color, Color.black, .5f));

                        float iterations = category.targetIterations;
                        float offset = 0f;
                        var outerColor = Color.Lerp(category.color, Color.black, .6f);
                        var innerColor = Color.Lerp(category.color, Color.black, .7f);
                        for (; iterations > 0; iterations -= 1f)
                        {
                            var width = Math.Min(1f, iterations) * (widthNorm / category.targetIterations);
                            var iterRect = categoryNormRect.RightPartPixels(width);
                            iterRect.x -= offset;
                            GUI.color = outerColor;
                            Widgets.DrawBox(iterRect.ContractedBy(1f));
                            offset += width;
                        }
                    }

                    if (category.enabled)
                    {
                        var widthSpeed = researchSpeedRect.width * (category.researchSpeedMultiplier / maxResearchSpeedMultiplier);
                        Rect speedRect = new Rect(researchSpeedRect.x, categoryY + (fractionHeight / 2f) - 2f, widthSpeed, 4f);
                        Widgets.DrawBoxSolid(speedRect, WindowBGBorderColor);
                        Rect speedEndCapRect = new Rect(researchSpeedRect.x + widthSpeed - 1f, categoryY + (fractionHeight / 2f) - 5f, 2f, 10f);
                        Widgets.DrawBoxSolid(speedEndCapRect, WindowBGBorderColor);
                        Rect speedLabelRect = new Rect(researchSpeedRect.x + 10f, categoryY, widthSpeed, fractionHeight);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        GUI.color = Color.white;
                        Widgets.Label(speedLabelRect, $"{(category.researchSpeedMultiplier * 100).ToString($"F0")}%");
                    }
                }

                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = category.color;
                Rect categoryLabelRect = new Rect(fractionsRect.x, categoryY, fractionsRect.width - 10f, fractionHeight);
                Widgets.Label(categoryLabelRect, category.LabelCap);

                Rect categoryRect = new Rect(graphRect.x, categoryY, graphRect.width, fractionHeight);
                if(Widgets.ButtonInvisible(categoryRect))
                {
                    ResearchReinventedMod.Settings.temp_selectedCategory = category;
                }
            }

            GUI.color = WindowBGBorderColor;
            Widgets.DrawBox(rect, 1, null);
            Widgets.DrawLine(new Vector2(centerLine, rect.x), new Vector2(centerLine, rect.x + rect.height), GUI.color, 1f);
            var hundredPercentOffset = centerLine + (researchSpeedRect.width * (1f / maxResearchSpeedMultiplier));
            Widgets.DrawLine(new Vector2(hundredPercentOffset, rect.x), new Vector2(hundredPercentOffset, botAxis.y), GUI.color, 1f);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawLine(new Vector2(botAxis.x, botAxis.y - 1f), new Vector2(botAxis.x + botAxis.width, botAxis.y - 1f), GUI.color, 2f);
            //botAxis.y += 2f;
            Rect fractionAxis = botAxis.LeftPart(0.6f);
            Rect researchSpeedAxis = botAxis.RightPart(0.4f);
            Widgets.DrawLine(new Vector2(fractionAxis.x, botAxis.y), new Vector2(fractionAxis.x, botAxis.y + 10f), GUI.color, 1f);
            Widgets.DrawLine(new Vector2(researchSpeedAxis.x + researchSpeedAxis.width, botAxis.y), new Vector2(researchSpeedAxis.x + researchSpeedAxis.width, botAxis.y + 10f), GUI.color, 1f);
            Widgets.Label(fractionAxis, "RR_setting_category_fractionAxis".Translate());
            Widgets.Label(researchSpeedAxis, "RR_setting_category_researchSpeedAxis".Translate());

            Text.Anchor = anchor;
            GUI.color = color;
        }
    }
}
