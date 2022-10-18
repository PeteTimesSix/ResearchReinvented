using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityJobPickers
{
    public class JobPicker_FromOpportunityDef : OpportunityJobPickerBase
    {
        public override List<JobDef> PickJobs(ResearchOpportunity opportunity)
        {
            return new List<JobDef>() { opportunity.def.jobDef };
        }
    }
}
