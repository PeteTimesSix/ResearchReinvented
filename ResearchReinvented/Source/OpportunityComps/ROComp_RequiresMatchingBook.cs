using HarmonyLib;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresSchematicWithProject : ResearchOpportunityComp
    {
        public ResearchProjectDef projectDef;

        public override string ShortDesc => String.Concat(projectDef?.label);
        public override TaggedString Subject => new TaggedString(projectDef?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => projectDef is null;

        public override bool IsRare => false;
        public override bool MetBy(Def def) => projectDef == def;
        public override bool MetBy(Thing thing) => throw new InvalidOperationException("This should never be called!");
        public override bool IsValid => projectDef != null;

        public ROComp_RequiresSchematicWithProject() 
        {
            //for deserialization
        }

        public ROComp_RequiresSchematicWithProject(ResearchProjectDef targetDef)
        {
            this.projectDef = targetDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref projectDef, "projectDef");
        }
    }
}
