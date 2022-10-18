using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityJobPickers
{
    public class JobPicker_NoJob : OpportunityJobPickerBase
    {
        private static List<JobDef> emptyList = new List<JobDef>();

        public override List<JobDef> PickJobs(ResearchOpportunity opportunity)
        {
            return emptyList;
        }
    }
}
