using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Defs;
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

        public ResearchRelation relation;

        public OpportunityFactoryCollectionsSetForRelation(ResearchRelation relation) 
        {
            this.relation = relation;
        }

        internal void AddAlternates(bool isDirectRelation)
        {
            forProductionFacilityAnalysis.AddRange(GetAlternates(forProductionFacilityAnalysis, isDirectRelation));
            forDirectAnalysis.AddRange(GetAlternates(forDirectAnalysis.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet(), isDirectRelation, isAnalysis: true));
            forIngredientsAnalysis.AddRange(GetAlternates(forIngredientsAnalysis, isDirectRelation));
            forHarvestProductAnalysis.AddRange(GetAlternates(forHarvestProductAnalysis, isDirectRelation));
            forFuelAnalysis.AddRange(GetAlternates(forFuelAnalysis, isDirectRelation));
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
                        alternates.AddRange(alternateSet.alternates);//.Where(d => d is ThingDef).Cast<ThingDef>());
                }
            }
            return alternates;
        }

        internal void RemoveDuplicates(OpportunityFactoryCollectionsSetForRelation other)
        {
            forProductionFacilityAnalysis.ExceptWith(other.forProductionFacilityAnalysis);
            forDirectAnalysis.ExceptWith(other.forDirectAnalysis);
            forIngredientsAnalysis.ExceptWith(other.forIngredientsAnalysis);
            forHarvestProductAnalysis.ExceptWith(other.forHarvestProductAnalysis);
            forFuelAnalysis.ExceptWith(other.forFuelAnalysis);
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
        OpportunityFactoryCollectionsSet collections = new OpportunityFactoryCollectionsSet();

        internal HashSet<Def> forForwardEngineering = new HashSet<Def>();


        public MasterFactory() 
        {
        }

        public IEnumerable<ResearchOpportunity> GenerateOpportunities(ResearchProjectDef project) 
        {
            FillCollections(project);

            return MakeOpportunities(project);
        }

        private IEnumerable<ResearchOpportunity> MakeOpportunities(ResearchProjectDef project)
        {
            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.BasicResearch, ResearchRelation.Direct, new ROComp_RequiresNothing());

            var specials = new HashSet<SpecialResearchOpportunityDef>();
            specials.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(o => o.originalProject == project));

            var allThings = new HashSet<ThingDef>();

            //var directSet = collections.GetSet(ResearchRelation.Direct);

            foreach (var set in collections.GetSets()) 
            {
                foreach (var productionFacility in set.forProductionFacilityAnalysis.Where(facility => !project.UnlockedDefs.Contains(facility))) //dont include tables we're trying to invent
                {
                    allThings.Add(productionFacility);
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseProductionFacility, set.relation, new ROComp_RequiresThing(productionFacility));
                }

                foreach (var analysable in set.forDirectAnalysis)
                {
                    if(analysable is ThingDef asThing)
                        allThings.Add(asThing);
                    foreach (var opportunity in OpportunitiesFromDirectAnalysis(project, analysable, set.relation))
                        yield return opportunity;
                }

                foreach (var material in set.forIngredientsAnalysis)
                {
                    allThings.Add(material);
                    foreach (var opportunity in OpportunitiesFromIngredientAnalysis(project, material, set.relation))
                        yield return opportunity;
                }

                foreach (var product in set.forHarvestProductAnalysis)
                {
                    allThings.Add(product);
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseHarvestProduct, set.relation, new ROComp_RequiresThing(product));
                }

                foreach (var fuel in set.forFuelAnalysis)
                {
                    allThings.Add(fuel);
                    foreach (var opportunity in OpportunitiesFromFuelAnalysis(project, fuel, set.relation))
                        yield return opportunity;
                }
                
            }

            specials.AddRange(DefDatabase<SpecialResearchOpportunityDef>.AllDefsListForReading.Where(s => s.originals != null && s.originals.Intersect(allThings).Any()));
            foreach (var special in specials)
            {
                foreach (var opportunity in OpportunitiesFromSpecialOpportunityDef(project, special))
                    yield return opportunity;
            }
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromSpecialOpportunityDef(ResearchProjectDef project, SpecialResearchOpportunityDef special)
        {
            if (special.alternates != null) 
            {
                foreach (var alternate in special.alternates)
                {
                    if (special.opportunityType != null)
                    { 
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, special.relation, new ROComp_RequiresThing(alternate), special.importanceMultiplier);
                        if (special.rareForThis)
                            opportunity.rare = true;
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, alternate, special.relation))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }

            if (special.alternateTerrains != null)
            {
                foreach (var alternate in special.alternateTerrains)
                {
                    if (special.opportunityType != null)
                    {
                        var opportunity = new ResearchOpportunity(project, special.opportunityType, special.relation, new ROComp_RequiresTerrain(alternate), special.importanceMultiplier);
                        if (special.rareForThis)
                            opportunity.rare = true;
                        yield return opportunity;
                    }
                    else
                    {
                        foreach (var standardOpportunity in OpportunitiesFromDirectAnalysis(project, alternate, special.relation))
                        {
                            standardOpportunity.importance = special.importanceMultiplier;
                            yield return standardOpportunity;
                        }
                    }
                }
            }
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromFuelAnalysis(ResearchProjectDef project, ThingDef fuel, ResearchRelation relation)
        {
            if (fuel.ingestible != null)
            {
                if(fuel.IsDrug)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelDrug, relation, new ROComp_RequiresThing(fuel));
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(fuel));
            }
            else
            {
                if (fuel.GetStatValueAbstract(StatDefOf.Flammability) >= 0.5f)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFlammable, relation, new ROComp_RequiresThing(fuel));
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuel, relation, new ROComp_RequiresThing(fuel));
            }
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromIngredientAnalysis(ResearchProjectDef project, ThingDef material, ResearchRelation relation)
        {
            //food n' drugs
            if (material.ingestible != null)
            {
                if (material.IsDrug)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredientsDrug, relation, new ROComp_RequiresThing(material));
                else if (material.IsRawFood())
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFuelFood, relation, new ROComp_RequiresThing(material));
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredientsFood, relation, new ROComp_RequiresThing(material));
            }
            //everything else
            else
                yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseIngredients, relation, new ROComp_RequiresThing(material));
        }

        public IEnumerable<ResearchOpportunity> OpportunitiesFromDirectAnalysis(ResearchProjectDef project, Def reverseEngineerable, ResearchRelation relation) 
        {
            if (reverseEngineerable is ThingDef asThing)
            {
                if (typeof(Plant).IsAssignableFrom(asThing.thingClass))
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalysePlant, relation, new ROComp_RequiresThing(asThing));
                else if (!asThing.EverHaulable)
                {
                    if (!asThing.IsInstantBuild())
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseInPlace, relation, new ROComp_RequiresThing(asThing));
                }
                else
                {
                    //food n' drugs
                    if (asThing.ingestible != null)
                    { 
                        if(asThing.IsDrug)
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseDrug, relation, new ROComp_RequiresThing(asThing));
                        else if(asThing.IsRawFood())
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseRawFood, relation, new ROComp_RequiresThing(asThing));
                        else
                            yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFood, relation, new ROComp_RequiresThing(asThing));
                    }
                    //everything else
                    else 
                        yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.Analyse, relation, new ROComp_RequiresThing(asThing));
                }
            }
            else if (reverseEngineerable is TerrainDef asTerrain)
            {
                if (asTerrain.IsSoil)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseSoil, relation, new ROComp_RequiresTerrain(asTerrain));
                else if (asTerrain.BuildableByPlayer)
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseFloor, relation, new ROComp_RequiresTerrain(asTerrain));
                else
                    yield return new ResearchOpportunity(project, ResearchOpportunityTypeDefOf.AnalyseTerrain, relation, new ROComp_RequiresTerrain(asTerrain));
            } 
        }

        public void FillCollections(ResearchProjectDef project)
        {
            OF_Recipes.MakeFromRecipes(project, ResearchRelation.Direct, collections.GetSet(ResearchRelation.Direct));
            OF_Unlocks.MakeFromUnlocks(project, ResearchRelation.Direct, collections.GetSet(ResearchRelation.Direct));
            OF_Plants.MakeFromPlants(project, ResearchRelation.Direct, collections.GetSet(ResearchRelation.Direct));
            OF_CompProps.MakeFromFuel(project, ResearchRelation.Direct, collections.GetSet(ResearchRelation.Direct));

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
                    OF_Unlocks.MakeFromUnlocks(prerequisite, ResearchRelation.Ancestor, collections.GetSet(ResearchRelation.Ancestor));
                }
            }

            /*discard what we dont want to generate from*/
            collections.GetSet(ResearchRelation.Ancestor).forIngredientsAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forIngredientsAnalysis.Clear();

            collections.GetSet(ResearchRelation.Ancestor).forFuelAnalysis.Clear();
            collections.GetSet(ResearchRelation.Descendant).forFuelAnalysis.Clear();

            collections.AddAlternates();
            collections.RemoveDuplicates();

            /*forProductionFacilityAnalysis.AddRange(GetAlternates(forProductionFacilityAnalysis));
            var temp1 = forDirectAnalysis.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet();
            forDirectAnalysis.AddRange(GetAlternates(temp1, true));
            forIngredientsAnalysis.AddRange(GetAlternates(forIngredientsAnalysis));
            forHarvestProductAnalysis.AddRange(GetAlternates(forHarvestProductAnalysis));
            var temp2 = forForwardEngineering.Where(d => d is ThingDef).Cast<ThingDef>().ToHashSet();
            forForwardEngineering.AddRange(GetAlternates(temp2));*/
        }

        
    }
}
