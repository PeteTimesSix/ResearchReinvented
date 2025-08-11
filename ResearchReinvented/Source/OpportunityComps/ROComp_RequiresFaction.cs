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
    public class ROComp_RequiresFaction : ResearchOpportunityComp
    {
        public Faction faction;

        public override string ShortDesc => faction.Name;
        public override TaggedString Subject => faction.NameColored;
        public override bool TargetIsNull => faction is null;

        public override bool IsRare => false;
        public override bool IsFreebie => false;
        public override bool MetBy(Def def) => faction.def == def;
        public override bool MetBy(Thing thing) => thing is Pawn pawn && MetByFaction(pawn.GetExtraHomeFaction() ?? pawn.Faction);
        public bool MetByFaction(Faction faction) => faction == this.faction;
        public override bool IsValid => faction != null;

        public ROComp_RequiresFaction()
        {
            //for deserialization
        }

        public ROComp_RequiresFaction(Faction faction)
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
