using System.Collections.Generic;
using System.Linq;

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
        
        public static void OnGetItem(this Unit self, int getWay, int itemType, int itemId, int itemNumber)
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
            self.GetComponent<TaskComponent>().OnGetItem_2(itemId);
        }

        /// <summary>
        /// 鉴定符根据熟练度算品质的方法
        /// </summary>
        /// <param name="bagInf0"></param>
        /// <param name="getType">1购买</param>
        public static void JianDingFuItem(BagInfo bagInf0, int shulianValue, int getType)
        {

            LDItem ldItemCof = LDItemCategory.Instance.Get(bagInf0.ItemID);
            float minValuePro = (float)shulianValue / (float)int.Parse(ldItemCof.ItemUsePar);
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

        public static void TreasureItem(Unit unit, BagInfo bagInfo)
        {

            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            if (ldItem.ItemSubType != 113 && ldItem.ItemSubType != 127)
            {
                return;
            }

            List<LDScene> dungeonConfigs = new List<LDScene>();
            List<LDScene> dungeonConfigsAll = LDSceneCategory.Instance.GetAll().Values.ToList();

            int roleLv = unit.GetComponent<UserInfoComponent>().UserInfo.Lv;

            for (int i = 0; i < dungeonConfigsAll.Count; i++)
            {
                if(LDSectionCategory.Instance.MysteryDungeonList.Contains(dungeonConfigsAll[i].Id))
                {
                    continue;
                }
                if (dungeonConfigsAll[i].GetEnterLv() <= roleLv && dungeonConfigsAll[i].Id < CommonConfig.GMDungeonId)
                {
                    dungeonConfigs.Add(dungeonConfigsAll[i]);
                }
            }

            int dungeonindex = RandomHelper.RandomNumber(0, dungeonConfigs.Count);
            int dungeonid = dungeonConfigs[dungeonindex].Id;

            int dropId = int.Parse(ldItem.ItemUsePar);
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

            bagInfo.ItemPar = $"{dungeonid}@{"TaskMove_6"}@{rewardList[0].ItemID + ";" + rewardList[0].ItemNum}";
            Log.Debug($"生成藏宝图:  {unit.Id} {unit.GetComponent<UserInfoComponent>().UserName} {rewardList[0].ItemID}");
        }


        //获取装备的鉴定属性
        public static List<HideProList> GetEquipZhuanJingHidePro(int itemtype ,int equipID, int itemID, int jianDingPinZhi, Unit unit, bool ifItem)
        {

            //获取最大值
            LDEquip ldEquipCof = LDEquipCategory.Instance.Get(equipID);
            List<HideProList> hideList = new List<HideProList>();

            //获取当前鉴定系数
            LDItem ldItemCof = LDItemCategory.Instance.Get(itemID);

            //鉴定符品质大于装备等级
            /*
            float JianDingPro = 1f;
            if (jianDingPinZhi >= itemCof.UseLv)
            {
   
            }
            else
            {
                JianDingPro = jianDingPinZhi / itemCof.UseLv * 0.5f;
            }
            */

            //测试
            //jianDingPinZhi = 99;

            //最低系数是20
            int pro = ldItemCof.UseLv;
            if (pro <= 20)
            {
                pro = 20;
            }

            if (ifItem == true && ldItemCof.UseLv < 30)
            {
                jianDingPinZhi = jianDingPinZhi + 5;
            }

            //鉴定符和当前装备的等级差
            float JianDingPro = (float)jianDingPinZhi / (float)pro;
            float addJianDingPro = 0;

            if (JianDingPro >= 1.5f)
            {
                JianDingPro = 1.5f;
                addJianDingPro += 0.2f;
            } else if (JianDingPro >= 1f) {
                addJianDingPro += 0.2f * (JianDingPro - 0.5f);
            }

            if (JianDingPro <= 0.5f)
            {
                JianDingPro = 0.5f;
            }

            int randomNum = 0;
            float randomFloat = RandomHelper.RandomNumberFloat(addJianDingPro,1) + addJianDingPro;
            Log.Info("randomFloat == " + randomFloat + "  JianDingPro = " + JianDingPro + "addJianDingPro = " + addJianDingPro);

            randomFloat = randomFloat * JianDingPro;

            if (randomFloat <= 0.25f)
            {
                randomNum = 0;
            }
            else if (randomFloat <= 0.6f)
            {
                randomNum = 1;
            }
            else if (randomFloat <= 1f)
            {
                randomNum = 2;
            }
            else
            {
                randomNum = 3;
            }
            /*
            else if (randomFloat <= 0.9f)
            {
                randomNum = 3;
            }
            */

            //65级装备默认最低2条属性
            if (ldItemCof.UseLv >= 65 && randomNum<2) {
                randomNum = 2;
            }

            //70级装备默认3条属性
            if (ldItemCof.UseLv >= 70 && randomNum < 3)
            {
                randomNum = 3;
            }

            if (ifItem)
            {
                if (randomNum >= 2)
                {
                    string noticeContent = $"恭喜玩家<color=#B6FF00>{unit.GetComponent<UserInfoComponent>().UserInfo.Name}</color>使用鉴定符鉴定装备时,一道金光装备出现<color=#FFA313>{randomNum}条极品属性</color>";
                    string noticeContentEn = $"Congratulations to the player<color=#B6FF00>{unit.GetComponent<UserInfoComponent>().UserInfo.Name}</color>Use Identifier to equipment,A flash of golden light   The equipment appeared <color=#FFA313>{randomNum} best attribute</color>";

                    ServerMessageHelper.SendBroadMessage(unit.DomainZone(), NoticeType.Notice, noticeContent, noticeContentEn);
                }
            }

            if (randomNum == 0)
            {
                return null;
            }

            return hideList;

        }
    }
}
