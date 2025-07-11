using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public enum AlternatesMode
    {
        NONE,
        EQUIVALENT,
        SIMILAR
    }
    public abstract class ResearchOpportunityComp: IExposable
    {
        public abstract string ShortDesc { get; }
        public abstract TaggedString Subject { get; }
        public abstract bool TargetIsNull { get; }
        public abstract bool IsRare { get; }
        public abstract bool IsFreebie { get; }
        public abstract bool IsValid { get; }

        public abstract bool MetBy(Def def);
        public abstract bool MetBy(Thing thing);
        public virtual int AlternateCount => 0;
        public virtual void ListAlts() { }

        public virtual void ExposeData()
        {
        }
    }
}
