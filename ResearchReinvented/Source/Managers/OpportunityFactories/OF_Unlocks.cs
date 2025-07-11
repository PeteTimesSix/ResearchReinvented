using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Opportunities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public static class OF_Unlocks
    {
        private static Dictionary<ResearchProjectDef, HashSet<ResearchProjectDef>> ancestorsTree;
        public static Dictionary<ResearchProjectDef, HashSet<ResearchProjectDef>> AncestorsTree
        {
            get { 
                if(ancestorsTree == null)
                {
                    try
                    {
                        PrepareAncestryTree();
                    }
                    catch(RecursiveAncestorLoopException)
                    {
                        Log.Error($"RR: Reached a depth of 1000 while recursively crawling the tech tree! This almost certainly means there's a loop in it.");
                    }
                }
                return ancestorsTree; 
            }
        }
        private static void PrepareAncestryTree()
        {
            ancestorsTree = new();
            foreach(var project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
            {
                ancestorsTree[project] = GetAllPrerequisites(project, 0);
            }
        }

        private static HashSet<ResearchProjectDef> GetAllPrerequisites(ResearchProjectDef project, int depth)
        {
            if (depth > 1000)
                throw new RecursiveAncestorLoopException();

            if (!ancestorsTree.ContainsKey(project))
            {
                HashSet<ResearchProjectDef> newSet = new HashSet<ResearchProjectDef>();
                if (project.prerequisites != null)
                {
                    foreach (var prereq in project.prerequisites)
                    {
                        newSet.AddRange(GetAllPrerequisites(prereq, depth + 1));
                    }
                }
                if (project.hiddenPrerequisites != null)
                {
                    foreach (var prereq in project.hiddenPrerequisites)
                    {
                        newSet.AddRange(GetAllPrerequisites(prereq, depth + 1));
                    }
                }
                ancestorsTree[project] = newSet;
            }
            var returnSet = new HashSet<ResearchProjectDef> { project };
            returnSet.AddRange(ancestorsTree[project]);
            return returnSet;
        }
        private class RecursiveAncestorLoopException : InvalidOperationException { }

        public static void MakeFromUnlocks(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            if(project.UnlockedDefs != null)
            {
                HashSet<IngredientCount> ingredientCounts = new HashSet<IngredientCount>();

                foreach (var unlock in project.UnlockedDefs)
                {
                    if (!unlock.PassesIdeoCheck())
                    {
                        continue;
                    }

                    if (unlock is RecipeDef asRecipe)
                    {
                        //handled in OF_Recipes
                        //ingredientCounts.AddRange(asRecipe.ingredients);
                    }
                    else if (unlock is ThingDef asThing)
                    {
                        collections.forDirectAnalysis.Add(asThing);
                        if(asThing.researchPrerequisites == null)
                        {
                            //this must be coming from a recipe, so let the recipe handle it
                            continue;
                        }
                        if (asThing.CostList != null && asThing.CostList.Count > 0)
                        {
                            ingredientCounts.AddRange(asThing.CostList.Select(c => c.ToIngredientCount()));
                        }
                        if (asThing.CostStuffCount > 0)
                        {
                            var stuffs = GenStuff.AllowedStuffsFor(asThing);
                            ingredientCounts.AddRange(stuffs.Select(s => new ThingDefCountClass(s, asThing.costStuffCount).ToIngredientCount()));
                        }

                        if (!(asThing.researchPrerequisites?.Except(project)?.Any(p => !AncestorsTree[project]?.Contains(p) ?? true) ?? false))
                        {
                            collections.forPrototyping.Add(asThing);
                        }
                    }
                    else if (unlock is TerrainDef asTerrain)
                    {
                        collections.forDirectAnalysis.Add(asTerrain);
                        if (asTerrain.CostList != null && asTerrain.CostList.Count > 0)
                        {
                            ingredientCounts.AddRange(asTerrain.CostList.Select(c => c.ToIngredientCount()));
                        }

                        if (!(asTerrain.researchPrerequisites?.Except(project)?.Any(p => !AncestorsTree[project]?.Contains(p) ?? true) ?? false))
                            collections.forPrototyping.Add(asTerrain);
                    }
                    else
                    {
                        Log.Warning($"{project.defName} unlocks {unlock.defName} which is not a handled type of def");
                    }
                }

                var rawIngredients = new List<ThingDefCount>();
                foreach (var ingredientCount in ingredientCounts)
                {
                    if (ingredientCount.IsFixedIngredient)
                        rawIngredients.Add(new ThingDefCount(ingredientCount.FixedIngredient, (int)ingredientCount.GetBaseCount()));
                    else
                    {
                        var possibleIngredients = ingredientCount.filter.AllowedThingDefs.Select(a => new ThingDefCount(a, (int)ingredientCount.GetBaseCount()));
                        rawIngredients.AddRange(possibleIngredients);
                    }
                }
                var ingredientThings = rawIngredients.Select(c => c.ThingDef).ToHashSet();

                collections.forIngredientsAnalysis.AddRange(ingredientThings);
            }
        }
    }
}
