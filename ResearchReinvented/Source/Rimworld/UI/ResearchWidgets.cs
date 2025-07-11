using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.UI
{
    public static class ResearchWidgets
    {
        public static void ThingDefIcon(Rect iconBox, ThingDef thingDef, bool doButton = true)
        {
            if (thingDef.uiIcon != null && thingDef.uiIcon != BaseContent.BadTex)
            {
                Widgets.DefIcon(iconBox, thingDef); //has own icon. Use DefIcon when possible to preserve colors
            }
            else
            {
                if (thingDef.IsCorpse && thingDef.ingestible?.sourceDef?.uiIcon != null && thingDef.ingestible?.sourceDef?.uiIcon != BaseContent.BadTex)
                {
                    Widgets.DefIcon(iconBox, thingDef.ingestible.sourceDef); //is a corpse and the pawn has an icon
                }
                else if (thingDef.race != null || thingDef.IsCorpse)
                    Widgets.DrawTextureFitted(iconBox, Textures.genericPawnIcon, 1f, 1f); //is a corspe or a pawn with no icon
                else
                    Widgets.DrawTextureFitted(iconBox, Textures.genericIcon, 1f, 1f); //isnt pawn-like and has no icon??
            }
            if (thingDef.IsCorpse)
            {
                var shrunkBox = new Rect(iconBox.x + iconBox.width / 2, iconBox.y + iconBox.height / 2, iconBox.width / 2, iconBox.height / 2);

                Widgets.DrawTextureFitted(shrunkBox, Textures.corpseIconOverlay, 1f, 1f);
            }
            if (doButton && Widgets.ButtonInvisible(iconBox))
                Find.WindowStack.Add(new Dialog_InfoCard(thingDef));
        }

        internal static void TerrainDefIcon(Rect iconBox, TerrainDef terrainDef, bool doButton = true)
        {
            Widgets.DefIcon(iconBox, terrainDef);
            if (doButton && Widgets.ButtonInvisible(iconBox))
                Find.WindowStack.Add(new Dialog_InfoCard(terrainDef));
        }

        internal static void RecipeDefIcon(Rect iconBox, RecipeDef recipeDef, bool doButton = true)
        {
            if (recipeDef.UIIconThing != null)
            {
                Widgets.ThingIcon(iconBox, recipeDef.UIIconThing);
            }
            else
            {
                bool hadThingIcon = false;
                {
                    ThingDef fixedIngredient = null;
                    if (recipeDef.ingredients.Count == 1 && recipeDef.ingredients[0].IsFixedIngredient)
                    {
                        fixedIngredient = recipeDef.ingredients[0].FixedIngredient;
                    }
                    else
                    {
                        var fixedIngredients = recipeDef.ingredients.Where(i => i.IsFixedIngredient).ToList();
                        if (fixedIngredients.Count == 1)
                        {
                            fixedIngredient = fixedIngredients[0].FixedIngredient;
                        }
                    }
                    if (fixedIngredient != null && fixedIngredient.uiIcon != null && fixedIngredient.uiIcon != BaseContent.BadTex)
                    {
                        hadThingIcon = true;
                        Widgets.DefIcon(iconBox, fixedIngredient); //has own icon. Use DefIcon when possible to preserve colors
                    }
                }
                if (!hadThingIcon)
                {
                    if (recipeDef.IsSurgery)
                    {
                        Widgets.DrawTextureFitted(iconBox, Textures.medicalInvasiveIcon, 1f, 1f);
                    }
                    else
                    {
                        Widgets.DrawTextureFitted(iconBox, Textures.medicalNoninvasiveIcon, 1f, 1f);
                    }
                }
            }

            if (doButton && Widgets.ButtonInvisible(iconBox))
                Find.WindowStack.Add(new Dialog_InfoCard(recipeDef));
        }
    }
}
