using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class RecastHelper
    {
        
		public static Vector3 GetCanChongJiPath(this MapComponent self, Unit unit, Vector3 start, Vector3 target)
		{
           
            using var list = ListComponent<Vector3>.Create();
            Vector3 dir = (target - start).normalized;
            Vector3 tmm = start;
            while (true)
            {
                Vector3 next = tmm + (1f * dir);

                //Game.Scene.GetComponent<RecastPathComponent>().SearchPath(self.NavMeshId, start, next, list, 2);
                self.SearchPath(unit, next, list);

                if (list.Count == 0 || list.Count == 1)
                {
                    break;
                }
				if (Mathf.Abs(list[list.Count - 1].x - next.x) > 0.1f || Mathf.Abs(list[list.Count - 1].z - next.z) > 0.1f)
                {
                    break;
                }
                if (Vector3.Distance(next, target) <= 1f)
                {
                    break;
                }
                tmm = next;
            }
            return tmm;
        }

		public static Vector3 GetCanReachPath(this MapComponent self, Unit unit, Vector3 start, Vector3 target)
		{
            using var list = ListComponent<Vector3>.Create();
            Vector3 dir = (start - target).normalized;
            while (true)
            {
                //Game.Scene.GetComponent<RecastPathComponent>().SearchPath(self.NavMeshId, start, target, list, 2);
                self.SearchPath( unit, target, list);

                if (list.Count >= 2)
                {
                    target = list[list.Count - 1];
                    break;
                }
                if (Vector3.Distance(start, target) < 0.5f)
                {
                    break;
                }
                target = target + (0.5f * dir);
            }
            return target;
        }

		public static void SearchPath(this MapComponent self, Unit unit, Vector3 target, List<Vector3> result)
		{

			if (ConfigData.OldNavMesh)
			{
				Game.Scene.GetComponent<RecastPathComponent>().SearchPath(self.NavMeshId, unit.Position, target, result, unit.Type);
			}
			else
			{
				unit.GetComponent<PathfindingComponent>().Find(unit.Position, target, result);
			}
		}

    }
}