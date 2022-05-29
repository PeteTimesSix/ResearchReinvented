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
    public static class OF_Plants
    {
        public static void MakeFromPlants(ResearchProjectDef project, ResearchRelation relation, OpportunityFactoryCollectionsSetForRelation collections)
        {
            if (project.UnlockedDefs != null)
            {
                foreach (var unlock in project.UnlockedDefs.Where(u => u is ThingDef asThing && typeof(Plant).IsAssignableFrom(asThing.thingClass)))
                {
                    var plant = (unlock as ThingDef);
                    var product = plant.plant?.harvestedThingDef;
                    if (product != null)
                    {
                        collections.forHarvestProductAnalysis.Add(product);
                    }
                }
            }
        }
    }
}
