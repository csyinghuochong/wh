using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 附加方法
    /// </summary>
    public static class ItemAddHelper
    {

        public static void OnItemUpdate( Unit self, BagInfo bagInfo)
        {
            //通知客户端背包道具发生改变
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoUpdate = new List<BagInfo>();
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
            MessageHelper.SendToClient(self, m2c_bagUpdate);
        }
        
        
        public static void OnGetItem(this Unit self, int getWay, BagInfo bagInfo)
        {
            /*self.GetComponent<TaskComponent>().OnGetItem_2(itemId);
            self.GetComponent<TaskComponent>().OnGetItemNumber( getWay, itemId, itemNum);
            self.GetComponent<ShoujiComponent>().OnGetItem(itemId);*/
        }
        
        public static void OnGetItem(this Unit self, int getWay, int itemType, int itemId, long itemNumber)
        {
            /*self.GetComponent<TaskComponent>().OnGetItem_2(itemId);
            self.GetComponent<TaskComponent>().OnGetItemNumber( getWay, itemId, itemNum);
            self.GetComponent<ShoujiComponent>().OnGetItem(itemId);*/
        }
        
        public static void OnGetItem(this Unit self, int getWay, RewardItem rewardItem)
        {
            /*self.GetComponent<TaskComponent>().OnGetItem_2(itemId);
            self.GetComponent<TaskComponent>().OnGetItemNumber( getWay, itemId, itemNum);       
            self.GetComponent<ShoujiComponent>().OnGetItem(itemId);*/
        }

        /// <summary>
        /// 任务类型2要检测一下道具数量
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemId"></param>
        public static void OnCostItem(this Unit self, int itemId)
        {
            self.GetComponent<TaskComponentServer>().OnGetItem_2(itemId);
        }

        /// <summary>
        /// 鉴定符根据熟练度算品质的方法
        /// </summary>
        /// <param name="bagInf0"></param>
        /// <param name="getType">1购买</param>
        public static void JianDingFuItem(BagInfo bagInf0, int shulianValue, int getType)
        {

            LDItem ldItemCof = LDItemCategory.Instance.Get(bagInf0.ItemID);
            float minValuePro = 0;/// (float)shulianValue / (float)int.Parse(ldItemCof.ItemUsePar);
            if (minValuePro >= 1)
            {
                minValuePro = 1;
            }
            if (minValuePro <= 0.2f)
            {
                minValuePro = 0.2f;
            }
            int minValue = (int)(minValuePro * 50f);
            int maxValue = (int)(minValuePro * 102f);
            int randValue = RandomHelper.RandomNumber(minValue, maxValue);
            if (randValue > 100) {
                randValue = 100;
            }
            bagInf0.ItemPar = randValue.ToString();
        }

        private static List<LDScene> TreasureDungeonPool;
        private static HashSet<int> TreasureMysterySet;

        private static void EnsureTreasureDungeonPool()
        {
            if (TreasureDungeonPool != null)
            {
                return;
            }

            TreasureMysterySet = new HashSet<int>(LDSectionCategory.Instance.MysteryDungeonList);
            TreasureDungeonPool = new List<LDScene>();
            foreach (KeyValuePair<int, LDScene> kv in LDSceneCategory.Instance.GetAll())
            {
                LDScene scene = kv.Value;
                if (TreasureMysterySet.Contains(scene.Id))
                {
                    continue;
                }
                if (scene.Id >= CommonConfig.GMDungeonId)
                {
                    continue;
                }
                TreasureDungeonPool.Add(scene);
            }
        }

        public static void TreasureItem(Unit unit, BagInfo bagInfo)
        {

            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            if (ldItem.ItemType != 113 && ldItem.ItemType != 127)
            {
                return;
            }

            EnsureTreasureDungeonPool();
            List<LDScene> dungeonConfigs = new List<LDScene>();
            int roleLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;

            for (int i = 0; i < TreasureDungeonPool.Count; i++)
            {
                LDScene scene = TreasureDungeonPool[i];
                if (scene.GetEnterLv() <= roleLv)
                {
                    dungeonConfigs.Add(scene);
                }
            }

            if (dungeonConfigs.Count == 0)
            {
                Log.Warning($"TreasureItem no dungeon: lv={roleLv}");
                return;
            }

            int dungeonindex = RandomHelper.RandomNumber(0, dungeonConfigs.Count);
            int dungeonid = dungeonConfigs[dungeonindex].Id;

            int dropId = -1;// int.Parse(ldItem.ItemUsePar);
            List<RewardItem> rewardList = new List<RewardItem>();

            //获取最终奖励
            if (RandomHelper.RandFloat01() <= 0.7f)
            {
                if (dropId == 0)
                {
                    Log.Warning($"dropId == 0:  {ldItem.Id}");
                }
                DropHelper.DropIDToDropItem_2(dropId, rewardList);
            }
            else {
                int baotutype = 1;
                if (bagInfo.ItemID == 10010039) 
                {
                    baotutype = 1;
                }

                if (bagInfo.ItemID == 10010040)
                {
                    baotutype = 2;
                }
                int dropID2 = CommonHelper.TreasureToDropID(dungeonid, roleLv, baotutype);
                if (dropID2 == 0)
                {
                    Log.Warning($"TreasureToDropID: {roleLv} {baotutype}");
                }

                DropHelper.DropIDToDropItem_2(dropID2, rewardList);
            }

            if (rewardList.Count == 0)
            {
                Log.Warning($"TreasureItem empty reward: {bagInfo.ItemID}");
                return;
            }

            bagInfo.ItemPar = $"{dungeonid}@{"TaskMove_6"}@{rewardList[0].ItemID + ";" + rewardList[0].ItemNum}";
            Log.Debug($"生成藏宝图:  {unit.Id} {unit.GetComponent<RoleInfoComponentServer>().UserName} {rewardList[0].ItemID}");
        }

    }
}
