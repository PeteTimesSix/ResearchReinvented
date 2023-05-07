using PeteTimesSix.ResearchReinvented.Rimworld.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class SmallExtensions
    {
        public static IEnumerable<EnumType> GetFlags<EnumType>(this EnumType e) where EnumType : System.Enum
        {
            return Enum.GetValues(typeof(EnumType))
                .Cast<EnumType>()
                .Where(v => e.HasFlag(v));
        }

        public static Rect OffsetBy(this Rect rect, float x, float y)
        {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }

        public static bool CanUseNow(this Building_ResearchBench bench) 
        {
            var powerComp = bench.GetComp<CompPowerTrader>();
            var forbidComp = bench.GetComp<CompForbiddable>();

            return bench.Spawned && (powerComp == null || powerComp.PowerOn) && (forbidComp == null || !forbidComp.Forbidden) && bench.Faction == Faction.OfPlayer;
		}
	}
}
