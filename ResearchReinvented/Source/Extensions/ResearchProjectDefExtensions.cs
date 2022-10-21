using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.ResearchReinvented.Extensions
{
    public static class ResearchProjectDefExtensions
	{
		public static bool HasAnyPrerequisites(this ResearchProjectDef project)
		{
			return 
                project.requiredResearchBuilding != null || 
                !project.requiredResearchFacilities.NullOrEmpty() || 
                !(ResearchReinventedMod.Settings.kitlessResearch || (ResearchReinventedMod.Settings.kitlessNeolithicResearch && project.techLevel <= TechLevel.Neolithic));
		}

		public static bool RequiredToUnlock(this ResearchProjectDef project, IEnumerable<ResearchProjectDef> prerequisites) 
		{
			var checkedSet = new HashSet<ResearchProjectDef>();
            var queue = new Queue<ResearchProjectDef>();

            foreach(var prerequisite in prerequisites)
                queue.Enqueue(prerequisite);

            while (queue.Count > 0)
            {
                var checkedProject = queue.Dequeue();
                if(checkedProject == project)
                    return true;
                if (checkedSet.Contains(checkedProject))
                    continue;
                else
                    checkedSet.Add(checkedProject);
                if (checkedProject.prerequisites != null)
                {
                    foreach (var prerequisite in checkedProject.prerequisites)
                    {
                        queue.Enqueue(prerequisite);
                    }
                }
                if (checkedProject.hiddenPrerequisites != null)
                {
                    foreach (var prerequisite in checkedProject.hiddenPrerequisites)
                    {
                        queue.Enqueue(prerequisite);
                    }
                }
            }
            return false;
        }
    }
}
