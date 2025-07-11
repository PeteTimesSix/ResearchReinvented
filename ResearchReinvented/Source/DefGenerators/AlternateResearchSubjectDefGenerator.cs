using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.DefGenerators
{
    public static class AlternateResearchSubjectDefGenerator
    {
        public static Dictionary<TerrainTemplateDef, List<TerrainDef>> carpets = new();
        public static Dictionary<ThingDef, RecipeDef[]> recipeMakerRecipes = new();

        public static IEnumerable<AlternateResearchSubjectsDef> AncientAlternateDefs()
		{
            string str = "ancient";
            return AlternateDefsOfDefNamePrefix<ThingDef>(str, false, true);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> AdvancedAlternateDefs()
        {
            string str = "advanced";
            return AlternateDefsOfDefNamePrefix<ThingDef>(str, false, true);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> UniqueAlternateDefs()
        {
            string str = "unique";
            return AlternateDefsOfDefNamePostfix<ThingDef>(str, false, true);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> BulkRecipes()
        {
            string str = "bulk";
            return AlternateDefsOfDefNamePostfix<RecipeDef>(str, true, true);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> WildPlantsAlternateDefs()
        {
            string str = "wild";
            return AlternateDefsOfDefNamePostfix<ThingDef>(str, false, true, (Def d) => d is ThingDef td && td.plant != null);
        }

        public static IEnumerable<AlternateResearchSubjectsDef> Carpets()
        {
            foreach (var (template, terrainDefs) in carpets)
            {
                //Log.Message($"Carpets for {template.defName}: " + string.Join(",", terrainDefs.Select(td => td.defName)));
                if (terrainDefs.Count > 0)
                {
                    var master = terrainDefs.First();
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_carpet_{template.defName}";
                    altDef.originalTerrains = new List<TerrainDef>() { master };
                    altDef.alternateEquivalentTerrains = terrainDefs.Skip(1).ToList();
                    altDef.alternateSimilarTerrains = altDef.alternateEquivalentTerrains;
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;
                    yield return altDef;
                }
            }
            //discard carpets
            carpets = null;
        }

        public static IEnumerable<AlternateResearchSubjectsDef> RecipeMakers()
        {
            foreach (var (thing, recipes) in recipeMakerRecipes)
            {
                if (recipes.Length > 1)
                {
                    var master = recipes.First();
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_recipeMaker_{master.defName}";
                    altDef.originalRecipes = new List<RecipeDef>() { master };
                    altDef.alternateEquivalentRecipes = recipes.Skip(1).ToList();
                    altDef.alternateSimilarRecipes = altDef.alternateEquivalentRecipes;
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;
                    yield return altDef;
                }
            }
            //discard recipes
            recipeMakerRecipes = null;
        }



        public static IEnumerable<AlternateResearchSubjectsDef> AlternateDefsOfDefNamePrefix<T>(string prefix, bool equivalent, bool similar, Func<Def, bool> additonalValidator = null) where T: Def
        {
            var matchingThings = DefDatabase<T>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant().StartsWith(prefix));
            foreach (var matchingThing in matchingThings)
            {
                if (additonalValidator != null && !additonalValidator(matchingThing))
                    continue;
                var alternateFullString = matchingThing.defName.ToLowerInvariant();
                var alternateNameIndex = alternateFullString.IndexOf(prefix);
                if (alternateNameIndex == -1)
                    continue;
                alternateNameIndex += prefix.Length;
                if (alternateNameIndex >= alternateFullString.Length - 5) //lets avoid picking up single letters
                    continue;
                var alternateName = alternateFullString.Substring(alternateNameIndex);
                if (alternateName[0] == '_') //sometimes people separate with underscores
                    alternateName = alternateName.Substring(1);

                var matchingTargetThings = DefDatabase<T>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant() == alternateName && d != matchingThing);
                foreach (var targetThing in matchingTargetThings)
                {
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_pre_{prefix}_{matchingThing.defName}_{targetThing}";
                    if(typeof(T) == typeof(ThingDef))
                    {
                        altDef.originals = new List<ThingDef>() { targetThing as ThingDef };
                        if (equivalent)
                            altDef.alternatesEquivalent = new List<ThingDef>() { matchingThing as ThingDef };
                        if(similar)
                            altDef.alternatesSimilar = new List<ThingDef>() { matchingThing as ThingDef };
                    }
                    else if(typeof(T) == typeof(TerrainDef))
                    {
                        altDef.originalTerrains = new List<TerrainDef>() { targetThing as TerrainDef };
                        if (equivalent)
                            altDef.alternateEquivalentTerrains = new List<TerrainDef>() { matchingThing as TerrainDef };
                        if (similar)
                            altDef.alternateSimilarTerrains = new List<TerrainDef>() { matchingThing as TerrainDef };
                    }
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;

                    //Log.Message($"Associating {prefixString} {matchingThing} with {targetThing} as ({altDef.defName})");
                    yield return altDef;
                }
            }
        }

        public static IEnumerable<AlternateResearchSubjectsDef> AlternateDefsOfDefNamePostfix<T>(string postfix, bool equivalent, bool similar, Func<Def, bool> additonalValidator = null) where T : Def
        {
            var matchingThings = DefDatabase<T>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant().EndsWith(postfix));
            foreach (var matchingThing in matchingThings)
            {
                if (additonalValidator != null && !additonalValidator(matchingThing))
                    continue;
                var alternateFullString = matchingThing.defName.ToLowerInvariant();
                var alternateNameIndex = alternateFullString.IndexOf(postfix);
                if (alternateNameIndex == -1)
                    continue;
                if (alternateNameIndex <= 5) //lets avoid picking up single letters
                    continue;
                var alternateName = alternateFullString.Substring(0, alternateNameIndex);
                if (alternateName[alternateName.Length - 1] == '_') //sometimes people separate with underscores
                    alternateName = alternateName.Substring(0, alternateName.Length - 1);

                var matchingTargetThings = DefDatabase<T>.AllDefsListForReading.Where(d => d.defName.ToLowerInvariant() == alternateName && d != matchingThing);
                foreach (var targetThing in matchingTargetThings)
                {
                    if (additonalValidator != null && !additonalValidator(matchingThing))
                        continue;
                    AlternateResearchSubjectsDef altDef = new AlternateResearchSubjectsDef();
                    altDef.defName = $"RR_auto_post_{postfix}_{matchingThing.defName}_{targetThing}";
                    if (typeof(T) == typeof(ThingDef))
                    {
                        altDef.originals = new List<ThingDef>() { targetThing as ThingDef };
                        if (equivalent)
                            altDef.alternatesEquivalent = new List<ThingDef>() { matchingThing as ThingDef };
                        if (similar)
                            altDef.alternatesSimilar = new List<ThingDef>() { matchingThing as ThingDef };
                    }
                    else if (typeof(T) == typeof(TerrainDef))
                    {
                        altDef.originalTerrains = new List<TerrainDef>() { targetThing as TerrainDef };
                        if (equivalent)
                            altDef.alternateEquivalentTerrains = new List<TerrainDef>() { matchingThing as TerrainDef };
                        if (similar)
                            altDef.alternateSimilarTerrains = new List<TerrainDef>() { matchingThing as TerrainDef };
                    }
                    else if (typeof(T) == typeof(RecipeDef))
                    {
                        altDef.originalRecipes = new List<RecipeDef>() { targetThing as RecipeDef };
                        if (equivalent)
                            altDef.alternateEquivalentRecipes = new List<RecipeDef>() { matchingThing as RecipeDef };
                        if (similar)
                            altDef.alternateSimilarRecipes = new List<RecipeDef>() { matchingThing as RecipeDef };
                    }
                    altDef.modContentPack = ResearchReinventedMod.ModSingleton.Content;

                    //Log.Message($"Associating {prefixString} {matchingThing} with {targetThing} as ({altDef.defName})");
                    yield return altDef;
                }
            }
        }
    }
}
