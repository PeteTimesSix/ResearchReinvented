using PeteTimesSix.ResearchReinvented.Managers;
using PeteTimesSix.ResearchReinvented.Rimworld;
using PeteTimesSix.ResearchReinvented.Rimworld.DefModExtensions;
using PeteTimesSix.ResearchReinvented.Rimworld.JobDrivers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.OpportunityComps
{
    public class ROComp_RequiresThing : ResearchOpportunityComp
    {
        public AlternatesMode altsMode;
        private bool alternatesCached = false;
        private ThingDef primaryThingDef;
        private ThingDef[] alternates;
        public ThingDef[] Alternates
        {
            get
            {
                if (!alternatesCached)
                    CacheAlternates();
                return alternates; 
            }
        }
        public override int AlternateCount
        {
            get
            {
                if (!alternatesCached)
                    CacheAlternates();
                return alternates?.Length ?? 0;
            }
        }
        private ThingDef[] allThings;
        public ThingDef[] AllThings
        {
            get
            {
                if(allThings == null)
                {
                    var tempList = new List<ThingDef>() { primaryThingDef };
                    if (Alternates != null)
                        tempList.AddRange(Alternates);
                    allThings = tempList.ToArray();
                }
                return allThings;
            }
        }

        public ThingDef ShownCycledThing
        {
            get
            {
                var thingDef = primaryThingDef;
                if (AlternateCount > 0 && Time.realtimeSinceStartupAsDouble % 2 > 1.0)
                {
                    thingDef = Alternates[(int)((Time.realtimeSinceStartupAsDouble / 2.0) % AlternateCount)];
                }
                return thingDef;
            }
        }
        public ThingDef PrimaryThingDef => primaryThingDef;
        public override string ShortDesc => String.Concat(ShownCycledThing?.label);
        public override TaggedString Subject => new TaggedString(ShownCycledThing?.label).Colorize(Color.cyan);
        public override bool TargetIsNull => primaryThingDef is null;

        public override bool IsRare => primaryThingDef.HasModExtension<RarityMarker>();
        public override bool IsFreebie => primaryThingDef.HasModExtension<FreebieMarker>();
        public override bool MetBy(Def def)
        {
            if (def is not ThingDef)
                return false;
            if (def == primaryThingDef)
                return true;
            if (!alternatesCached)
                CacheAlternates();
            if (alternates != null && alternates.Contains(def))
                return true;
            return false;
        }

        public override bool MetBy(Thing thing)
        {
            var def = thing.def;
            if ((thing is MinifiedThing minified))
                def = minified.InnerThing.def;
            return MetBy(def);
        }
        public override void ListAlts()
        {
            Log.Message($"alts for {primaryThingDef.defName}: {string.Join(",", alternates.Select(a => a.defName))}");
        }

        public override bool IsValid => primaryThingDef != null;

        public ROComp_RequiresThing() 
        {
            //for deserialization
        }

        public ROComp_RequiresThing(ThingDef targetDef, AlternatesMode altsMode)
        {
            this.primaryThingDef = targetDef;
            this.altsMode = altsMode;
            CacheAlternates();
        }


        public void CacheAlternates()
        {
            switch(altsMode)
            {
                case AlternatesMode.NONE:
                    alternates = null;
                    break;
                case AlternatesMode.EQUIVALENT:
                    {
                        if (AlternatesKeeper.alternatesEquivalent.TryGetValue(primaryThingDef, out ThingDef[] alts))
                            alternates = alts;
                    }
                    break;
                case AlternatesMode.SIMILAR:
                    {
                        if (AlternatesKeeper.alternatesSimilar.TryGetValue(primaryThingDef, out ThingDef[] alts))
                            alternates = alts;
                    }
                    break;
            }
            alternatesCached = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref primaryThingDef, "targetDef");
            Scribe_Values.Look(ref altsMode, "altsMode");
        }
    }
}
