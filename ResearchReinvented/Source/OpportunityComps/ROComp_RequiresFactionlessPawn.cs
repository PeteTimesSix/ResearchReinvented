using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresFactionlessPawn : ResearchOpportunityComp
    {
        public override string ShortDesc => "outsider";
        public override TaggedString Subject => "outsider";
        public override bool TargetIsNull => true;

        public override bool IsRare => false;
        public override bool MetBy(Def def) => false;
        public override bool MetBy(Thing thing) => thing is Pawn pawn && (pawn.Faction == null || pawn.Faction.temporary);
        public override bool IsValid => true;

        public ROComp_RequiresFactionlessPawn()
        {
            //for deserialization
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
