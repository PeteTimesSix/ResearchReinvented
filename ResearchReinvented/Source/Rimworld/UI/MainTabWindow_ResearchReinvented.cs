using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.UI
{
    class MainTabWindow_ResearchReinvented : MainTabWindow
    {
        public Vector2 scrollPos = new Vector2(0f, 0f);
        public float innerRectSizeCache = 0f;

        public const float TITLEBAR_HEIGHT = 26f;
        public const float HEADER_ROW_HEIGHT = 25f;
        public const float ROW_HEIGHT = 40f;
        public const float ROW_GAP = 2f;
        public const float ICON_GAP = 5f;
        public const float ICON_SIZE = ROW_HEIGHT;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(500f, 700f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            var cachedStyle = Text.Font;
            var cachedAnchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            var fullRect = inRect;
            //Rect fullRect = new Rect(0f, TITLEBAR_HEIGHT, this.size.x, this.size.y - TITLEBAR_HEIGHT).Rounded();
            IReadOnlyCollection<ResearchOpportunityCategoryDef> opportunityCategories = ResearchOpportunityManager.instance.CurrentOpportunityCategories;
            IReadOnlyCollection<ResearchOpportunity> opportunities = ResearchOpportunityManager.instance.CurrentOpportunities;
            Rect sizedRect = new Rect(fullRect.x, fullRect.y, fullRect.width - 20f, fullRect.height).ContractedBy(2f).Rounded();
            sizedRect.height = innerRectSizeCache;

            /*Rect oddTextRect = new Rect(sizedRect.x + ICON_SIZE - OVERLAP_FIX, sizedRect.y, sizedRect.width - ((ICON_SIZE * 2) + ICON_GAP_OTHERSIDE), ROW_HEIGHT).Rounded();
            Rect oddIconRect = new Rect(sizedRect.x, sizedRect.y, ICON_SIZE, ICON_SIZE).Rounded();
            Rect evenTextRect = new Rect(sizedRect.x + ICON_SIZE + ICON_GAP_OTHERSIDE, sizedRect.y, sizedRect.width - ((ICON_SIZE * 2) + ICON_GAP_OTHERSIDE + OVERLAP_FIX), ROW_HEIGHT).Rounded();
            Rect evenIconRect = new Rect(evenTextRect.x + evenTextRect.width - OVERLAP_FIX, sizedRect.y - (ROW_HEIGHT + ROW_GAP), ICON_SIZE, ICON_SIZE).Rounded();*/
            Rect headerRect = new Rect(sizedRect.x, sizedRect.y, sizedRect.width, HEADER_ROW_HEIGHT).Rounded();
            Rect textRectStart = new Rect(sizedRect.x + ICON_SIZE + ICON_GAP, sizedRect.y, sizedRect.width - (ICON_SIZE + ICON_GAP), ROW_HEIGHT).Rounded();
            Rect iconRectStart = new Rect(sizedRect.x, sizedRect.y, ICON_SIZE, ICON_SIZE).Rounded();

            Widgets.BeginScrollView(fullRect, ref scrollPos, sizedRect);

            float heightTotal = 0f;

            foreach (var opportunityCategory in opportunityCategories.OrderByDescending(c => c.priority))
            {
                var matchingOpportunitites = opportunities.Where(o => o.def.GetCategory(o.relation) == opportunityCategory);
                if (matchingOpportunitites.Any())
                {
                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(headerRect.OffsetBy(0f, heightTotal), opportunityCategory.LabelCap);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    heightTotal += HEADER_ROW_HEIGHT + ROW_GAP;
                    bool odd = true;

                    foreach (var opportunity in matchingOpportunitites)
                    {
                        Color borderColor = odd ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.5f, 0.5f, 0.5f);
                        Color bgColor = odd ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.25f);
                        Color progressColor = odd ? new Color(0.2f, 0.2f, 0.8f) : new Color(0.25f, 0.25f, 0.8f);
                        //Rect textBox = odd ? oddTextRect : evenTextRect;
                        //Rect iconBox = odd ? oddIconRect : evenIconRect;
                        Rect textRect = textRectStart.OffsetBy(0f, heightTotal);
                        Rect iconRect = iconRectStart.OffsetBy(0f, heightTotal);

                        Widgets.DrawBoxSolid(iconRect, borderColor);
                        Widgets.DrawBoxSolid(iconRect.ContractedBy(2f).Rounded(), bgColor);
                        Rect iconBoxInner = iconRect.ContractedBy(5f).Rounded();
                        if (opportunity.requirement is ROComp_RequiresThing requiresThingComp)
                        {
                            Widgets.DefIcon(iconBoxInner, requiresThingComp.targetDef);
                        }
                        else if (opportunity.requirement is ROComp_RequiresIngredients requiresIngredientsComp)
                        {
                            var ingredients = requiresIngredientsComp.ingredients;
                            for(int i = 0; i < ingredients.Count; i++)
                            {
                                var ingredient = ingredients[i];
                                var ingredientBox = iconBoxInner.ContractedBy(ICON_SIZE / 4f).OffsetBy(
                                    (float)(ICON_SIZE / 4 * Math.Sin(i / ingredients.Count * Math.PI)),
                                    (float)(ICON_SIZE / 4 * Math.Cos(i / ingredients.Count * Math.PI))).Rounded();
                                if(ingredient.Value.IsFixedIngredient)
                                    Widgets.DefIcon(iconBoxInner, ingredient.Value.FixedIngredient);
                            }
                        }
                        else if (opportunity.requirement is ROComp_RequiresTerrain requiresTerrainComp)
                        {
                            Widgets.DefIcon(iconBoxInner, requiresTerrainComp.terrainDef);
                        }
                        else 
                        {
                            Widgets.DrawTextureFitted(iconBoxInner, Textures.scienceIcon, 1f);
                        }

                        Widgets.DrawBoxSolid(textRect, borderColor);
                        Widgets.DrawBoxSolid(textRect.ContractedBy(2f).Rounded(), bgColor);
                        var progressRect = new Rect(textRect).ContractedBy(2f).Rounded();
                        progressRect.width *= opportunity.ProgressFraction;
                        Widgets.DrawBoxSolid(progressRect, progressColor);
                        Rect textBoxInternal = textRect.ContractedBy(2f, 0f).Rounded();
                        Widgets.Label(textBoxInternal.TopHalf().Rounded(), $"{opportunity.def.GetHeaderCap(opportunity.relation)}");
                        Widgets.Label(textBoxInternal.BottomHalf().Rounded(), $"{opportunity.requirement.ShortDesc.CapitalizeFirst()}");
                        Text.Anchor = TextAnchor.MiddleRight;
                        if(!opportunity.def.GetCategory(opportunity.relation).infiniteOverflow)
                            Widgets.Label(textBoxInternal.BottomHalf().Rounded(), $"{Math.Round(opportunity.ProgressFraction * 100, 1)}%");
                        
                        Widgets.Label(textBoxInternal.TopHalf().Rounded(), $"{Math.Round(opportunity.Progress, 0)} / {Math.Round(opportunity.MaximumProgress, 0)}");

                        Text.Anchor = TextAnchor.MiddleLeft;

                        heightTotal += ROW_HEIGHT + ROW_GAP;
                        odd = !odd;
                    }
                    //if (!odd)
                    //    heightTotal += ROW_HEIGHT + ROW_GAP;
                }
            }
            innerRectSizeCache = heightTotal;

            Widgets.EndScrollView();

            Text.Font = cachedStyle;
            Text.Anchor = cachedAnchor;
        }
    }
}
