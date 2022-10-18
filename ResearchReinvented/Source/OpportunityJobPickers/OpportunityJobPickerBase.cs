using PeteTimesSix.ResearchReinvented.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityJobPickers
{
    public abstract class OpportunityJobPickerBase
    {
        public abstract List<JobDef> PickJobs(ResearchOpportunity opportunity);
    }
}
