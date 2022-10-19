using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public class OF_Recipes
    {
        public static void MakeFromRecipes(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            HashSet<ThingDef> users = new HashSet<ThingDef>();
            HashSet<IngredientCount> ingredients = new HashSet<IngredientCount>();
            HashSet<ThingDef> products = new HashSet<ThingDef>();
            HashSet<ThingDef> prototypeables = new HashSet<ThingDef>();

            foreach (var recipe in GatherDirectRecipes(project).Where(r => r.PassesIdeoCheck())) 
            {
                users.AddRange(recipe.AllRecipeUsers);
                if (recipe.products != null)
                    products.AddRange(recipe.products.Select(p => p.thingDef));
                if (recipe.ingredients != null)
                    ingredients.AddRange(recipe.ingredients);

                if (!recipe.AllResearchPrerequisites().Except(project).Any())
                    prototypeables.AddRange(recipe.products.Select(p => p.thingDef));
            }

            foreach (var recipe in GatherCreationRecipes(project).Where(r => r.PassesIdeoCheck()))
            {
                users.AddRange(recipe.AllRecipeUsers);
            }

            foreach (var recipe in GatherNewImpliedRecipes(project).Where(r => r.products != null).Where(r => r.PassesIdeoCheck()))
            {
                if (recipe.products != null)
                    products.AddRange(recipe.products.Select(p => p.thingDef));
                if (recipe.ingredients != null)
                    ingredients.AddRange(recipe.ingredients);

                if (!recipe.AllResearchPrerequisites().Except(project).Any())
                    prototypeables.AddRange(recipe.products.Select(p => p.thingDef));
            }

            var rawIngredients = new List<ThingDefCount>();
            foreach (var ingredientCount in ingredients)
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

            users = FilterProducersToPlausiblyUnlocked(project, users);

            collections.forProductionFacilityAnalysis.AddRange(users);
            collections.forDirectAnalysis.AddRange(products);
            collections.forIngredientsAnalysis.AddRange(ingredientThings);

            collections.forPrototyping.AddRange(prototypeables);
        }

        private static HashSet<ThingDef> FilterProducersToPlausiblyUnlocked(ResearchProjectDef project, HashSet<ThingDef> users)
        {
            var resultSet = new HashSet<ThingDef>();
            foreach (var thingDef in users) 
            {
                if(thingDef.researchPrerequisites != null)
                {
                    if (project.RequiredToUnlock(thingDef.researchPrerequisites)) 
                    {
                        //Log.Message($"skipping {thingDef} as a production facility, {project} is required to ever build it");
                        continue;
                    }
                }

                resultSet.Add(thingDef);
            }
            return resultSet;
        }

        private static HashSet<RecipeDef> GatherDirectRecipes(ResearchProjectDef project)
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            recipes.AddRange(
                DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(r => r.researchPrerequisite == project || (r.researchPrerequisites != null && r.researchPrerequisites.Contains(project)))
                );

            if (project.UnlockedDefs != null)
            {
                recipes.AddRange(
                    project.UnlockedDefs.Where(u => u is RecipeDef).Cast<RecipeDef>()
                    );
            }

            return recipes;
        }
        private static HashSet<RecipeDef> GatherNewImpliedRecipes(ResearchProjectDef project)
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            if (project.UnlockedDefs != null)
            {
                var unlockedThings = project.UnlockedDefs.Where(u => u is ThingDef).Cast<ThingDef>().ToList();
                foreach (var thing in unlockedThings)
                {
                    if (thing.AllRecipes != null)
                    {
                        recipes.AddRange(thing.AllRecipes.Where(r => r.IsAvailableOnlyForPrototyping(true)));
                    }
                }
            }

            return recipes;
        }

        private static HashSet<RecipeDef> GatherCreationRecipes(ResearchProjectDef project) 
        {
            HashSet<RecipeDef> recipes = new HashSet<RecipeDef>();

            if (project.UnlockedDefs != null)
            {
                foreach (var unlock in project.UnlockedDefs)
                {
                    if(unlock is ThingDef asThing)
                    {
                        recipes.AddRange(
                            DefDatabase<RecipeDef>.AllDefsListForReading
                                .Where(r => r.products.Any(tdc => tdc.thingDef == asThing))
                        );
                    }
                }
            }

            return recipes;
        }

    }
}
