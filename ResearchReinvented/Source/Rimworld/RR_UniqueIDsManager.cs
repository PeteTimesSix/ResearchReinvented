using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Rimworld
{
	public class RR_UniqueIDsManager : GameComponent, IExposable
	{
		public static RR_UniqueIDsManager instance => Current.Game.GetComponent<RR_UniqueIDsManager>();

		// Token: 0x060060ED RID: 24813 RVA: 0x0021303C File Offset: 0x0021123C
		public int GetNextResearchOpportunityID()
		{
			return RR_UniqueIDsManager.GetNextID(ref this.nextOpportunityID);
		}

		// Token: 0x060060EE RID: 24814 RVA: 0x00213049 File Offset: 0x00211249
		public RR_UniqueIDsManager(Game game)
		{
		}

		// Token: 0x060060EF RID: 24815 RVA: 0x00213064 File Offset: 0x00211264
		private static int GetNextID(ref int nextID)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars && !instance.wasLoaded)
			{
				Log.Warning("Getting next unique ID during LoadingVars before UniqueIDsManager was loaded. Assigning a random value.");
				return Rand.Int;
			}
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Log.Warning("Getting next unique ID during saving This may cause bugs.");
			}
			int result = nextID;
			nextID++;
			if (nextID == 2147483647)
			{
				Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
				nextID = 0;
			}
			return result;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.nextOpportunityID, "nextOpportunityID", 0, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.wasLoaded = true;
			}
		}

		private int nextOpportunityID;

		private bool wasLoaded;
	}
}
