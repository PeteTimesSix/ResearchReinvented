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
    public class ROComp_RequiresPawnOfFaction : ResearchOpportunityComp
    {
        public Faction faction;

        public override string ShortDesc => faction.Name;
        public override TaggedString Subject => faction.NameColored;
        public override bool TargetIsNull => faction is null;

        public override bool IsRare => false;
        public override bool MetBy(Def def) => faction.def == def;
        public override bool MetBy(Thing thing) => thing is Pawn pawn && pawn.Faction == faction;
        public override bool IsValid => faction != null;

        public ROComp_RequiresPawnOfFaction()
        {
            //for deserialization
        }

        public ROComp_RequiresPawnOfFaction(Faction faction)
        {
            this.faction = faction;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
        }
    }
}
