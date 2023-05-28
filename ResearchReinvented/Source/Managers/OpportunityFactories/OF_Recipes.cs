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
            HashSet<ThingDef> ingredients = new HashSet<ThingDef>();
            HashSet<ThingDef> products = new HashSet<ThingDef>();
            HashSet<Def> prototypeables = new HashSet<Def>();

            foreach (var recipe in GatherDirectRecipes(project).Where(r => r.PassesIdeoCheck()))
            {
                users.AddRange(recipe.AllRecipeUsers);
                if (recipe.products != null)
                    products.AddRange(recipe.products.Select(p => p.thingDef)); 
                if (recipe.ingredients != null)
                {
                    FilterRecipeIngredients(ingredients, recipe);
                }

                if (!recipe.AllResearchPrerequisites().Except(project).Any())
                    prototypeables.Add(recipe);
                    //prototypeables.AddRange(recipe.products.Select(p => p.thingDef));
            }

            foreach (var recipe in GatherCreationRecipes(project).Where(r => r.PassesIdeoCheck()))
            {
                users.AddRange(recipe.AllRecipeUsers);
            }

            users = FilterProducersToPlausiblyUnlocked(project, users);

            collections.forProductionFacilityAnalysis.AddRange(users);
            collections.forDirectAnalysis.AddRange(products);
            collections.forIngredientsAnalysis.AddRange(ingredients);

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

        private static void FilterRecipeIngredients(HashSet<ThingDef> ingredientsStore, RecipeDef recipe)
        {
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = recipe.ingredients[i];
                if (ingredientCount.IsFixedIngredient)
                {
                    ingredientsStore.Add(ingredientCount.FixedIngredient);
                }
                else
                {
                    if (recipe.fixedIngredientFilter != null)
                    {
                        var allowedThings = ingredientCount.filter.AllowedThingDefs.Where(t => recipe.fixedIngredientFilter.Allows(t));
                        ingredientsStore.AddRange(allowedThings);
                    }
                    else
                    {
                        ingredientsStore.AddRange(ingredientCount.filter.AllowedThingDefs);
                    }
                }
            }
        }

    }
}
