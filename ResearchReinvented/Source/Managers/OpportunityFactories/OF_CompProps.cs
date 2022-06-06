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
    public static class OF_CompProps
    {
        public static void MakeFromFuel(ResearchProjectDef project, OpportunityFactoryCollectionsSetForRelation collections)
        {
            if (project.UnlockedDefs != null)
            {
                foreach (var unlock in project.UnlockedDefs.Where(u => u is ThingDef asThing).Cast<ThingDef>())
                {
                    {
                        var fuelComp = unlock.GetCompProperties<CompProperties_Refuelable>();
                        if (fuelComp != null)
                        {
                            collections.forFuelAnalysis.AddRange(fuelComp.fuelFilter.AllowedThingDefs);
                        }
                    }
                }
            }
        }
    }
}
