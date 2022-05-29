using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    class ROComp_RequiresNothing : ResearchOpportunityComp
    {
        public override string ShortDesc
        {
            get
            {
                return "Generic research";
            }
        }

        public override bool TargetIsNull => false;

        public override bool IsRare => false;

        public ROComp_RequiresNothing()
        {
        }
    }
}
