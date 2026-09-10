
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{

    public class HomeSceneComponentAwake : AwakeSystem<HomeSceneComponent>
    {
        public override void Awake(HomeSceneComponent self)
        {
            self.HomeFubens.Clear(); 
        }
    }

    public static class HomeSceneComponentSystem
    {

        public static void OnUnitLeave(this HomeSceneComponent self, Scene scene)
        {
            List<Unit> units = UnitHelper.GetUnitList(scene, UnitType.Player);
            if (units.Count > 0)
            {
                return;
            }
            long unitid = scene.GetComponent<HomeDungeonComponent>().MasterId;

            long fubeninstanceid = 0;
            self.HomeFubens.TryGetValue(unitid, out fubeninstanceid);

            TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
            scene.Dispose();
            if (fubeninstanceid != 0)
            {
                self.HomeFubens.Remove(unitid);
            }
        }

        public static async ETTask CreateHomeUnit(this HomeSceneComponent self, Scene fubnescene, long masterid, long unitid)
        {
            HomeComponentServer homeComponentServer = await DBHelper.GetComponent<HomeComponentServer>(UnitZoneHelper.GetHomeZone(masterid), masterid);

            if (homeComponentServer.HomePastureList_7.Count > 100 
                || homeComponentServer.HomePlantList_7.Count > 100)
            {
                Log.Error($"CreateHomeUnit:  {masterid}");
                return;
            }

            for (int i = 0;i < homeComponentServer.HomePastureList_7.Count; i++)
            {
                UnitFactory.CreatePasture(fubnescene, homeComponentServer.HomePastureList_7[i], masterid);
            }
            for (int i = 0; i < homeComponentServer.HomePlantList_7.Count; i++)
            {
                UnitFactory.CreatePlan(fubnescene, homeComponentServer.HomePlantList_7[i], masterid);
            }

            long serverTime = TimeHelper.ServerNow();
        }

        public static async ETTask<long> GetHomeYuanFubenId(this HomeSceneComponent self, long masterid, long unitid)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Home, masterid))
            {
                if (self.HomeFubens.ContainsKey(masterid))
                {
                    return self.HomeFubens[masterid];
                }
                int homesceneid = CommonHelper.HomeSceneID();
                long fubenid = IdGenerater.Instance.GenerateId();
                long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "Home" + masterid.ToString(), SceneType.Map);
                fubnescene.AddComponent<HomeDungeonComponent>().MasterId = masterid;
                MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
                mapComponent.SetMapInfo((int)MapTypeEnum.Home, homesceneid, 0);
                mapComponent.NavMeshId = LDSceneCategory.Instance.Get(homesceneid).GetNavMeshId();
                await self.CreateHomeUnit(fubnescene, masterid, unitid);
                TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                self.HomeFubens.Add(masterid, fubenInstanceId);
                return fubenInstanceId;
            }
        }
    }
}
