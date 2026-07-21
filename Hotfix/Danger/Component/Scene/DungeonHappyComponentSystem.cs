using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
   
    [ObjectSystem]
    public class DungeonHappyComponentAwake : AwakeSystem<DungeonHappyComponent>
    {
        public override void Awake(DungeonHappyComponent self)
        {
            //先刷新一次
            self.OnTimer();
        }
    }

    public static class DungeonHappyComponentSystem
    {
        public static int GetDropId(this DungeonHappyComponent self, int openDay)
        {
            string dropinfo = LDGlobalValueCategory.Instance.Get(102).Value;
            string[] dropList = dropinfo.Split('@');
            string[] firstDrop = dropList[0].Split(';');

            for (int i = dropList.Length - 1; i >= 0; i--)
            {
                string[] dropitem = dropList[i].Split(';');
                int day = int.Parse(dropitem[0]);
                int dropid = int.Parse((dropitem[1]));

                if (openDay >= day)
                {
                    return dropid;
                }
            }
            return int.Parse(firstDrop[1]);
        }

        public static void OnTimer(this DungeonHappyComponent self)
        {
            HashSet<int> dropcells = new HashSet<int>();
            List<Unit> droplist = UnitHelper.GetUnitList(self.DomainScene(), UnitType.DropItem);
            for (int i = 0; i < droplist.Count; i++)
            {
                DropComponent dropComponent = droplist[i].GetComponent<DropComponent>();
                dropcells.Add(dropComponent.CellIndex);
            }

            int openDay = ServerHelper.GetOpenServerDay(false, self.DomainZone());
            int dropid = self.GetDropId(openDay);

            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            for (int p = 0; p < HappyFubenConfig.PositionList.Count; p++)
            {
                //空格子的概率
                if (RandomHelper.RandFloat01() < 0.3f)
                {
                    continue;
                }
                //该格子有道具
                if (dropcells.Contains(p + 1))
                {
                    continue;
                }

                List<RewardItem> rewardist = new List<RewardItem>();
                DropHelper.DropIDToDropItem(dropid, rewardist);
                if (rewardist.Count > 100)
                {
                    Log.Error($"rewardist.Count > 100:   {dropid}");
                    break;
                }

                for (int i = 0; i < rewardist.Count; i++)
                {
                    Unit dropitem = unitComponent.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), 1);
                    unitComponent.Add(dropitem);
                    dropitem.AddComponent<UnitInfoComponent>();
                    dropitem.Type = UnitType.DropItem;
                    DropComponent dropComponent = dropitem.AddComponent<DropComponent>();
                    dropComponent.SetDropReward(rewardist[i]);
                    dropComponent.SetCellIndex(p + 1);

                    Vector3 vector3 = HappyFubenConfig.PositionList[p];
                    dropitem.Position = vector3;
                    dropitem.ConfigId = rewardist[i].ItemID;
                    dropitem.AddComponent<AOIEntity, int, Vector3>(2 * 1000, dropitem.Position);
                    dropComponent.SetDropType(0);
                    dropComponent.InitDropInfo(dropitem);
                }
            }
        }
    }
}
