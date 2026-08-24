
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{

    public class JiaYuanSceneComponentAwake : AwakeSystem<JiaYuanSceneComponent>
    {
        public override void Awake(JiaYuanSceneComponent self)
        {
            self.JiaYuanFubens.Clear(); 
        }
    }

    public static class JiaYuanSceneComponentSystem
    {

        public static void OnUnitLeave(this JiaYuanSceneComponent self, Scene scene)
        {
            List<Unit> units = UnitHelper.GetUnitList(scene, UnitType.Player);
            if (units.Count > 0)
            {
                return;
            }
            long unitid = scene.GetComponent<JiaYuanDungeonComponent>().MasterId;

            long fubeninstanceid = 0;
            self.JiaYuanFubens.TryGetValue(unitid, out fubeninstanceid);

            TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
            scene.Dispose();
            if (fubeninstanceid != 0)
            {
                self.JiaYuanFubens.Remove(unitid);
            }
        }

        public static async ETTask CreateJiaYuanUnit(this JiaYuanSceneComponent self, Scene fubnescene, long masterid, long unitid)
        {
            JiaYuanComponentServer jiaYuanComponentServer = await DBHelper.GetComponent<JiaYuanComponentServer>(UnitZoneHelper.GetHomeZone(masterid), masterid);

            if (jiaYuanComponentServer.JiaYuanPastureList_7.Count > 100 
                || jiaYuanComponentServer.JianYuanPlantList_7.Count > 100
                || jiaYuanComponentServer.JiaYuanMonster_2.Count > 100)
            {
                Log.Error($"CreateJiaYuanUnit:  {masterid}");
                return;
            }

            for (int i = 0;i < jiaYuanComponentServer.JiaYuanPastureList_7.Count; i++)
            {
                UnitFactory.CreatePasture(fubnescene, jiaYuanComponentServer.JiaYuanPastureList_7[i], masterid);
            }
            for (int i = 0; i < jiaYuanComponentServer.JianYuanPlantList_7.Count; i++)
            {
                UnitFactory.CreatePlan(fubnescene, jiaYuanComponentServer.JianYuanPlantList_7[i], masterid);
            }

            long serverTime = TimeHelper.ServerNow();
            for (int i = 0; i < jiaYuanComponentServer.JiaYuanMonster_2.Count; i++)
            {
                JiaYuanMonster keyValuePair = jiaYuanComponentServer.JiaYuanMonster_2[i];
                Vector3 vector3 = new Vector3(keyValuePair.x, keyValuePair.y, keyValuePair.z);
                UnitFactory.CreateMonster(fubnescene, keyValuePair.ConfigId, vector3, new CreateMonsterInfo()
                {
                    Camp = CampEnum.CampMonster1,
                    BornTime = serverTime - keyValuePair.BornTime,
                    UnitId = keyValuePair.unitId
                }); 
            }
        }

        public static async ETTask<long> GetJiaYuanFubenId(this JiaYuanSceneComponent self, long masterid, long unitid)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.JiaYuan, masterid))
            {
                if (self.JiaYuanFubens.ContainsKey(masterid))
                {
                    return self.JiaYuanFubens[masterid];
                }
                int jiayuansceneid = CommonHelper.JiaYuanSceneID();
                long fubenid = IdGenerater.Instance.GenerateId();
                long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "JiaYuan" + masterid.ToString(), SceneType.Map);
                fubnescene.AddComponent<JiaYuanDungeonComponent>().MasterId = masterid;
                MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
                mapComponent.SetMapInfo((int)MapTypeEnum.JiaYuan, jiayuansceneid, 0);
                mapComponent.NavMeshId = LDSceneCategory.Instance.Get(jiayuansceneid).GetNavMeshId();
                await self.CreateJiaYuanUnit(fubnescene, masterid, unitid);
                TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                self.JiaYuanFubens.Add(masterid, fubenInstanceId);
                return fubenInstanceId;
            }
        }
    }
}
