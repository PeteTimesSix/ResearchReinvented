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
                HashSet<JobDef> jobs = new HashSet<JobDef>();
                foreach(var thingDef in thingRequirement.AllThings)
                {
                    if (thingDef.EverHaulable)
                    {
                        jobs.AddRange(AtBenchOnly);
                        //return AtBenchOnly;
                    }
                    else
                    {
                        if (thingDef.Minifiable)
                        {
                            jobs.AddRange(InPlaceOrMinifiedAtBench);
                            //return InPlaceOrMinifiedAtBench;
                        }
                        else
                        {
                            jobs.AddRange(InPlaceOnly);
                            //return InPlaceOnly;
                        }
                    }
                }
                /*if (jobs.Count == 0)
                {
                    Log.ErrorOnce("RR: Got no job set for AnalyseInPlaceOrMinified for thing: " + thingRequirement.PrimaryThingDef, 524562 + thingRequirement.PrimaryThingDef.shortHash);
                    return InPlaceOnly;
                }
                if (jobs.Count > 1)
                {
                    Log.WarningOnce("RR: Got mismatched job sets for AnalyseInPlaceOrMinified for thing: " + thingRequirement.PrimaryThingDef, 121202 + thingRequirement.PrimaryThingDef.shortHash);
                    return InPlaceOnly;
                }*/
                return jobs.ToList();
            }
        }
    }
}
