using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
using PeteTimesSix.ResearchReinvented.Extensions;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using PeteTimesSix.ResearchReinvented.Rimworld;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers.OpportunityFactories
{
    public class OpportunityFactoryCollectionsSetForRelation
    {
        internal HashSet<ThingDef> forProductionFacilityAnalysis = new HashSet<ThingDef>();
        internal HashSet<Def> forDirectAnalysis = new HashSet<Def>();
        internal HashSet<ThingDef> forIngredientsAnalysis = new HashSet<ThingDef>();
        internal HashSet<ThingDef> forHarvestProductAnalysis = new HashSet<ThingDef>();
        internal HashSet<ThingDef> forFuelAnalysis = new HashSet<ThingDef>();
        internal HashSet<Def> forPrototyping = new HashSet<Def>();
        internal HashSet<SpecialResearchOpportunityDef> specials = new HashSet<SpecialResearchOpportunityDef>();

        public ResearchRelation relation;

        internal OpportunityFactoryCollectionsSetForRelation alts;

        public OpportunityFactoryCollectionsSetForRelation(ResearchRelation relation) 
        {
            this.relation = relation;
        }

        internal void AddAlternates(bool isDirectRelation)
        {
            alts = new OpportunityFactoryCollectionsSetForRelation(this.relation);

            alts.forProductionFacilityAnalysis.AddRange(GetAlternates(forProductionFacilityAnalysis, isDirectRelation));
            alts.forDirectAnalysis.AddRange(GetAlternates(forDirectAnalysis.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet(), isDirectRelation, isAnalysis: true));
            alts.forIngredientsAnalysis.AddRange(GetAlternates(forIngredientsAnalysis, isDirectRelation));
            alts.forHarvestProductAnalysis.AddRange(GetAlternates(forHarvestProductAnalysis, isDirectRelation));
            alts.forFuelAnalysis.AddRange(GetAlternates(forFuelAnalysis, isDirectRelation));

        }
        public IEnumerable<ThingDef> GetAlternates(HashSet<ThingDef> originals, bool isDirectRelation, bool isAnalysis = false)
        {
            IEnumerable<AlternateResearchSubjectsDef> consideredDefs = DefDatabase<AlternateResearchSubjectsDef>.AllDefsListForReading;
            if (!isAnalysis)
                consideredDefs = consideredDefs.Where(d => !d.onlyEquivalentForAnalysis);
            if (!isDirectRelation)
                consideredDefs = consideredDefs.Where(d => !d.onlyEquivalentForDirectRelation);

            var alternates = new HashSet<ThingDef>();
            foreach (var def in originals)
            {
                var newAlternates = consideredDefs.Where(d => d.originals != null && d.originals.Contains(def));
                foreach (var alternateSet in newAlternates)
                {
                    if (alternateSet.alternates != null)
                        alternates.AddRange(alternateSet.alternates);
                }
            }
            return alternates.Except(originals);
        }

        internal void RemoveDuplicates(OpportunityFactoryCollectionsSetForRelation other)
        {
            forProductionFacilityAnalysis.ExceptWith(other.forProductionFacilityAnalysis);
            forDirectAnalysis.ExceptWith(other.forDirectAnalysis);
            forIngredientsAnalysis.ExceptWith(other.forIngredientsAnalysis);
            forHarvestProductAnalysis.ExceptWith(other.forHarvestProductAnalysis);
            forFuelAnalysis.ExceptWith(other.forFuelAnalysis);
            forPrototyping.ExceptWith(other.forPrototyping);

            specials.ExceptWith(other.specials);

            if(alts != null) //if we ARE alts, we cant do this
                alts.RemoveDuplicates(other.alts);
        }
    }

    public class OpportunityFactoryCollectionsSet
    {
        OpportunityFactoryCollectionsSetForRelation collections_direct = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Direct);
        OpportunityFactoryCollectionsSetForRelation collections_ancestor = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Ancestor);
        OpportunityFactoryCollectionsSetForRelation collections_descendant = new OpportunityFactoryCollectionsSetForRelation(ResearchRelation.Descendant);

        public OpportunityFactoryCollectionsSetForRelation GetSet(ResearchRelation relation) 
        {
            switch (relation)
            {
                case ResearchRelation.Direct:
                    return collections_direct;
                case ResearchRelation.Ancestor:
                    return collections_ancestor;
                case ResearchRelation.Descendant:
                    return collections_descendant;
            }
            return null;
        }

        public OpportunityFactoryCollectionsSetForRelation[] GetSets() 
        {
            return new OpportunityFactoryCollectionsSetForRelation[3] { collections_direct, collections_ancestor, collections_descendant };
        }

        public void AddAlternates()
        {
            collections_direct.AddAlternates(true);
            collections_ancestor.AddAlternates(false);
            collections_descendant.AddAlternates(false);
        }

        public void RemoveDuplicates()
        {
            collections_ancestor.RemoveDuplicates(collections_direct);
            collections_descendant.RemoveDuplicates(collections_direct);
        }
    }

    public class MasterFactory
    {
        //OpportunityFactoryCollectionsSet collections = new OpportunityFactoryCollectionsSet();

        public MasterFactory() 
        {
        }

        public IEnumerable<ResearchOpportunity> GenerateOpportunities(ResearchProjectDef project) 
        {
            if (project == null)
                return new List<ResearchOpportunity>();

            var collections = FillCollections(project);
            var opportunities = MakeOpportunities(project, collections);

            opportunities = ValidOpportunities(opportunities);

            return opportunities;
        }

        public IEnumerable<ResearchOpportunity> ValidOpportunities(IEnumerable<ResearchOpportunity> opportunities) 
        {
            foreach(var opportunity in opportunities) 
            {
                if(opportunity.IsValid()) 
                {
                    yield return opportunity;
                }
            }
            //yield break;
        }

        private IEnumerable<ResearchOpportunity> MakeOpportunities(ResearchProjectDef project, OpportunityFactoryCollectionsSet collections)
        {
            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.BasicResearch, ResearchRelation.Direct, new ROComp_RequiresNothing(), "Project");

            //var specials = new HashSet<SpecialResearchOpportunityDef>();
            //specials.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(o => o.originalProject == project));

            //var allThings = new HashSet<ThingDef>();

            //var directSet = collections.GetSet(ResearchRelation.Direct);

            foreach (var set in collections.GetSets())
            {
                foreach (var productionFacility in set.forProductionFacilityAnalysis.Where(facility => !project.UnlockedDefs.Contains(facility))) //dont include tables we're trying to invent
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseProductionFacility, set.relation, new ROComp_RequiresThing(productionFacility), "set.production facility");
                }
                foreach (var productionFacility in set.alts.forProductionFacilityAnalysis.Where(facility => !project.UnlockedDefs.Contains(facility))) //dont include tables we're trying to invent
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseProductionFacility, set.relation, new ROComp_RequiresThing(productionFacility), "alt set. production facility" , isAlternate: true);
                }

                foreach (var analysable in set.forDirectAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromDirectAnalysis(project, analysable, set.relation))
                        yield return opportunity;
                }
                foreach (var analysable in set.alts.forDirectAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromDirectAnalysis(project, analysable, set.relation, isAlternate: true))
                        yield return opportunity;
                }

                foreach (var material in set.forIngredientsAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromIngredientAnalysis(project, material, set.relation))
                        yield return opportunity;
                }
                foreach (var material in set.alts.forIngredientsAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromIngredientAnalysis(project, material, set.relation, isAlternate: true))
                        yield return opportunity;
                }

                foreach (var product in set.forHarvestProductAnalysis)
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseHarvestProduct, set.relation, new ROComp_RequiresThing(product), "set.harvest product");
                }
                foreach (var product in set.alts.forHarvestProductAnalysis)
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseHarvestProduct, set.relation, new ROComp_RequiresThing(product), "alt set.harvest product" , isAlternate: true);
                }

                foreach (var fuel in set.forFuelAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromFuelAnalysis(project, fuel, set.relation))
                        yield return opportunity;
                }
                foreach (var fuel in set.alts.forFuelAnalysis)
                {
                    foreach (var opportunity in OpportunitiesFromFuelAnalysis(project, fuel, set.relation, isAlternate: true))
                        yield return opportunity;
                }

                foreach (var prototypeable in set.forPrototyping)
                {
                    foreach (var opportunity in OpportunitiesFromPrototyping(project, prototypeable, set.relation))
                        yield return opportunity;
                }

                foreach (var special in set.specials)
                {
                    foreach (var opportunity in OpportunitiesFromSpecialOpportunityDef(project, special, set.relation))
                        yield return opportunity;
                }
            }

            //specials.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.originals != null && s.originals.Intersect(allThings).Any()));
            
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromSpecialOpportunityDef(ResearchProjectDef project, SpecialResearchOpportunityDef special, ResearchRelation relation)
        {
            var relationOverride = special.relationOverride.HasValue ? special.relationOverride.Value : relation;
            bool requiredSomething = false;
            if (special.alternates != null) 
            {
                requiredSomething = true;
                foreach (var alternate in special.alternates)
                {
                    if (special.opportunityType != null)
                    { 
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, relationOverride, new ROComp_RequiresThing(alternate), "special (thing)", special.importanceMultiplier, special.rareForThis, special.markAsAlternate);
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, alternate, relationOverride, isAlternate: special.markAsAlternate))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if (special.alternateTerrains != null)
            {
                requiredSomething = true;
                foreach (var alternate in special.alternateTerrains)
                {
                    if (special.opportunityType != null)
                    {
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, relationOverride, new ROComp_RequiresTerrain(alternate), "special (terrain)", special.importanceMultiplier, special.rareForThis, special.markAsAlternate);
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, alternate, relationOverride))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if(!requiredSomething)
                yield return new ResearchOpportunity(project, special.opportunityType, relation, new ROComp_RequiresNothing(), "special (nothing)", special.importanceMultiplier, special.markAsAlternate);
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromFuelAnalysis(ResearchProjectDef project, ThingDef fuel, ResearchRelation relation, bool isAlternate = false)
        {
            if (fuel.ingestible != null)
            {
                if(fuel.IsDrug)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelDrug, relation, new ROComp_RequiresThing(fuel), "fuel analysis (drug)", isAlternate: isAlternate);
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(fuel), "fuel analysis (ingestible)", isAlternate: isAlternate);
            }
            else
            {
                if (fuel.GetStatValueAbstract(StatDefOf.Flammability) >= 0.5f)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFlammable, relation, new ROComp_RequiresThing(fuel), "fuel analysis (flammable)", isAlternate: isAlternate);
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuel, relation, new ROComp_RequiresThing(fuel), "fuel analysis", isAlternate: isAlternate);
            }
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromIngredientAnalysis(ResearchProjectDef project, ThingDef material, ResearchRelation relation, bool isAlternate = false)
        {
            //food n' drugs
            if (material.ingestible != null)
            {
                //corpses
                if (material.IsCorpse)
                {
                    var deadThing = material.ingestible?.sourceDef;
                    if (deadThing != null)
                    {
                        var deadThingRace = deadThing.race;
                        if (deadThingRace != null)
                        {
                            if (deadThingRace.IsFlesh)
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissect, relation, new ROComp_RequiresThing(deadThing), "direct analysis (corpse)", isAlternate: isAlternate);
                            else
                                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDissectNonFlesh, relation, new ROComp_RequiresThing(deadThing), "direct analysis (corpse non-flesh)", isAlternate: isAlternate);

                        }
                    }
                }
                else if (material.IsDrug || material.IsMedicine)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredientsDrug, relation, new ROComp_RequiresThing(material), "ingre. analysis (drug)", isAlternate: isAlternate);
                else if (material.IsTrulyRawFood())
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(material), "ingre. analysis (raw food)", isAlternate: isAlternate);
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredientsFood, relation, new ROComp_RequiresThing(material), "ingre. analysis (ingestible)", isAlternate: isAlternate);
            }
            //everything else
            else
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredients, relation, new ROComp_RequiresThing(material), "ingre. analysis", isAlternate: isAlternate);
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromDirectAnalysis(ResearchProjectDef project, Def reverseEngineerable, ResearchRelation relation, bool isAlternate = false) 
        {
            if (reverseEngineerable is ThingDef asThing)
            {
                if (typeof(Plant).IsAssignableFrom(asThing.thingClass))
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePlant, relation, new ROComp_RequiresThing(asThing), "direct analysis (plant)", isAlternate: isAlternate);
                else if (!asThing.EverHaulable)
                {
                    if (!asThing.IsInstantBuild())
                    {
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, relation, new ROComp_RequiresThing(asThing), "direct analysis (unhaulable)", isAlternate: isAlternate);

                        //if (!isAlternate && relation == ResearchRelation.Direct && asThing.BuildableByPlayer)
                        //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeConstruction, relation, new ROComp_RequiresThing(asThing), "direct analysis (unhaulable)", isAlternate: isAlternate);
                    }
                }
                else
                {
                    //food n' drugs
                    if (asThing.ingestible != null)
                    { 
                        if(asThing.IsDrug)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDrug, relation, new ROComp_RequiresThing(asThing), "direct analysis (drug)", isAlternate: isAlternate);
                        else if(asThing.IsTrulyRawFood())
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseRawFood, relation, new ROComp_RequiresThing(asThing), "direct analysis (raw food)", isAlternate: isAlternate);
                        else
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFood, relation, new ROComp_RequiresThing(asThing), "direct analysis (ingestible)", isAlternate: isAlternate);
                    }
                    //everything else
                    else 
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, relation, new ROComp_RequiresThing(asThing), "direct analysis", isAlternate: isAlternate);

                    //if (!isAlternate && relation == ResearchRelation.Direct)
                    //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeProduction, relation, new ROComp_RequiresThing(asThing), "direct analysis", isAlternate: isAlternate);
                }

            }
            else if (reverseEngineerable is TerrainDef asTerrain)
            {
                if (asTerrain.IsSoil)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseSoil, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr. (soil)", isAlternate: isAlternate);
                else if (asTerrain.BuildableByPlayer)
                {
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFloor, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr. (builable)", isAlternate: isAlternate);
                    //if (!isAlternate && relation == ResearchRelation.Direct && asTerrain.BuildableByPlayer)
                    //    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeTerrainConstruction, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr. (builable)", isAlternate: isAlternate);
                }
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseTerrain, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr.", isAlternate: isAlternate);
            } 
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromPrototyping(ResearchProjectDef project, Def prototypeable, ResearchRelation relation, bool isAlternate = false)
        {
            if (prototypeable is ThingDef asThing)
            {
                if (typeof(Plant).IsAssignableFrom(asThing.thingClass))
                    yield break;
                else if (!asThing.EverHaulable)
                {
                    if (!asThing.IsInstantBuild())
                    {
                        if (!isAlternate && relation == ResearchRelation.Direct && asThing.BuildableByPlayer)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeConstruction, relation, new ROComp_RequiresThing(asThing), "direct analysis (unhaulable)", isAlternate: isAlternate);
                    }
                }
                else
                {
                    if (!isAlternate && relation == ResearchRelation.Direct)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeProduction, relation, new ROComp_RequiresThing(asThing), "direct analysis", isAlternate: isAlternate);
                }

            }
            else if (prototypeable is TerrainDef asTerrain)
            {
                if (asTerrain.BuildableByPlayer)
                {
                    if (!isAlternate && relation == ResearchRelation.Direct && asTerrain.BuildableByPlayer)
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.PrototypeTerrainConstruction, relation, new ROComp_RequiresTerrain(asTerrain), "direct analysis tr. (builable)", isAlternate: isAlternate);
                }
            }
        }

        public OpportunityFactoryCollectionsSet FillCollections(ResearchProjectDef project)
        {
            OpportunityFactoryCollectionsSet collections = new OpportunityFactoryCollectionsSet();

            OF_Recipes.MakeFromRecipes(project, collections.GetSet(ResearchRelation.Direct));
            OF_Unlocks.MakeFromUnlocks(project, collections.GetSet(ResearchRelation.Direct));
            OF_Plants.MakeFromPlants(project, collections.GetSet(ResearchRelation.Direct));
            OF_CompProps.MakeFromFuel(project, collections.GetSet(ResearchRelation.Direct));

            {
                var prerequisites = new List<ResearchProjectDef>();
                if (project.prerequisites != null)
                    prerequisites.AddRange(project.prerequisites);
                if (project.hiddenPrerequisites != null)
                    prerequisites.AddRange(project.hiddenPrerequisites);

                foreach (var prerequisite in prerequisites)
                {
                    if (prerequisite == project)
                        continue;

                    //OF_Recipes.MakeFromRecipes(prerequisite, ResearchRelation.Ancestor, collections.GetSet(ResearchRelation.Ancestor));
                    OF_Unlocks.MakeFromUnlocks(prerequisite, collections.GetSet(ResearchRelation.Ancestor));
                }
            }

            /*{
                var descendants = new List<ResearchProjectDef>();
                foreach(var candidate in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(p => p != project && (p.prerequisites?.Contains(project) ?? false) || p.hiddenPrerequisites?.Contains(project) ?? false)))

                foreach (var prerequisite in prerequisites)
                {
                    if (prerequisite == project)
                        continue;

                    //OF_Recipes.MakeFromRecipes(prerequisite, ResearchRelation.Ancestor, collections.GetSet(ResearchRelation.Ancestor));
                    OF_Unlocks.MakeFromUnlocks(prerequisite, collections.GetSet(ResearchRelation.Ancestor));
                }
            }*/

            /*discard what we dont want to generate from*/
            collections.GetSet(ResearchRelation.Ancestor).forIngredientsAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forIngredientsAnalysis.Clear();

            collections.GetSet(ResearchRelation.Ancestor).forFuelAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forFuelAnalysis.Clear();

            collections.GetSet(ResearchRelation.Ancestor).forPrototyping.Clear();
            collections.GetSet(ResearchRelation.Descendant).forPrototyping.Clear();

            /*add handcrafted opportunities*/
            collections.AddAlternates();
            OF_Specials.MakeFromSpecials(project, collections);

            collections.RemoveDuplicates();

            /*forProductionFacilityAnalysis.AddRange(GetAlternates(forProductionFacilityAnalysis));
            var temp1 = forDirectAnalysis.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet();
            forDirectAnalysis.AddRange(GetAlternates(temp1, true));
            forIngredientsAnalysis.AddRange(GetAlternates(forIngredientsAnalysis));
            forHarvestProductAnalysis.AddRange(GetAlternates(forHarvestProductAnalysis));
            var temp2 = forForwardEngineering.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet();
            forForwardEngineering.AddRange(GetAlternates(temp2));*/

            return collections;
        }

        
    }
}
