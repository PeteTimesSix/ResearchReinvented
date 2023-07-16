using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Managers
{
    public class FactionLectureManager: GameComponent
    {
        public static FactionLectureManager Instance => Current.Game.GetComponent<FactionLectureManager>();

        private Dictionary<Faction, int> _factionLectureCooldowns = new Dictionary<Faction, int>();

        public static int LectureCooldownTicks = 60000 / 4; //60000 ticks per day

        public FactionLectureManager(Game game) { }

        public bool IsOnCooldown(Faction faction) 
        {
            if (!_factionLectureCooldowns.ContainsKey(faction))
            {
                return false;
            }
            else
            {
                var isOnCooldown = (Find.TickManager.TicksAbs - _factionLectureCooldowns[faction]) < LectureCooldownTicks;
                return isOnCooldown;
            }
        }

        public void PutOnCooldown(Faction faction) 
        {
            _factionLectureCooldowns[faction] = Find.TickManager.TicksAbs;
        }
    }
}
