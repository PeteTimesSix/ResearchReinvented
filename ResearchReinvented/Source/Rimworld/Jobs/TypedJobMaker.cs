using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace PeteTimesSix.ResearchReinvented.Rimworld.Jobs
{
	public static class TypedJobMaker<T> where T : Job, new()
	{
		// Token: 0x06000413 RID: 1043 RVA: 0x00015D1F File Offset: 0x00013F1F
		public static T MakeJob()
		{
			T job = SimplePool<T>.Get();
			job.loadID = Find.UniqueIDsManager.GetNextJobID();
			return job;
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x00015D36 File Offset: 0x00013F36
		public static T MakeJob(JobDef def)
		{
			T job = MakeJob();
			job.def = def;
			return job;
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00015D44 File Offset: 0x00013F44
		public static T MakeJob(JobDef def, LocalTargetInfo targetA)
		{
			T job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			return job;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x00015D59 File Offset: 0x00013F59
		public static T MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
		{
			T job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.targetB = targetB;
			return job;
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x00015D75 File Offset: 0x00013F75
		public static T MakeJob(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
		{
			T job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.targetB = targetB;
			job.targetC = targetC;
			return job;
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x00015D98 File Offset: 0x00013F98
		public static T MakeJob(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			T job = MakeJob();
			job.def = def;
			job.targetA = targetA;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = checkOverrideOnExpiry;
			return job;
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x00015DBB File Offset: 0x00013FBB
		public static T MakeJob(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			T job = MakeJob();
			job.def = def;
			job.expiryInterval = expiryInterval;
			job.checkOverrideOnExpire = checkOverrideOnExpiry;
			return job;
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x00015DD7 File Offset: 0x00013FD7
		public static void ReturnToPool(T job)
		{
			if (job == null)
			{
				return;
			}
			if (SimplePool<Job>.FreeItemsCount >= MaxJobPoolSize)
			{
				return;
			}
			job.Clear();
			SimplePool<Job>.Return(job);
		}

		private const int MaxJobPoolSize = 1000;
	}
}
