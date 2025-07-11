using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld.UI.Dialogs
{
	public enum AltMapperMode
	{
        ALL_THINGS,
		BUILDABLE_THINGS,
		CRAFTABLE_THINGS,
        PLANTS,
        ALL_TERRAINS,
		BUILDABLE_TERRAINS,
        RECIPES
	}

	public class Dialog_AlternateMapper : Window
	{
		private float viewHeight;

		private Def selectedDef = null;
        private Def selectedAlternateDef = null;

        private HashSet<ThingDef> newAlternatesEquivalentForSelected = new();
        private HashSet<TerrainDef> newAlternateTerrainsEquivalentForSelected = new();
        private HashSet<RecipeDef> newAlternateRecipesEquivalentForSelected = new();
        private HashSet<ThingDef> newAlternateSimilarForSelected = new();
        private HashSet<TerrainDef> newAlternateTerrainSimilarForSelected = new();
        private HashSet<RecipeDef> newAlternateRecipesSimilarForSelected = new();
        private AltMapperMode mode = AltMapperMode.ALL_THINGS;

        private List<Def> originalsDefs;
        private List<Def> alternatesDefs;

        private QuickSearchWidget searchWidgetOriginal = new();
		private QuickSearchWidget searchWidgetAlternate = new();

		public Vector2 scrollPosOriginal = new Vector2(0f, 0f);
		public float sizeCacheOriginal = 0f;
		public Vector2 scrollPosAlternate = new Vector2(0f, 0f);
		public float sizeCacheAlternate = 0f;

		public const float SCROLLBAR_WIDTH = MainTabWindow_ResearchReinvented.SCROLLBAR_WIDTH;
		public const float ICON_SIZE = MainTabWindow_ResearchReinvented.ICON_SIZE;
		//public const float ROW_HEIGHT = 25f;

		public override Vector2 InitialSize => new Vector2(Mathf.Min(1000f, Verse.UI.screenWidth), Mathf.Min(1000f, Verse.UI.screenHeight));

		public Dialog_AlternateMapper()
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;

			UpdateDefsFilter();
		}

		public override void PostClose()
		{
			base.PostClose();
		}

		public override void DoWindowContents(Rect inRect)
		{
			inRect.height -= Window.CloseButSize.y;
			inRect.y += Window.CloseButSize.y;

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.EnumSelector(null, ref mode, "", onChange: UpdateDefsFilter);

            var buttonRect = listingStandard.GetRect(25);
			if(Widgets.ButtonText(buttonRect, "Generate alternates XML"))
			{
				GenerateAlternatesXML();
            }

            var labelsRect = listingStandard.GetRect(25);
            Widgets.Label(labelsRect.RightPartPixels(ICON_SIZE * 4).LeftHalf(), "Similar");
            Widgets.Label(labelsRect.RightPartPixels(ICON_SIZE * 4).RightHalf(), "Equivalent");

            var restRect = listingStandard.GetRect(inRect.height - listingStandard.CurHeight);
            var searchRect = restRect.TopPartPixels(25);
            var listsRect = restRect.BottomPartPixels(restRect.height - 25);
            {
				searchWidgetOriginal.OnGUI(searchRect.LeftHalf(), UpdateFilterOriginal, UpdateFilterOriginal);
				DrawDefOriginalsListing(listsRect.LeftHalf(), originalsDefs);
			}

			if(selectedDef != null)
			{
				searchWidgetAlternate.OnGUI(searchRect.RightHalf(), UpdateFilterAlternate, UpdateFilterAlternate);
				DrawDefAlternatesListing(listsRect.RightHalf(), alternatesDefs);
            }

            listingStandard.End();
        }

        private IEnumerable<ThingDef> ReasonableThingDefs()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading.Where(d => !d.IsBlueprint && !d.IsFrame && !d.IsFilth && d.mote == null && d.projectile == null && d.defName != "Flashstorm");
        }

        private void UpdateDefsFilter()
        {
			switch(mode)
			{
				case AltMapperMode.ALL_THINGS:
                    alternatesDefs = ReasonableThingDefs().Select(td => td as Def).ToList();
                    originalsDefs = ReasonableThingDefs().Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.BUILDABLE_THINGS:
                    alternatesDefs = ReasonableThingDefs().Select(td => td as Def).ToList();
                    originalsDefs = ReasonableThingDefs().Where(d => d is BuildableDef buildable && buildable.BuildableByPlayer).Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.CRAFTABLE_THINGS:
                    alternatesDefs = ReasonableThingDefs().Select(td => td as Def).ToList();
                    var recipeDefs = DefDatabase<RecipeDef>.AllDefsListForReading;
					HashSet<ThingDef> recipeProducts = new();
					foreach(var recipe in recipeDefs)
					{
						if (recipe.products != null)
							foreach (var product in recipe.products)
								recipeProducts.Add(product.thingDef);
					}
                    originalsDefs = recipeProducts.Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.PLANTS:
                    alternatesDefs = ReasonableThingDefs().Select(td => td as Def).ToList();
                    originalsDefs = ReasonableThingDefs().Where(d => d is ThingDef thingDef && thingDef.plant != null).Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.ALL_TERRAINS:
                    alternatesDefs = DefDatabase<TerrainDef>.AllDefsListForReading.Select(td => td as Def).ToList();
                    originalsDefs = DefDatabase<TerrainDef>.AllDefsListForReading.Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.BUILDABLE_TERRAINS:
                    alternatesDefs = DefDatabase<TerrainDef>.AllDefsListForReading.Select(td => td as Def).ToList();
                    originalsDefs = DefDatabase<TerrainDef>.AllDefsListForReading.Where(d => d is BuildableDef buildable && buildable.BuildableByPlayer).Select(td => td as Def).ToList();
                    break;
                case AltMapperMode.RECIPES:
                    alternatesDefs = DefDatabase<RecipeDef>.AllDefsListForReading.Select(td => td as Def).ToList();
                    originalsDefs = DefDatabase<RecipeDef>.AllDefsListForReading.Select(td => td as Def).ToList();
                    break;
            }
        }

        private void GenerateAlternatesXML()
        {
            if (selectedDef == null)
            {
                Log.Warning("RR: Selected def is null!");
                return;
            }
            if (selectedDef.modContentPack == null)
            {
                Log.Warning($"RR: Selected def {selectedDef.defName} modContentPack is null!");
            }
            HashSet<string> requiredPackageIds = new();
            CheckForMissingModContentPacks(newAlternatesEquivalentForSelected, "equivalent alts");
            CheckForMissingModContentPacks(newAlternateTerrainsEquivalentForSelected, "equivalent terrain alts");
            CheckForMissingModContentPacks(newAlternateRecipesEquivalentForSelected, "equivalent recipe alts");
            CheckForMissingModContentPacks(newAlternateSimilarForSelected, "similar alts");
            CheckForMissingModContentPacks(newAlternateTerrainSimilarForSelected, "similar terrain alts");
            CheckForMissingModContentPacks(newAlternateRecipesSimilarForSelected, "similar recipe alts");

            var allAlts = new HashSet<Def>();
            allAlts.AddRange(newAlternatesEquivalentForSelected);
            allAlts.AddRange(newAlternateTerrainsEquivalentForSelected);
            allAlts.AddRange(newAlternateRecipesEquivalentForSelected);
            allAlts.AddRange(newAlternateSimilarForSelected);
            allAlts.AddRange(newAlternateTerrainSimilarForSelected);
            allAlts.AddRange(newAlternateRecipesSimilarForSelected);

            var combinedHash = allAlts.Select(td => td.defNameHash).Aggregate((hash, acc) => HashCode.Combine(hash, acc));

            StringBuilder result = new StringBuilder($"\t<PeteTimesSix.ResearchReinvented.Defs.AlternateResearchSubjectsDef>\n");
            result.Append($"\t\t<defName>RR_alts_{combinedHash}_{selectedDef.defName}</defName>\n");
            if(selectedDef is ThingDef)
                AddListOfDefs(result, new Def[] { selectedDef }, "originals");
            else if (selectedDef is TerrainDef)
                AddListOfDefs(result, new Def[] { selectedDef }, "originalTerrains");
            else if (selectedDef is RecipeDef)
                AddListOfDefs(result, new Def[] { selectedDef }, "originalRecipes");
            if (newAlternatesEquivalentForSelected.Count > 0)
                AddListOfDefs(result, newAlternatesEquivalentForSelected, "alternatesEquivalent");
            if (newAlternateTerrainsEquivalentForSelected.Count > 0)
                AddListOfDefs(result, newAlternateTerrainsEquivalentForSelected, "alternateEquivalentTerrains");
            if (newAlternateRecipesEquivalentForSelected.Count > 0)
                AddListOfDefs(result, newAlternateRecipesEquivalentForSelected, "alternateEquivalentRecipes");
            if (newAlternateSimilarForSelected.Count > 0)
                AddListOfDefs(result, newAlternateSimilarForSelected, "alternatesSimilar");
            if (newAlternateTerrainSimilarForSelected.Count > 0)
                AddListOfDefs(result, newAlternateTerrainSimilarForSelected, "alternateSimilarTerrains");
            if (newAlternateRecipesSimilarForSelected.Count > 0)
                AddListOfDefs(result, newAlternateRecipesSimilarForSelected, "alternateSimilarRecipes");
            result.Append("\t</PeteTimesSix.ResearchReinvented.Defs.AlternateResearchSubjectsDef>\n\n");

            GUIUtility.systemCopyBuffer = result.ToString();
            //Log.Message(result.ToString());
            Messages.Message("Copied to clipboard", MessageTypeDefOf.NeutralEvent, historical: false);
        }

        private void AddListOfDefs(StringBuilder result, IEnumerable<Def> defs, string fieldName)
        {
            result.Append($"\t\t<{fieldName}>\n");
            foreach (var alt in defs)
            {
                if (alt.modContentPack?.IsCoreMod ?? true)
                    result.Append($"\t\t\t<li>{alt.defName}</li>\n");
                else
                    result.Append($"\t\t\t<li MayRequire=\"{alt.modContentPack.PackageId}\">{alt.defName}</li>\n");
            }
            result.Append($"\t\t</{fieldName}>\n");
        }

        private void CheckForMissingModContentPacks(IEnumerable<Def> collection, string label)
        {
            var noModContentPackDefs = collection.Where(a => a.modContentPack == null).Select(a => a.defName);
            if (noModContentPackDefs.Any())
                Log.Warning($"RR: Missing modContentPack for {label}: " + string.Join(",", noModContentPackDefs));
        }

        private void DrawDefOriginalsListing(Rect listRect, IEnumerable<Def> defsToDraw)
		{
			Rect internalRect = new Rect(listRect.x, listRect.y, listRect.width, listRect.height).Rounded();

            internalRect.height = sizeCacheOriginal;

			bool hasScrollbar = listRect.height < internalRect.height;
			if (hasScrollbar)
				internalRect.width -= SCROLLBAR_WIDTH; //clear space for scrollbar

			Widgets.BeginScrollView(listRect, ref scrollPosOriginal, internalRect);

			float heightTotal = 0f;

			foreach (var def in defsToDraw.Where(td => searchWidgetOriginal.filter.Matches(td.label) || searchWidgetOriginal.filter.Matches(td.defName)))
            {
                if (def is ThingDef thingDef)
                    DrawOriginalThingEntry(listRect, internalRect, hasScrollbar, ref heightTotal, thingDef);
                else if (def is TerrainDef terrainDef)
                    DrawOriginalTerrainEntry(listRect, internalRect, hasScrollbar, ref heightTotal, terrainDef);
                else if (def is RecipeDef recipeDef)
                    DrawOriginalRecipeEntry(listRect, internalRect, hasScrollbar, ref heightTotal, recipeDef);
                else
                    Log.Warning($"RR: unexpected def type '{def.GetType()}' for : {def.defName}");
			}
			sizeCacheOriginal = heightTotal;

			Widgets.EndScrollView();
		}


        private void DrawOriginalThingEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, ThingDef thingDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.ThingDefIcon(iconRect, thingDef);
            heightTotal = DrawOriginalSelectButton(heightTotal, thingDef, labelRect);
        }

        private void DrawOriginalTerrainEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, TerrainDef terrainDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.TerrainDefIcon(iconRect, terrainDef);
            heightTotal = DrawOriginalSelectButton(heightTotal, terrainDef, labelRect);
        }
        private void DrawOriginalRecipeEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, RecipeDef recipeDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.RecipeDefIcon(iconRect, recipeDef);
            heightTotal = DrawOriginalSelectButton(heightTotal, recipeDef, labelRect);
        }


        private float DrawOriginalSelectButton(float heightTotal, Def def, Rect labelRect)
        {
            if (def == selectedDef)
                GUI.color = Color.green;
            else if (selectedAlternateDef != null && AlternatesForDefContain(def, selectedAlternateDef))
                GUI.color = Color.cyan;
            if (Widgets.ButtonText(labelRect, def.defName + " / " + def.LabelCap))
            {
                if (selectedDef != def)
                    selectedDef = def;
                else
                    selectedDef = null;

                newAlternatesEquivalentForSelected.Clear();
                newAlternateTerrainsEquivalentForSelected.Clear();
                newAlternateRecipesEquivalentForSelected.Clear();
                newAlternateSimilarForSelected.Clear();
                newAlternateTerrainSimilarForSelected.Clear();
                newAlternateRecipesSimilarForSelected.Clear();
            }
            GUI.color = Color.white;


            heightTotal += ICON_SIZE;
            return heightTotal;
        }

        private bool AlternatesForDefContain(Def def, Def candidate)
        {
            if(def is ThingDef thingDef)
            {
                return AlternatesKeeper.alternatesEquivalent.TryGetValue(def as ThingDef, out ThingDef[] alts1) && alts1.Contains(candidate) ||
                    AlternatesKeeper.alternatesSimilar.TryGetValue(def as ThingDef, out ThingDef[] alts2) && alts2.Contains(candidate);
            }
            else if(def is TerrainDef terrainDef)
            {
                return AlternatesKeeper.alternateEquivalentTerrains.TryGetValue(def as TerrainDef, out TerrainDef[] alts1) && alts1.Contains(candidate) ||
                    AlternatesKeeper.alternateSimilarTerrains.TryGetValue(def as TerrainDef, out TerrainDef[] alts2) && alts2.Contains(candidate);
            }
            else if (def is RecipeDef recipeDef)
            {
                return AlternatesKeeper.alternateEquivalentRecipes.TryGetValue(def as RecipeDef, out RecipeDef[] alts1) && alts1.Contains(candidate) ||
                    AlternatesKeeper.alternateSimilarRecipes.TryGetValue(def as RecipeDef, out RecipeDef[] alts2) && alts2.Contains(candidate);
            }
            return false;
        }

        private void DrawDefAlternatesListing(Rect listRect, IEnumerable<Def> defsToDraw)
        {
            Rect internalRect = new Rect(listRect.x, listRect.y, listRect.width, listRect.height).Rounded();

            internalRect.height = sizeCacheAlternate;

            bool hasScrollbar = listRect.height < internalRect.height;
            if (hasScrollbar)
                internalRect.width -= SCROLLBAR_WIDTH; //clear space for scrollbar

            Widgets.BeginScrollView(listRect, ref scrollPosAlternate, internalRect);

            float heightTotal = 0f;

            foreach (var def in defsToDraw.Where(td => td != selectedDef && (searchWidgetAlternate.filter.Matches(td.label) || searchWidgetAlternate.filter.Matches(td.defName))))
            {
                if (def is ThingDef thingDef)
                    DrawAlternateThingEntry(listRect, internalRect, hasScrollbar, ref heightTotal, thingDef);
                else if (def is TerrainDef terrainDef)
                    DrawAlternateTerrainEntry(listRect, internalRect, hasScrollbar, ref heightTotal, terrainDef);
                else if (def is RecipeDef recipeDef)
                    DrawAlternateRecipeEntry(listRect, internalRect, hasScrollbar, ref heightTotal, recipeDef);
                else
                    Log.Warning($"RR: unexpected def type '{def.GetType()}' for : {def.defName}");
            }
            sizeCacheAlternate = heightTotal;

            Widgets.EndScrollView();
        }

        private void DrawAlternateThingEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, ThingDef thingDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.ThingDefIcon(iconRect, thingDef);
            heightTotal = DrawAlternateCheckbox(internalRect, hasScrollbar, heightTotal, thingDef, labelRect);
        }

        private void DrawAlternateTerrainEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, TerrainDef terrainDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.TerrainDefIcon(iconRect, terrainDef);
            heightTotal = DrawAlternateCheckbox(internalRect, hasScrollbar, heightTotal, terrainDef, labelRect);
        }

        private void DrawAlternateRecipeEntry(Rect wrapperRect, Rect internalRect, bool hasScrollbar, ref float heightTotal, RecipeDef recipeDef)
        {
            Rect iconRect = new Rect(internalRect.x, internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect labelRect = new Rect(internalRect.x + ICON_SIZE, internalRect.y + heightTotal, internalRect.width - ICON_SIZE, ICON_SIZE).Rounded();

            ResearchWidgets.RecipeDefIcon(iconRect, recipeDef);
            heightTotal = DrawAlternateCheckbox(internalRect, hasScrollbar, heightTotal, recipeDef, labelRect);
        }

        private float DrawAlternateCheckbox(Rect internalRect, bool hasScrollbar, float heightTotal, Def def, Rect labelRect)
        {
            if (selectedDef == null)
                return 0;

            //if (Widgets.ButtonText(labelRect, def.defName + " / " + def.LabelCap))
            //    selectedDef = def;
            if (def == selectedAlternateDef)
                GUI.color = Color.green;
            if (Widgets.ButtonTextSubtle(labelRect, def.defName + " / " + def.LabelCap))
            {
                if (selectedAlternateDef == def)
                    selectedAlternateDef = null;
                else
                    selectedAlternateDef = def;
            }
            GUI.color = Color.white;

            Rect checkboxAncestorRect = new Rect(internalRect.x + internalRect.width - ((ICON_SIZE * 2) + (hasScrollbar ? SCROLLBAR_WIDTH : 0)), internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            Rect checkboxRect = new Rect(internalRect.x + internalRect.width - (ICON_SIZE + (hasScrollbar ? SCROLLBAR_WIDTH : 0)), internalRect.y + heightTotal, ICON_SIZE, ICON_SIZE).Rounded();
            if (def is ThingDef thingDef)
            {
                if (newAlternateSimilarForSelected.Contains(thingDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternateSimilarForSelected.Remove(thingDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternatesSimilar.TryGetValue(selectedDef as ThingDef, out ThingDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, state, true);
                    if (retVal != state)
                        newAlternateSimilarForSelected.Add(thingDef);
                }

                if (newAlternatesEquivalentForSelected.Contains(thingDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternatesEquivalentForSelected.Remove(thingDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternatesEquivalent.TryGetValue(selectedDef as ThingDef, out ThingDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxRect, state, true);
                    if (retVal != state)
                        newAlternatesEquivalentForSelected.Add(thingDef);
                }
            }
            else if (def is TerrainDef terrainDef)
            {
                if (newAlternateTerrainSimilarForSelected.Contains(terrainDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternateTerrainSimilarForSelected.Remove(terrainDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternateSimilarTerrains.TryGetValue(selectedDef as TerrainDef, out TerrainDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, state, true);
                    if (retVal != state)
                        newAlternateTerrainSimilarForSelected.Add(terrainDef);
                }

                if (newAlternateTerrainsEquivalentForSelected.Contains(terrainDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternateTerrainsEquivalentForSelected.Remove(terrainDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternateEquivalentTerrains.TryGetValue(selectedDef as TerrainDef, out TerrainDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxRect, state, true);
                    if (retVal != state)
                        newAlternateTerrainsEquivalentForSelected.Add(terrainDef);
                }
            }
            else if (def is RecipeDef recipeDef)
            {
                if (newAlternateRecipesSimilarForSelected.Contains(recipeDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternateRecipesSimilarForSelected.Remove(recipeDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternateSimilarRecipes.TryGetValue(selectedDef as RecipeDef, out RecipeDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxAncestorRect, state, true);
                    if (retVal != state)
                        newAlternateRecipesSimilarForSelected.Add(recipeDef);
                }

                if (newAlternateRecipesEquivalentForSelected.Contains(recipeDef))
                {
                    var retVal = Widgets.CheckboxMulti(checkboxRect, MultiCheckboxState.On, true);
                    if (retVal != MultiCheckboxState.On)
                        newAlternateRecipesEquivalentForSelected.Remove(recipeDef);
                }
                else
                {
                    var isAlreadyAlt = AlternatesKeeper.alternateEquivalentRecipes.TryGetValue(selectedDef as RecipeDef, out RecipeDef[] alts) && alts.Contains(def);
                    var state = isAlreadyAlt ? MultiCheckboxState.Partial : MultiCheckboxState.Off;
                    var retVal = Widgets.CheckboxMulti(checkboxRect, state, true);
                    if (retVal != state)
                        newAlternateRecipesEquivalentForSelected.Add(recipeDef);
                }
            }

            heightTotal += ICON_SIZE;
            return heightTotal;
        }

        private void UpdateFilterOriginal() { 
			
		}

		private void UpdateFilterAlternate() {
			
		}
	}
}
