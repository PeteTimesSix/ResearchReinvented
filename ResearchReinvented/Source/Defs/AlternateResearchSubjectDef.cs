using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Defs
{
    public class AlternateResearchSubjectsDef : Def
    {
        public List<ThingDef> originals;
        public List<ThingDef> alternates;
        public bool onlyEquivalentForAnalysis = true;
        public bool onlyEquivalentForDirectRelation = true;
        public bool markAsRare = false;
    }
}
