using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public static class AlternatesKeeper
    {
        public static Dictionary<ThingDef, ThingDef[]> alternatesEquivalent = new();
        public static Dictionary<TerrainDef, TerrainDef[]> alternateEquivalentTerrains = new();
        public static Dictionary<ThingDef, ThingDef[]> alternatesSimilar = new();
        public static Dictionary<TerrainDef, TerrainDef[]> alternateSimilarTerrains = new();
        public static Dictionary<RecipeDef, RecipeDef[]> alternateEquivalentRecipes = new();
        public static Dictionary<RecipeDef, RecipeDef[]> alternateSimilarRecipes = new();

        public static void PrepareAlternates()
        {
            Dictionary<ThingDef, HashSet<ThingDef>> altsEquivalentDict = new();
            Dictionary<TerrainDef, HashSet<TerrainDef>> altTerrainsEquivalentDict = new();
            Dictionary<RecipeDef, HashSet<RecipeDef>> altRecipesEquivalentDict = new();
            Dictionary<ThingDef, HashSet<ThingDef>> altsSimilarDict = new();
            Dictionary<TerrainDef, HashSet<TerrainDef>> altTerrainsSimilarDict = new();
            Dictionary<RecipeDef, HashSet<RecipeDef>> altRecipesSimilarDict = new();
            foreach (var altsDef in DefDatabase<AlternateResearchSubjectsDef>.AllDefsListForReading)
            {
                if(altsDef.originals != null)
                {
                    foreach (var original in altsDef.originals)
                    {
                        if(altsDef.alternatesEquivalent != null)
                        {
                            if (!altsEquivalentDict.ContainsKey(original))
                                altsEquivalentDict[original] = new();
                            altsEquivalentDict[original].AddRange(altsDef.alternatesEquivalent);
                        }
                        if (altsDef.alternatesSimilar != null)
                        {
                            if (!altsSimilarDict.ContainsKey(original))
                                altsSimilarDict[original] = new();
                            altsSimilarDict[original].AddRange(altsDef.alternatesSimilar);
                        }
                    }
                }
                if (altsDef.originalTerrains != null)
                {
                    foreach (var originalTerrain in altsDef.originalTerrains)
                    {
                        if (altsDef.alternateEquivalentTerrains != null)
                        {
                            if (!altTerrainsEquivalentDict.ContainsKey(originalTerrain))
                                altTerrainsEquivalentDict[originalTerrain] = new();
                            altTerrainsEquivalentDict[originalTerrain].AddRange(altsDef.alternateEquivalentTerrains);
                        }
                        if (altsDef.alternateSimilarTerrains != null)
                        {
                            if (!altTerrainsSimilarDict.ContainsKey(originalTerrain))
                                altTerrainsSimilarDict[originalTerrain] = new();
                            altTerrainsSimilarDict[originalTerrain].AddRange(altsDef.alternateSimilarTerrains);
                        }
                    }
                }
                if (altsDef.originalRecipes != null)
                {
                    foreach (var originalRecipe in altsDef.originalRecipes)
                    {
                        if (altsDef.alternateEquivalentRecipes != null)
                        {
                            if (!altRecipesEquivalentDict.ContainsKey(originalRecipe))
                                altRecipesEquivalentDict[originalRecipe] = new();
                            altRecipesEquivalentDict[originalRecipe].AddRange(altsDef.alternateEquivalentRecipes);
                        }
                        if (altsDef.alternateSimilarRecipes != null)
                        {
                            if (!altRecipesSimilarDict.ContainsKey(originalRecipe))
                                altRecipesSimilarDict[originalRecipe] = new();
                            altRecipesSimilarDict[originalRecipe].AddRange(altsDef.alternateSimilarRecipes);
                        }
                    }
                }
            }
            foreach(var (original, alternatesSet) in altsEquivalentDict)
            {
                alternatesEquivalent[original] = alternatesSet.ToArray();
            }
            foreach (var (original, alternatesSet) in altsSimilarDict)
            {
                alternatesSimilar[original] = alternatesSet.ToArray();
            }
            foreach (var (originalTerrain, alternatesSet) in altTerrainsEquivalentDict)
            {
                alternateEquivalentTerrains[originalTerrain] = alternatesSet.ToArray();
            }
            foreach (var (originalTerrain, alternatesSet) in altTerrainsSimilarDict)
            {
                alternateSimilarTerrains[originalTerrain] = alternatesSet.ToArray();
            }
            foreach (var (originalRecipe, alternatesSet) in altRecipesEquivalentDict)
            {
                alternateEquivalentRecipes[originalRecipe] = alternatesSet.ToArray();
            }
            foreach (var (originalRecipe, alternatesSet) in altRecipesSimilarDict)
            {
                alternateSimilarRecipes[originalRecipe] = alternatesSet.ToArray();
            }
        }
    }
}
