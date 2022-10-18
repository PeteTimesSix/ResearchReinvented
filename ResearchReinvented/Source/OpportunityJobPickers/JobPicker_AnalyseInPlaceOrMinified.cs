using PeteTimesSix.ResearchReinvented.DefOfs;
using PeteTimesSix.ResearchReinvented.Opportunities;
using PeteTimesSix.ResearchReinvented.OpportunityComps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityJobPickers
{
    public class JobPicker_AnalyseInPlaceOrMinified : OpportunityJobPickerBase
    {
        private static List<JobDef> InPlaceOnly = new List<JobDef>() { JobDefOf_Custom.RR_AnalyseInPlace };
        private static List<JobDef> InPlaceOrMinifiedAtBench = new List<JobDef>() { JobDefOf_Custom.RR_AnalyseInPlace, JobDefOf_Custom.RR_Analyse };
        private static List<JobDef> AtBenchOnly = new List<JobDef>() { JobDefOf_Custom.RR_Analyse };

        public override List<JobDef> PickJobs(ResearchOpportunity opportunity)
        {
            if(!(opportunity.requirement is ROComp_RequiresThing thingRequirement))
            {
                Log.Error($"opportunity {opportunity} def {opportunity.def.defName} had JobPicker_AnalyseInPlaceOrMinified but does not require a thing!");
                return new List<JobDef>();
            }
            else
            {
                var def = thingRequirement.thingDef;
                if(def.EverHaulable)
                {
                    return AtBenchOnly;
                }
                else
                {
                    if (def.Minifiable) 
                    {
                        return InPlaceOrMinifiedAtBench;
                    }
                    else
                    {
                        return InPlaceOnly;
                    }
                }
            }
        }
    }
}
