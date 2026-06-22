using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class BagComponentAwakeSystem : AwakeSystem<BagComponent>
    {

        public override void Awake(BagComponent self)
        {
          
        }
    }

    public static class BagComponentSystem
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool HaveOccEquip(this BagComponent self)
        {
            List<BagInfo> allequiplist = new List<BagInfo>();
            allequiplist.AddRange(self.EquipList);
            allequiplist.AddRange(self.EquipList_2);

            for (int i = 0; i < allequiplist.Count; i++)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(allequiplist[i].ItemID);
                int equipType = ItemHelper.GetNewEquipType(allequiplist[i]);
                if (ldItem.ItemType == 3
                    && equipType >= 0 && equipType <= 100
                    && ldItem.ItemType >= 0 && ldItem.ItemType <= 12)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<PropertyValue> GetGemProLists(this BagComponent self)
        {
            List<PropertyValue> list = new List<PropertyValue>();
            for (int i = 0; i < self.GemList.Count; i++)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(self.GemList[i].ItemID);
                string itemUsePar = ldItem.ItemUsePar;
                if (string.IsNullOrEmpty(itemUsePar) || itemUsePar == "0")
                {
                    continue;
                }
                string[] attributes = itemUsePar.Split('@');
                for (int a = 0; a < attributes.Length; a++)
                {
                    string[] attributeItem = attributes[a].Split(';');
                    int hideId = int.Parse(attributeItem[0]);
                    long hide_value = 0;
                    if (NumericHelp.GetNumericValueType(hideId) == 2)
                    {
                        hide_value = (long)(float.Parse(attributeItem[1]) * 10000);
                    }
                    else
                    {
                        hide_value = long.Parse(attributeItem[1]);
                    }
                    list.Add(new PropertyValue() { HideID = hideId, HideValue = hide_value });
                }
            }

            return list;
        }
        
        public static List<BagInfo> GetItemByLoc(this BagComponent self, ItemLocType itemEquipType)
        {
            List<BagInfo> ItemTypeList = null;
            switch (itemEquipType)
            {
                case ItemLocType.ItemLocBag:
                    ItemTypeList = self.BagItemList;
                    break;
                case ItemLocType.ItemPetHeXinBag:
                    ItemTypeList = self.BagItemPetHeXin;
                    break;
                case ItemLocType.ItemLocGem:
                    ItemTypeList = self.GemList;
                    break;
                case ItemLocType.ItemLocEquip:
                    ItemTypeList = self.EquipList;
                    break;
                case ItemLocType.ItemPetHeXinEquip:
                    ItemTypeList = self.PetHeXinList;
                    break;
                case ItemLocType.ItemWareHouse1:
                    ItemTypeList = self.Warehouse1;
                    break;
                case ItemLocType.ItemWareHouse2:
                    ItemTypeList = self.Warehouse2;
                    break;
                case ItemLocType.ItemWareHouse3:
                    ItemTypeList = self.Warehouse3;
                    break;
                case ItemLocType.ItemWareHouse4:
                    ItemTypeList = self.Warehouse4;
                    break;
                case ItemLocType.JianYuanWareHouse1:
                    ItemTypeList = self.JianYuanWareHouse1;
                    break;
                case ItemLocType.JianYuanWareHouse2:
                    ItemTypeList = self.JianYuanWareHouse2;
                    break;
                case ItemLocType.JianYuanWareHouse3:
                    ItemTypeList = self.JianYuanWareHouse3;
                    break;
                case ItemLocType.JianYuanWareHouse4:
                    ItemTypeList = self.JianYuanWareHouse4;
                    break;
                case ItemLocType.JianYuanTreasureMapStorage1:
                    ItemTypeList = self.JianYuanTreasureMapStorage1;
                    break;
                case ItemLocType.JianYuanTreasureMapStorage2:
                    ItemTypeList = self.JianYuanTreasureMapStorage2;
                    break;
                case ItemLocType.ChouKaWarehouse:
                    ItemTypeList = self.ChouKaWarehouse;
                    break;
                /*case ItemLocType.ItemLocEquip_2:
                    ItemTypeList = self.EquipList_2;
                    break;*/
                case ItemLocType.SeasonJingHe:
                    ItemTypeList = self.SeasonJingHe;
                    break;
                case ItemLocType.PetLocEquip:
                    ItemTypeList = self.PetEquipList;
                    break;
                case ItemLocType.GemWareHouse1:
                    ItemTypeList = self.GemWareHouse1;
                    break;
            }
            return ItemTypeList;
        }

        public static void ZhengLiItemList(this BagComponent self, Dictionary<int, List<BagInfo>> ItemSameList, M2C_RoleBagUpdate m2c_bagUpdate)
        {
            foreach (var item in ItemSameList)
            {
                List<BagInfo> bagInfos = item.Value;
                if (bagInfos.Count == 1)
                {
                    continue;
                }
                LDItem ldItemCof = LDItemCategory.Instance.Get(bagInfos[0].ItemID);

                int totalNum = 0;
                int needGrid = 0;
                int finalNum = 0;
                for (int i = 0; i < bagInfos.Count; i++)
                {
                    totalNum += (int)bagInfos[i].ItemNum;
                }
                needGrid = totalNum / ldItemCof.ItemPileSum;
                needGrid += (totalNum % ldItemCof.ItemPileSum > 0 ? 1 : 0);
                finalNum = totalNum - (needGrid - 1) * ldItemCof.ItemPileSum;

                if (needGrid <= 0 || needGrid > bagInfos.Count)
                {
                    Console.WriteLine($"RecvItemSortError: {self.GetParent<Unit>().Id} {bagInfos[0].ItemID}   {totalNum}   {needGrid}  {bagInfos.Count}");
                    continue;
                }
                bagInfos[needGrid - 1].ItemNum = finalNum;
                m2c_bagUpdate.BagInfoUpdate.Add(bagInfos[needGrid - 1]);
                for (int i = 0; i < needGrid - 1; i++)
                {
                    bagInfos[i].ItemNum = ldItemCof.ItemPileSum;
                    m2c_bagUpdate.BagInfoUpdate.Add(bagInfos[i]);
                }
                //删除后面的空格子
                for (int i = needGrid; i < bagInfos.Count; i++)
                {
                    bagInfos[i].ItemNum = 0;
                    m2c_bagUpdate.BagInfoDelete.Add(bagInfos[i]);
                }
            }
        }


        public static void OnRecvItemSort(this BagComponent self, ItemLocType itemEquipType)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemEquipType);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();


            //绑定的
            Dictionary<int, List<BagInfo>> ItemSameList_1 = new Dictionary<int, List<BagInfo>>();
            //未绑定
            Dictionary<int, List<BagInfo>> ItemSameList_2 = new Dictionary<int, List<BagInfo>>();
            //找出可以堆叠并且格子未放满的道具
            for (int i = 0; i < ItemTypeList.Count; i++)
            {
                BagInfo bagInfo = ItemTypeList[i];

                //最大堆叠数量
                LDItem ldItemCof = LDItemCategory.Instance.Get(bagInfo.ItemID);
                if (bagInfo.ItemNum >= ldItemCof.ItemPileSum)
                {
                    continue;
                }

                if (bagInfo.isBinging)
                {
                    if (!ItemSameList_1.ContainsKey(bagInfo.ItemID))
                    {
                        ItemSameList_1[bagInfo.ItemID] = new List<BagInfo>();
                    }
                    ItemSameList_1[bagInfo.ItemID].Add(bagInfo);
                }
                else
                {
                    if (!ItemSameList_2.ContainsKey(bagInfo.ItemID))
                    {
                        ItemSameList_2[bagInfo.ItemID] = new List<BagInfo>();
                    }
                    ItemSameList_2[bagInfo.ItemID].Add(bagInfo);
                }
            }

            self.ZhengLiItemList(ItemSameList_1, m2c_bagUpdate);
            self.ZhengLiItemList(ItemSameList_2, m2c_bagUpdate);

            for (int i = ItemTypeList.Count - 1; i >= 0; i--)
            {
                if (ItemTypeList[i].ItemNum == 0)
                {
                    ItemTypeList.RemoveAt(i);
                }
            }

            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);

            ItemHelper.ItemLitSort(ItemTypeList);
        }

        public static void CheckValiedItem(this BagComponent self, List<BagInfo> bagInfos, int occ, int occTwo)
        {
            Unit unit = self.GetParent<Unit>();
           
            for (int i = bagInfos.Count - 1; i >= 0; i--)
            {
                if( !ItemNewHelper.CheckValiedItem(bagInfos[i]))
                {
                    bagInfos.RemoveAt(i);
                    continue;
                }
                
              
                if (bagInfos[i].ItemNum <= 0)
                {
                    Console.WriteLine($"CheckValiedItem22:  {bagInfos[i].ItemID}   {bagInfos[i].ItemNum}");
                    bagInfos[i].ItemNum = 1;
                }
                
                BagInfo bagInfoitem = bagInfos[i];
                //如果有宝石但是没空 加打印。 目前主处理某一个人的
            }
        }

        //获取自身所有的道具
        public static List<BagInfo> GetAllItems(this BagComponent self, int occ, int occTwo)
        {
            List<BagInfo> bagList = new List<BagInfo>();

            self.CheckValiedItem(self.GemList, occ, occTwo);
            self.CheckValiedItem(self.BagItemList, occ, occTwo);
            self.CheckValiedItem(self.EquipList, occ, occTwo);
            self.CheckValiedItem(self.BagItemPetHeXin, occ, occTwo);
            self.CheckValiedItem(self.PetHeXinList, occ, occTwo);
            self.CheckValiedItem(self.Warehouse1, occ, occTwo);
            self.CheckValiedItem(self.Warehouse2, occ, occTwo);
            self.CheckValiedItem(self.Warehouse3, occ, occTwo);
            self.CheckValiedItem(self.Warehouse4, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanWareHouse1, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanWareHouse2, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanWareHouse3, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanWareHouse4, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanTreasureMapStorage1, occ, occTwo);
            //self.CheckValiedItem(self.JianYuanTreasureMapStorage2, occ, occTwo);
            //self.CheckValiedItem(self.ChouKaWarehouse, occ, occTwo);
            self.CheckValiedItem(self.EquipList_2, occ, occTwo);
            //self.CheckValiedItem(self.SeasonJingHe, occ, occTwo);
            //self.CheckValiedItem(self.PetEquipList, occ, occTwo);
            //self.CheckValiedItem(self.GemWareHouse1, occ, occTwo);

            for (int i =  self.EquipList.Count - 1; i >=0; i--)
            {
                LDEquip ldItem = LDEquipCategory.Instance.Get(self.EquipList[i].ItemID);
            }

            bagList.AddRange(self.GemList);
            bagList.AddRange(self.BagItemList);
            bagList.AddRange(self.BagItemPetHeXin);
            bagList.AddRange(self.EquipList);
            bagList.AddRange(self.PetHeXinList);
            bagList.AddRange(self.Warehouse1);
            bagList.AddRange(self.Warehouse2);
            bagList.AddRange(self.Warehouse3);
            bagList.AddRange(self.Warehouse4);
            bagList.AddRange(self.JianYuanWareHouse1);
            bagList.AddRange(self.JianYuanWareHouse2);
            bagList.AddRange(self.JianYuanWareHouse3);
            bagList.AddRange(self.JianYuanWareHouse4);
            bagList.AddRange(self.JianYuanTreasureMapStorage1);
            bagList.AddRange(self.JianYuanTreasureMapStorage2);
            bagList.AddRange(self.ChouKaWarehouse);
            bagList.AddRange(self.EquipList_2);
            bagList.AddRange(self.SeasonJingHe);
            bagList.AddRange(self.PetEquipList);
            bagList.AddRange(self.GemWareHouse1);

            return bagList;
        }

        public static List<BagInfo> GetIdItemListByLoc(this BagComponent self, int itemId, ItemLocType loc)
        {
            List<BagInfo> baginfo = new List<BagInfo>();
            List<BagInfo> bagList = self.GetItemByLoc(loc);
            for (int i = 0; i < bagList.Count; i++)
            {
                if (bagList[i].ItemID == itemId)
                {
                    baginfo.Add(bagList[i]);
                }
            }
            return baginfo;
        }

        public static List<BagInfo> GetIdItemList(this BagComponent self, int itemId)
        {
            List<BagInfo> baginfo = new List<BagInfo>();
            for (int i = 0; i < self.BagItemList.Count; i++)
            {
                if (self.BagItemList[i].ItemID == itemId)
                {
                    baginfo.Add(self.BagItemList[i]);
                }
            }
            return baginfo;
        }

        public static int GetNeedCell(this BagComponent self, List<RewardItem> itemids, ItemLocType itemLocType)
        {
            int needcell = 0;
            for  ( int i =0; i < itemids.Count; i++ )
            {
                LDItem ldItem = LDItemCategory.Instance.Get(itemids[i].ItemID);
                long curNumber = self.GetItemNumber(ItemBigType.Type_Item, itemids[i].ItemID, itemLocType);

                if (curNumber > 0 && curNumber + itemids[i].ItemNum < ldItem.ItemPileSum)
                {
                    needcell = 0;
                }
                else
                {
                    int temp = 0;
                    temp += (int)(1f * itemids[i].ItemNum / ldItem.ItemPileSum);
                    temp += (itemids[i].ItemNum % ldItem.ItemPileSum > 0 ? 1 : 0);

                    needcell += temp;

                    if (temp != 1)
                    {
                        //Console.WriteLine($"needcell:{needcell}  ItemNum:{itemids[i].ItemNum}   ItemPileSum:{Item.ItemPileSum}");
                    }
                }
            }

            return needcell;
        }

        //获取某个道具的数量
        public static long GetItemNumber(this BagComponent self, int itemType,  int itemId, ItemLocType itemLocType = ItemLocType.ItemLocBag)
        {
            int userDataType = ItemNewHelper.GetItemToUserDataType(itemType, itemId);
            long number = 0;
            switch (userDataType)
            {
                case UserDataType.None:
                    List<BagInfo> bagInfos = self.GetItemByLoc(itemLocType);
                    for (int i = 0; i < bagInfos.Count; i++)
                    {
                        if (bagInfos[i].ItemID == itemId)
                        {
                            number += bagInfos[i].ItemNum;
                        }
                    }
                    break;
                case UserDataType.Gold:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.Gold;
                    break;
                case UserDataType.Diamond:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.Diamond;
                    break;
                case UserDataType.V1TotalPoints:
                    number = (long)self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.V1TotalPoints;
                    break;
                case UserDataType.RongYu:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.RongYu;
                    break;
                case UserDataType.JiaYuanFund:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.JiaYuanFund;
                    break;
                case UserDataType.UnionContri:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.UnionZiJin;
                    break;
                case UserDataType.SeasonCoin:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.SeasonCoin;
                    break;
                case UserDataType.WeiJingGold:
                    number = self.GetParent<Unit>().GetComponent<UserInfoComponent>().UserInfo.WeiJingGold;
                    break;
                default:
                    break;
            }
            return number;
        }


        //根据ID获取对应的背包数据
        public static BagInfo GetItemByLoc(this BagComponent self, ItemLocType itemLocType, long bagId)
        {
            if (bagId == 0)
                return null;
            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemLocType);
            for (int i = 0; i < ItemTypeList.Count; i++)
            {
                if (ItemTypeList[i].BagInfoID == bagId)
                {
                    return ItemTypeList[i];
                }
            }
            return null;
        }

        public static bool IsBagFull(this BagComponent self)
        {
            return self.GetBagLeftCell() <= 0;
        }

        public static int GetBagLeftCell(this BagComponent self)
        {
            return self.GetBagTotalCell() - self.BagItemList.Count;
        }

        public static int GetBagTotalCell(this BagComponent self)
        {
            if (self.WarehouseAddedCell.Count == 0 || self.AdditionalCellNum.Count == 0)
            {
                return LDGlobalValueCategory.Instance.BagInitCapacity;
            }
            return self.WarehouseAddedCell[0] + self.AdditionalCellNum[0] + + LDGlobalValueCategory.Instance.BagInitCapacity;
        }

        public static bool IsHourseFullByLoc(this BagComponent self, int hourseId)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return ItemTypeList.Count >= self.GetHourseTotalCell(hourseId);
        }

        public static int GetHourseLeftCell(this BagComponent self, int hourseId)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return self.GetHourseTotalCell(hourseId) - ItemTypeList.Count;
        }

        public static int GetHourseTotalCell(this BagComponent self, int hourseId)
        {
            int storeCapacity = LDGlobalValueCategory.Instance.HourseInitCapacity;
            if (hourseId == (int)ItemLocType.GemWareHouse1)
            {
                storeCapacity = LDGlobalValueCategory.Instance.GemStoreInitCapacity;
            }
            return storeCapacity + self.WarehouseAddedCell[hourseId] + self.AdditionalCellNum[hourseId];
        }

        /// <summary>
        /// 获取抽卡仓库剩余的格数，上限100
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetChouKaLeftSpace(this BagComponent self)
        {
            return 100 - self.ChouKaWarehouse.Count;
        }

        public static void OnChangeItemLoc(this BagComponent self, BagInfo bagInfo, ItemLocType itemLocTypeTo, ItemLocType itemLocTypeFrom)
        {
            List<BagInfo> ItemTypeListSour = self.GetItemByLoc(itemLocTypeFrom);
            for (int i = ItemTypeListSour.Count - 1; i >= 0; i--)
            {
                if (ItemTypeListSour[i].BagInfoID == bagInfo.BagInfoID)
                {
                    ItemTypeListSour.RemoveAt(i);
                }
            }

            List<BagInfo> ItemTypeListDest = self.GetItemByLoc(itemLocTypeTo);
            bagInfo.Loc = (int)itemLocTypeTo;
            ItemTypeListDest.Add(bagInfo);
        }

        /// <summary>
        /// 是否有装备技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsHaveEquipSkill(this BagComponent self, int skillId, long xilianequip)
        {
            for (int i = 0; i < self.EquipList.Count; i++)
            {
                if (self.EquipList[i].BagInfoID == xilianequip)
                {
                    continue;
                }

                LDItem ldItem = LDItemCategory.Instance.Get(self.EquipList[i].ItemID);
                /*if (Item.SkillID.Contains(skillId.ToString()))
                {
                    return true;
                }*/
                if (self.EquipList[i].HideSkillLists.Contains(skillId))
                {
                    return true;
                }
                if (self.EquipList[i].InheritSkills.Contains(skillId))
                {
                    return true;
                }
            }
            return false;
        }

        public static void OnResetSeason(this BagComponent self, bool notice)
        { 
            self.SeasonJingHePlan = 0;
            self.SeasonJingHe.Clear();

            self.ClearJingHeItem(self.BagItemList);
            self.ClearJingHeItem(self.Warehouse1);
            self.ClearJingHeItem(self.Warehouse2);
            self.ClearJingHeItem(self.Warehouse3);
            self.ClearJingHeItem(self.Warehouse4);
            self.ClearJingHeItem(self.SeasonJingHe);
        }

        public static void ClearJingHeItem(this BagComponent self, List<BagInfo> bagInfos)
        {
            for (int i = bagInfos.Count - 1; i >= 0; i--)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(bagInfos[i].ItemID);
                int equipType = ItemHelper.GetNewEquipType(bagInfos[i]);
                if (equipType == 201)
                {
                    bagInfos.RemoveAt(i);
                }
            }
        }

        public static List<BagInfo> GetCurJingHeList(this BagComponent self)
        {
            List<BagInfo> bagInfos = new List<BagInfo>();
            for (  int i = 0; i < self.SeasonJingHe.Count; i++ )
            {
                if (self.SeasonJingHe[i].EquipPlan == self.SeasonJingHePlan)
                {
                    bagInfos.Add(self.SeasonJingHe[i]);
                }
            }
            return bagInfos;
        }

        public static bool IsEquipJingHe(this BagComponent self, int itemId)
        {
            List<BagInfo> bagInfos  =self.GetCurJingHeList();
            for (int i = 0; i < bagInfos.Count; i++)
            {
                if (bagInfos[i].ItemID == itemId)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<int> GetEquipTianFuIds(this BagComponent self)
        {
            List<int> equiptianfuids = new List<int>(); 
            List<BagInfo> equiplist = new List<BagInfo>();
            equiplist.AddRange(self.EquipList );
            equiplist.AddRange(self.EquipList_2);
            equiplist.AddRange(self.SeasonJingHe);

            for (int i = 0; i < self.EquipList.Count; i++)
            {
                if (self.EquipList[i].ItemType != ItemBigType.Type_Equip)
                {
                    continue;
                }

                if (!LDItemCategory.Instance.Contain(self.EquipList[i].ItemID))
                {
                    continue;
                }

                LDItem ldItem = LDItemCategory.Instance.Get(self.EquipList[i].ItemID);

                LDEquip ldEquip = LDEquipCategory.Instance.Get(self.EquipList[i].ItemID);
                /*if (equip.TianFuId != 0)
                {
                    equiptianfuids.Add(equip.TianFuId);
                }*/
            }

            return equiptianfuids;
        }

        public static BagInfo GetJingHeByWeiZhi(this BagComponent self, int subType)
        {
            List<BagInfo> bagInfos = self.GetCurJingHeList();
            for (int i = 0; i < bagInfos.Count; i++)
            {
                if (bagInfos[i].EquipIndex == subType)
                { 
                    return bagInfos[i]; 
                }
            }
            return null;
        }

        public static List<BagInfo> GetEquipListByWeizhi(this BagComponent self, ItemLocType equipIndex, int position)
        {
            List<BagInfo> bagInfos = new List<BagInfo>();
            List<BagInfo> equipList = self.GetItemByLoc(equipIndex);
            for (int i = 0; i < equipList.Count; i++)
            {
                int caowei = ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID);
                if (caowei == position)
                {
                    bagInfos.Add(equipList[i]);
                }
            }
            return bagInfos;
        }

        public static int GetMaxQiangHuaLevel(this BagComponent self)
        {
            int maxLevel = 0;
            return maxLevel;
        }

        //获取某个装备位置的道具数据
        public static BagInfo GetEquipBySubType(this BagComponent self, ItemLocType equipIndex, int subType)
        {
            List<BagInfo> equipList = self.GetItemByLoc(equipIndex);
            for (int i = 0; i < equipList.Count; i++)
            {
                int caowei = ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID);
                if (caowei == subType)
                {
                    return equipList[i];
                }
            }
            return null;
        }

        //获取某个装备位置的道具数据
        public static BagInfo GetMagicEquipBySubType(this BagComponent self, ItemLocType equipIndex, int position)
        {
            List<BagInfo> equipList = self.GetItemByLoc(equipIndex);
            for (int i = 0; i < equipList.Count; i++)
            {
                LDItem ldItemCof = LDItemCategory.Instance.Get(equipList[i].ItemID);
                if ((ldItemCof.ItemType == 4001 || ldItemCof.ItemType == 4002) && equipList[i].EquipIndex == position)
                {
                    return equipList[i];
                }
            }
            return null;
        }

        public static void OnLogin(this BagComponent self, int robotId)
        {

            Unit unit = self.GetParent<Unit>();
            int zodiacnumber = self.GetZodiacnumber();
            unit.GetComponent<ChengJiuComponent>().TriggerEvent(ChengJiuTargetEnum.ZodiacEquipNumber_215, 0, zodiacnumber);


            ///old
            //int warehourseNumber = (int)ItemLocType.ItemLocMax - 5;
            //if (self.WarehouseAddedCell.Count < warehourseNumber)  // 11)
            //{
            //    for (int i = self.WarehouseAddedCell.Count; i < warehourseNumber; i++)
            //    {
            //        self.WarehouseAddedCell.Add(0);
            //    }
            //}

            if (self.BagAddedCell >= 0)
            {
                //if (self.WarehouseAddedCell.Count > 0 && self.WarehouseAddedCell.Count < (int)ItemLocType.ItemLocMax - 5)
                if (self.WarehouseAddedCell.Count > 0 )
                {
                    List<int> bagaddCell = new List<int>() { self.BagAddedCell, 0,0,0,0 };
                    self.WarehouseAddedCell.InsertRange(0, bagaddCell);
                }

                self.BagAddedCell = -1;  //该字段废弃掉
            }

            for (int i = self.WarehouseAddedCell.Count; i < (int)ItemLocType.ItemLocMax; i++)
            {
                self.WarehouseAddedCell.Add(0);
            }
            for (int i = self.AdditionalCellNum.Count; i < (int)ItemLocType.ItemLocMax; i++)
            {
                self.AdditionalCellNum.Add(0);
            }
            

            if (robotId != 0)
            {
                int[] equipList = new int[0];
                UserInfoComponent userInfoComponent = unit.GetComponent<UserInfoComponent>();
                RobotConfig robotConfig = RobotConfigCategory.Instance.Get(robotId);

                if (robotConfig.Behaviour != 1 && robotConfig.Level > userInfoComponent.UserInfo.Lv)
                {
                    userInfoComponent.UserInfo.Lv = robotConfig.Level;
                }
                if (robotConfig.EquipList != null)
                {
                    equipList = robotConfig.EquipList != null ? robotConfig.EquipList : equipList;
                }
                else
                {
                    equipList = LDItemCategory.Instance.GetRandomEquipList(userInfoComponent.UserInfo.Occ, userInfoComponent.UserInfo.Lv);
                }
                for (int i = 0; i < equipList.Length; i++)
                {
                    if (equipList[i] == 0)
                    {
                        continue;
                    }
                    int caowei  = ItemNewHelper.GetNewEquipCaoWei(equipList[i]);
                    if (self.GetEquipBySubType(ItemLocType.ItemLocEquip, caowei) != null)
                    {
                        continue;
                    }
                    if (self.GetIdItemList(equipList[i]).Count > 0)
                    {
                        continue;
                    }

                    self.OnAddItemData($"{equipList[i]};1", $"{ItemGetWay.System}_0", false);
                    List<BagInfo> bagInfo = self.GetIdItemList(equipList[i]);
                    if (bagInfo.Count == 0)
                    {
                        Log.Warning("机器人装备 bagInfo.Count == 0");
                        continue;
                    }

                    self.OnChangeItemLoc(bagInfo[0], ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                }
            }
        }

        public static int GetZodiacnumber(this BagComponent self)
        {
            int number = 0;
            for (int i = 0; i < self.EquipList.Count; i++)
            {
               
            }

            return number;
        }

        public static int GetWuqiItemId(this BagComponent self)
        {
            BagInfo bagInfo = self.GetEquipBySubType(ItemLocType.ItemLocEquip, (int)EquipCaoWeiTypeEnum.Wuqi_1);
            return bagInfo != null ? bagInfo.ItemID : 0;
        }

        public static void OnAddJianDing(this BagComponent self)
        {
            self.OnAddItemData( $"11200001;1@11200002;1@11200003;1", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}" );
        }

        //字符串添加道具 
        public static bool OnAddItemData(this BagComponent self, string rewardItems, string getType, bool notice = true)
        {
            List<RewardItem> costItems = ItemHelper.GetRewardItems(rewardItems);
            return self.OnAddItemData(costItems, string.Empty, getType, notice);
        }

        public static void OnAddItemData(this BagComponent self, List<BagInfo> bagInfos, string getType)
        {
            for (int i = 0; i < bagInfos.Count; i++)
            {
                self.OnAddItemData(bagInfos[i], getType);
            }
        }

        public static bool OnAddItemData(this BagComponent self, BagInfo bagInfo, string getType)
        {
            LDItem ldItemCof = LDItemCategory.Instance.Get(bagInfo.ItemID);
            int maxPileSum = ldItemCof.ItemPileSum;

            if (maxPileSum > 1 || bagInfo.BagInfoID == 0)
            {
                return self.OnAddItemData($"{bagInfo.ItemID};{bagInfo.ItemNum}", string.IsNullOrEmpty(bagInfo.GetWay) ? getType : bagInfo.GetWay);
            }
            else
            {
                self.BagItemList.Add(bagInfo);

                M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
                m2c_bagUpdate.BagInfoAdd.Add(bagInfo);
                //通知客户端背包道具发生改变
                MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);

                //检测任务需求道具
                ItemAddHelper.OnGetItem(self.GetParent<Unit>(), int.Parse(getType.Split('_')[0]), bagInfo);
                return true;
            }
        }

        public static void OnAddItemToStore(this BagComponent self, int itemlockType, int itemid, int itemnumber, string getType)
        {
            BagInfo useBagInfo = new BagInfo();
            useBagInfo.ItemID = itemid;
            useBagInfo.ItemNum = itemnumber;
            useBagInfo.Loc = itemlockType;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GemHole = ItemHelper.DefaultGem;
            useBagInfo.GemIDNew = ItemHelper.DefaultGem;
            useBagInfo.GetWay = getType;
            self.GetItemByLoc((ItemLocType)useBagInfo.Loc).Add(useBagInfo);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
        }

        public static void OnAddItemDataNewCell(this BagComponent self, BagInfo bagInfo, int itemnumber)
        {
            int itemid = bagInfo.ItemID;
            BagInfo useBagInfo = new BagInfo();
            useBagInfo.ItemID = itemid;
            useBagInfo.ItemNum = itemnumber;
            LDItem ldItemCof = LDItemCategory.Instance.Get(itemid);
            useBagInfo.Loc = ldItemCof.ItemType == (int)ItemTypeEnum.PetHeXin ? (int)ItemLocType.ItemPetHeXinBag : (int)ItemLocType.ItemLocBag;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GemHole = ItemHelper.DefaultGem;
            useBagInfo.GemIDNew = ItemHelper.DefaultGem;
            useBagInfo.GetWay = bagInfo.GetWay;
            useBagInfo.isBinging = bagInfo.isBinging;
            self.GetItemByLoc((ItemLocType)useBagInfo.Loc).Add(useBagInfo);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
        }

        /// <summary>
        /// 暂时只有宝石仓库用到
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemId"></param>
        /// <param name="itemNumber"></param>
        /// <param name="itemLocType"></param>
        /// <returns></returns>
        public static bool CheckCanAddItem(this BagComponent self, int itemId , int itemNumber, ItemLocType itemLocType)
        {
            if (itemLocType == ItemLocType.GemWareHouse1)
            {
                if (self.IsHourseFullByLoc((int)itemLocType))
                {
                    List<BagInfo> bagInfoList = self.GetItemByLoc(itemLocType);
                    for (int i = 0; i <bagInfoList.Count; i++)
                    {
                        if (bagInfoList[i].ItemID!= itemId)
                        {
                            continue;
                        }
                        LDItem ldItem = LDItemCategory.Instance.Get(itemId);
                        if (bagInfoList[i].ItemNum + itemNumber <= ldItem.ItemPileSum)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else 
                {

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        //添加背包道具道具[支持同时添加多个]
        public static bool OnAddItemData(this BagComponent self, List<RewardItem> rewardItems_init, string makeUserID, string getWay, bool notice = true, bool gm = false, ItemLocType UseLocType = ItemLocType.ItemLocBag)
        {
            if (rewardItems_init.Count <= 0)
            {
                return false;
            }
            if (rewardItems_init[0].ItemType == ItemBigType.Type_None)
            {
                Log.Error("rewardItems_init[0].ItemType == ItemBigType.Type_None");
                return false;
            }
            
            int bagCellNumber = 0;
            int petHeXinNumber = 0;
            string[] getWayInfo = getWay.Split('_');
            int getType = int.Parse(getWayInfo[0]);
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot() && getType == ItemGetWay.PickItem)
            {
                return true;
            }

            if (getType == ItemGetWay.GM)
            {
                gm = true;
            }

            List<RewardItem> rewardItems = new List<RewardItem>();
            for (int i = rewardItems_init.Count - 1; i >= 0; i--)
            {
                bool have = false;
                for (int bb = rewardItems.Count - 1; bb >= 0; bb--)
                {
                    if (rewardItems[bb].ItemID == rewardItems_init[i].ItemID
                        && rewardItems[bb].ItemType == rewardItems_init[i].ItemType)
                    {
                        rewardItems[bb].ItemNum += rewardItems_init[i].ItemNum;
                        have = true;
                        break;
                    }
                }

                if (!have)
                {
                    RewardItem item = new RewardItem();
                    item.ItemType =  rewardItems_init[i].ItemType;
                    item.ItemID = rewardItems_init[i].ItemID;
                    item.ItemNum = rewardItems_init[i].ItemNum;
                    rewardItems.Add(item);
                }
            }

            for (int i = rewardItems.Count - 1; i >= 0; i--)
            {
                RewardItem rewardItem = rewardItems[i];
                
                if (!ItemNewHelper.CheckValiedItem(rewardItem))
                {
                    rewardItems.RemoveAt(i);
                    continue;
                }

                //获取类型不进背包
                if (rewardItem.ItemType == ItemBigType.Type_Money)
                {
                    continue;
                }

                long ItemPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                if (UseLocType >= ItemLocType.ItemWareHouse1)
                {
                    continue;
                }
                
                /*if (itemCof.ItemType == ItemTypeEnum.PetHeXin)
                {
                    petHeXinNumber += rewardItems[i].ItemNum;
                    continue;
                }*/

                if (ItemPileSum == 1)
                {
                    bagCellNumber += rewardItems[i].ItemNum;
                }
                else if (rewardItems[i].ItemNum <= ItemPileSum)
                {
                    bagCellNumber += 1;
                }
                else
                {
                    bagCellNumber += (int)(1f * rewardItems[i].ItemNum / ItemPileSum);
                    bagCellNumber += (rewardItems[i].ItemNum % ItemPileSum > 0 ? 1 : 0);
                }
            }
            if (rewardItems.Count == 0)
            {
                return true;
            }


            if (getType != ItemGetWay.GemHeCheng)
            {
                if (bagCellNumber > self.GetBagLeftCell() && UseLocType == ItemLocType.ItemLocBag)
                {
                    return false;
                }
                if (petHeXinNumber > 0 && (petHeXinNumber + self.BagItemPetHeXin.Count > CommonConfig.PetHeXinMax) && UseLocType == ItemLocType.ItemLocBag)
                {
                    return false;
                }
            }


            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = self.message;
            m2c_bagUpdate.BagInfoAdd.Clear();
            m2c_bagUpdate.BagInfoUpdate.Clear();
            m2c_bagUpdate.BagInfoDelete.Clear();
            for (int i = rewardItems.Count - 1; i >= 0; i--)
            {
                RewardItem rewardItem = rewardItems[i];
                
                int itemID = rewardItems[i].ItemID;
                int itemtype = rewardItems[i].ItemType;
                if (itemID == 0 || !ItemHelper.IsValidItem(rewardItems[i]))
                {
                    continue;
                }

                int leftNum = rewardItems[i].ItemNum;
                int userDataType = ItemNewHelper.GetItemToUserDataType(rewardItem);
                if (userDataType == UserDataType.PiLao)
                {
                    //Log.Warning($"[增加疲劳] {unit.DomainZone()}  {unit.Id}   {getType}  {rewardItems[i].ItemNum}");
                }
                if (userDataType != UserDataType.None)
                {
                    //检测任务需求道具
                    unit.GetComponent<UserInfoComponent>().UpdateRoleMoneyAdd(userDataType, leftNum.ToString(), true, getType);
                    ItemAddHelper.OnGetItem(unit, getType, rewardItem);
                    continue;
                }


                int maxPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                
                ItemLocType itemLockType = ItemLocType.ItemLocBag;
                List<BagInfo> itemlist = null;
               
                /*if (itemCof.ItemType == ItemTypeEnum.PetHeXin)
                {
                    maxPileSum = itemCof.ItemPileSum;
                    itemLockType = ItemLocType.ItemPetHeXinBag;
                    itemlist = self.GetItemByLoc(itemLockType);
                }*/
                
                itemLockType = UseLocType;
                itemlist = self.GetItemByLoc(itemLockType);
                
                for (int k = 0; k < itemlist.Count; k++)
                {
                    BagInfo userBagInfo = itemlist[k];
                    
                    if (userBagInfo.ItemID != itemID || userBagInfo.ItemType != itemtype)
                    {
                        continue;
                    }
                    if (userBagInfo.ItemNum >= maxPileSum)
                    {
                        continue;
                    }
                    int newNum = leftNum + userBagInfo.ItemNum;
                    if (newNum > maxPileSum)
                    {
                        leftNum = newNum - maxPileSum;
                        newNum = maxPileSum;
                    }
                    else
                    {
                        leftNum = 0;
                    }
                    userBagInfo.ItemNum = newNum;
                    m2c_bagUpdate.BagInfoUpdate.Add(userBagInfo);

                    if (leftNum == 0)
                    {
                        //跳出循环
                        break;
                    }
                }

                //还没有插入完，需要开启新格子
                while (leftNum > 0)
                {
                    BagInfo useBagInfo = new BagInfo();
                    
                    useBagInfo.ItemType = itemtype;
                    useBagInfo.ItemID = itemID;
                    useBagInfo.ItemNum = (leftNum > maxPileSum) ? maxPileSum : leftNum;
                    useBagInfo.Loc = (int)itemLockType;
                    useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
                    useBagInfo.GemHole = ItemHelper.DefaultGem;
                    useBagInfo.GemIDNew = ItemHelper.DefaultGem;
                    useBagInfo.GetWay = getWay;
                    leftNum -= useBagInfo.ItemNum;

                    useBagInfo.HideID = 0;
                    
                    //记录制造的玩家
                    useBagInfo.MakePlayer = makeUserID;

                    
                    if (ItemGetWay.ItemGetBing.Contains(getType))
                    {
                        useBagInfo.isBinging = true;
                    }
                    useBagInfo.isBinging = ItemNewHelper.CheckItemIfLock(rewardItem);
                    
                    
                    ///装备处理
                    if (itemtype == ItemBigType.Type_Equip)
                    {
                        LDEquip equipconfig = LDEquipCategory.Instance.Get(itemID);
                        //蓝色品质的装备需要进行鉴定
                        useBagInfo.IfJianDing = false;

                        //默认洗练
                        if (!ItemNewHelper.IsBuyItem(getType) &&  itemtype == ItemBigType.Type_Equip)
                        {
                            /*int xilianLevel = XiLianHelper.GetXiLianId(unit.GetComponent<NumericComponent>().GetAsInt(NumericType.ItemXiLianDu));
                            xilianLevel = xilianLevel != 0 ? EquipXiLianConfigCategory.Instance.Get(xilianLevel).XiLianLevel : 0;

                            int xilianType = 0;
                            if (getType == ItemGetWay.SkillMake || getType == ItemGetWay.TreasureMap)
                            {
                                xilianType = 2;
                            }
                            
                            ItemXiLianResult itemXiLian = new ItemXiLianResult();
                            if (equipType < 101 || equipType == 301) //装备洗炼
                            {
                                itemXiLian = XiLianHelper.XiLianItem(unit, useBagInfo, xilianType, xilianLevel, 0,0);
                            }
                            else if(equipType == 101)//生肖洗炼
                            {
                                itemXiLian = XiLianHelper.XiLianShengXiao(useBagInfo);
                            }

                            useBagInfo.XiLianHideProLists = itemXiLian.XiLianHideProLists;              //基础属性洗炼
                            useBagInfo.HideSkillLists = itemXiLian.HideSkillLists;                      //隐藏技能
                            useBagInfo.XiLianHideTeShuProLists = itemXiLian.XiLianHideTeShuProLists;    //特殊属性洗炼*/
                        }
                        
                        //掉落的橙色装备默认为绑定的物品
                        if ((getType == ItemGetWay.PickItem || getType == ItemGetWay.ChouKa) && itemtype == ItemBigType.Type_Equip)
                        {
                           
                            if (equipconfig.Quality >= 5)
                            {
                                useBagInfo.isBinging = true;
                            }
                            if (getType == ItemGetWay.System )
                            {
                                useBagInfo.IfJianDing = false;
                            }
                        }
                        
                        
                        //拾取到橙色装备
                        if (equipconfig.Quality >= 5 && getType == ItemGetWay.PickItem)
                        {
                            string name = unit.GetComponent<UserInfoComponent>().UserInfo.Name;
                            string noticeContent = $"恭喜玩家 {name} 获得装备: <color=#{CommonHelper.QualityReturnColor(5)}>{equipconfig.Name}</color>";
                            string noticeContentEn = $"Congratulations to player {name} Get Equip: <color=#{CommonHelper.QualityReturnColor(5)}>{equipconfig.Name}</color>";
                            ServerMessageHelper.SendBroadMessage(self.DomainZone(), NoticeType.Notice, noticeContent, noticeContentEn);
                        }

                        //刷新传承属性
                        if (equipconfig.Quality >= 5 && equipconfig.UseLv >= 60)
                        {
                            int occ = unit.GetComponent<UserInfoComponent>().UserInfo.Occ;
                            int occTwo = unit.GetComponent<UserInfoComponent>().UserInfo.OccTwo;
                            int skillid = XiLianHelper.XiLianChuanChengJianDing(equipconfig, occ, occTwo);
                            if (skillid != 0)
                            {
                                useBagInfo.InheritSkills.Add(skillid);
                            }
                        }

                        
                        // 赛季晶核
                        /*if (equipType == 201)
                        {
                            useBagInfo.ItemPar = ItemHelper.GetJingHeInitQulity(useBagInfo.ItemID).ToString();

                            //增加技能的晶核无须鉴定
                            int jingheSkill = ItemHelper.GetJingHeSkillId(useBagInfo.ItemID);
                            if (jingheSkill > 0)
                            {
                                useBagInfo.IfJianDing = false;
                                useBagInfo.HideSkillLists.Add(jingheSkill); 
                            }
                            else
                            {
                                useBagInfo.IfJianDing = true;
                            }
                        }*/
                    }
                    
                    //道具处理
                    if (itemtype == ItemBigType.Type_Item)
                    {
                        int subType = LDItemCategory.Instance.Get(itemID).ItemType;
                        
                           //藏宝图
                        if (subType == ItemNewSubType.CangBaoTu )
                        {
                            ItemAddHelper.TreasureItem(unit, useBagInfo);
                        }
                        //鉴定符
                        if (subType == 121)
                        {
                            int makePlan = 1;
                            if (getType == ItemGetWay.SkillMake && getWayInfo.Length >= 3)
                            {
                                makePlan = int.Parse(getWayInfo[1]);
                            }
                            if (makePlan != 1 && makePlan != 2)
                            {
                                makePlan = 1;
                            }
                            int shulianduNumeric = makePlan == 1 ? NumericType.MakeShuLianDu_1 : NumericType.MakeShuLianDu_2;
                            int shuliandu = unit.GetComponent<NumericComponent>().GetAsInt(shulianduNumeric);
                            ItemAddHelper.JianDingFuItem(useBagInfo, shuliandu, getType);

                            if (getType == ItemGetWay.GM)
                            {
                                useBagInfo.ItemPar = "100";
                            }
                        }
                        if (getType == ItemGetWay.PetEggPutOut && subType == 102)
                        {
                            if (getWayInfo.Length >= 3)
                            {
                                useBagInfo.FuLing = int.Parse(getWayInfo[2]);
                            }
                        }
                        //食物
                        if (subType == 1 && subType == 131)
                        {
                            useBagInfo.ItemPar = RandomHelper.RandomNumber(1, 100).ToString();
                        }
                        //家园烹饪
                        if (getType == ItemGetWay.JiaYuanCook)
                        {
                            useBagInfo.ItemPar = RandomHelper.RandomNumber(1, 100).ToString();
                        }
                        /*if (subType == 3 && equipType == 401)
                        {
                            useBagInfo.IfJianDing = false;
                            useBagInfo.ItemPar = RandomHelper.RandomNumber(1, 100).ToString();
                        }*/
                        
                            
                 
                        // 振幅卷轴
                        if (subType == ItemTypeEnum.Consume && subType == 17)
                        {
                            // 属性
                            useBagInfo.IncreaseProLists.AddRange(XiLianHelper.GetHidePro(useBagInfo.ItemID));
                            // 技能
                            useBagInfo.IncreaseSkillLists.AddRange(XiLianHelper.GetHideSkill(useBagInfo.ItemID));
                        }

                    }
                    
                    
                    if (getType == ItemGetWay.PaiMaiShop || getType == ItemGetWay.StoreBuy || getType == ItemGetWay.RandomTowerReward || getType == 97)
                    {
                        useBagInfo.isBinging = true;    
                    }

                    self.GetItemByLoc((ItemLocType)useBagInfo.Loc).Add(useBagInfo);
                    m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
                }
                //检测任务需求道具
                ItemAddHelper.OnGetItem(unit, getType, itemtype, itemID, leftNum);
            }

            //通知客户端背包道具发生改变
            if (notice)
            {
                MessageHelper.SendToClient(unit, m2c_bagUpdate);
            }

            return true;
        }

        public static bool CheckNeedItem(this BagComponent self, string rewardItems)
        {
            string[] needList = rewardItems.Split('@');
            for (int i = 0; i < needList.Length; i++)
            {
                string[] itemInfo = needList[i].Split(';');
                if (itemInfo.Length < 2)
                {
                    continue;
                }
                int itemId = int.Parse(itemInfo[0]);
                int itemNum = int.Parse(itemInfo[1]);
                if (self.GetItemNumber(ItemBigType.Type_Item, itemId) < itemNum)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckNeedItem(this BagComponent self, List<RewardItem> rewardItems)
        {
            for (int i = 0; i < rewardItems.Count; i++)
            {
                RewardItem itemInfo = rewardItems[i];
                if (self.GetItemNumber(ItemBigType.Type_Item, itemInfo.ItemID) < itemInfo.ItemNum)
                {
                    return false;
                }
            }
            return true;
        }

        //字符串删除道具
        public static bool OnCostItemData(this BagComponent self, string rewardItems, ItemLocType itemLocType, int itemGetWay)
        {
            List<RewardItem> costItems = new List<RewardItem>();
            string[] needList = rewardItems.Split('@');
            for (int i = 0; i < needList.Length; i++)
            {
                string[] itemInfo = needList[i].Split(';');
                if (itemInfo.Length < 2)
                {
                    continue;
                }
                int itemId = int.Parse(itemInfo[0]);
                int itemNum = int.Parse(itemInfo[1]);
                costItems.Add(new RewardItem() { ItemID = itemId, ItemNum = itemNum });
            }
            return self.OnCostItemData(costItems, itemLocType, itemGetWay);
        }

        //删除背包道具道具[支持同时添加多个]
        public static bool OnCostItemData(this BagComponent self, List<long> costItems, ItemLocType itemLocType = ItemLocType.ItemLocBag)
        {
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemLocType);

            for (int i = 0; i < costItems.Count; i++)
            {
                for (int k = ItemTypeList.Count - 1; k >= 0; k--)
                {
                    if (ItemTypeList[k].BagInfoID == costItems[i])
                    {
                        m2c_bagUpdate.BagInfoDelete.Add(ItemTypeList[k]);
                        ItemTypeList.RemoveAt(k);
                        break;
                    }
                }
            }

            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
            return true;
        }

        //指定某一个格子的ID
        public static bool OnCostItemData(this BagComponent self, long uid, int number)
        {
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            List<BagInfo> ItemTypeList = self.GetItemByLoc(ItemLocType.ItemLocBag);
            for (int k = ItemTypeList.Count - 1; k >= 0; k--)
            {
                if (ItemTypeList[k].BagInfoID == uid)
                {
                    ItemTypeList[k].ItemNum -= number;

                    if (ItemTypeList[k].ItemNum <= 0)
                    {
                        m2c_bagUpdate.BagInfoDelete.Add(ItemTypeList[k]);
                        ItemTypeList.RemoveAt(k);
                    }
                    else
                    {
                        m2c_bagUpdate.BagInfoUpdate.Add(ItemTypeList[k]);
                    }
                    break;
                }
            }
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
            return true;
        }

        //删除背包道具道具[支持同时添加多个]
        public static bool OnCostItemData(this BagComponent self, List<RewardItem> costItems, ItemLocType itemLocType, int itemGetWay)
        {
            for (int i = costItems.Count - 1; i >= 0; i--)
            {
                int itemID = costItems[i].ItemID;
                int itemNum = costItems[i].ItemNum;

                //获取背包内的道具是否足够
                if (self.GetItemNumber(ItemBigType.Type_Item, itemID, itemLocType) < itemNum)
                {
                    return false;
                }
            }

            //通知客户端背包刷新
            Unit unit = self.GetParent<Unit>();
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd = new List<BagInfo>();

            for (int i = costItems.Count - 1; i >= 0; i--)
            {
                int itemID = costItems[i].ItemID;
                int itemNum = costItems[i].ItemNum;

                //扣除金币
                if (itemID == (int)UserDataType.Gold)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleMoneySub(UserDataType.Gold, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
                if (itemID == (int)UserDataType.WeiJingGold)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleMoneySub(UserDataType.WeiJingGold, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
                if (itemID == (int)UserDataType.Diamond)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleMoneySub(UserDataType.Diamond, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
                if (itemID == (int)UserDataType.V1TotalPoints)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleMoneySub(UserDataType.V1TotalPoints, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
                if (itemID == (int)UserDataType.RongYu)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.RongYu, itemNum.ToString());
                    continue;
                }
                if (itemID == (int)UserDataType.JiaYuanFund)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.JiaYuanFund, itemNum.ToString());
                    continue;
                }
                if (itemID == (int)UserDataType.SeasonCoin)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.SeasonCoin, itemNum.ToString());
                    continue;
                }
                if (itemID == (int)UserDataType.UnionContri)
                {
                    itemNum = -1 * itemNum;
                    unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.UnionContri, itemNum.ToString());
                    continue;
                }
                
                LogHelper.LogWarning($"消耗道具: {unit.Id} {itemID} {itemNum}", false);
                List<BagInfo> bagInfos = self.GetItemByLoc(itemLocType);
                for (int k = bagInfos.Count - 1; k >= 0; k--)
                {
                    BagInfo userBagInfo = bagInfos[k];
                    if (userBagInfo.ItemID == itemID)
                    {
                        if (userBagInfo.ItemNum >= itemNum)
                        {
                            //满足扣除数
                            int costNum = itemNum;
                            itemNum -= userBagInfo.ItemNum;
                            userBagInfo.ItemNum -= costNum;
                            if (userBagInfo.ItemNum <= 0)
                            {
                                m2c_bagUpdate.BagInfoDelete.Add(userBagInfo);
                                bagInfos.RemoveAt(k);
                            }
                            else
                            {
                                m2c_bagUpdate.BagInfoUpdate.Add(userBagInfo);
                            }
                        }
                        else
                        {
                            itemNum -= userBagInfo.ItemNum;
                            //完全删除道具
                            userBagInfo.ItemNum = 0;
                            m2c_bagUpdate.BagInfoDelete.Add(userBagInfo);
                            bagInfos.RemoveAt(k);
                        }

                        //扣除完道具直接跳出当前循环
                        if (itemNum <= 0)
                        {
                            break;
                        }
                    }
                }
                ItemAddHelper.OnCostItem(unit, itemID);
            }

            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            return true;
        }

        public static int GetQiangHuaLevel(this BagComponent self, int subType)
        {
            return 0;
        }

        public static void OnEquipFuMo(this BagComponent self, int itemid, List<HideProList> hideProLists, int index)
        {
            LDItem ldItem = LDItemCategory.Instance.Get(itemid);
            string[] itemparams = ldItem.ItemUsePar.Split('@');
            int weizhi = int.Parse(itemparams[0]);
            List<BagInfo> bagInfos = self.GetEquipListByWeizhi(ItemLocType.ItemLocEquip, weizhi);
            if (bagInfos.Count <= index)
            {
                return;
            }
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfos[index]);

            //9@200103; 0.03; 0.03
            bagInfos[index].FumoProLists.Clear();
            bagInfos[index].FumoProLists.AddRange(hideProLists);
            //bagInfos[index].FumoProLists.AddRange( ItemHelper.GetItemFumoPro(itemid) );

            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
            Function_Fight.UnitUpdateProperty_Base(self.GetParent<Unit>(), true, true);
        }



        public static void OnGmGaoJi(this BagComponent self, int level)
        {
          
        
        }

        public static void GetEquipAttribute(this BagComponent self, List<HideProList> occInitAttribute)
        {
            Unit unit = self.GetParent<Unit>();
            int occ = unit.GetComponent<UserInfoComponent>().UserInfo.Occ;
            List<int> equipIDList = new List<int>();
            List<int> equipSuitIDList = new List<int>();
            List<BagInfo> equipList =  self.GetItemByLoc(ItemLocType.ItemLocEquip);
            //List<BagInfo> equipList_2 = unit.GetComponent<BagComponent>().GetItemByLoc(ItemLocType.ItemLocEquip_2);
            
           // List<BagInfo> equipList = new List<BagInfo>();
           // equipList.AddRange(equipList_1);
            //equipList.AddRange(equipList_2);

            for (int i = equipList.Count - 1; i >= 0; i--)
            {
                BagInfo userBagInfo = equipList[i];
                if (!LDEquipCategory.Instance.Contain(userBagInfo.ItemID))
                {
                    equipList.RemoveAt(i);
                    continue;
                }

                List<HideProList> hideProLists = LDEquipCategory.Instance.GetEquipAttribute(userBagInfo.ItemID);
                occInitAttribute.AddRange(hideProLists);
            }

            for (int i = equipList.Count - 1; i >= 0; i--)
            {
                BagInfo userBagInfo = equipList[i];
                if (!LDEquipCategory.Instance.Contain(userBagInfo.ItemID))
                {
                    equipList.RemoveAt(i);
                    continue;
                }

                //存储装备ID
                LDEquip itemCof = LDEquipCategory.Instance.Get(userBagInfo.ItemID);
                int equipType = ItemHelper.GetNewEquipType(userBagInfo);                
                //生肖装备没激活直接跳出来
                if (equipType == 101 && ItemHelper.IfShengXiaoActive(itemCof.Id, equipList) == false)
                {
                    continue;
                }

                //赛季晶核装备
                if (equipType == 201)
                {

                }

                bool ifAddHidePro = true;
               
                if (ifAddHidePro)
                {
                    //存储装备精炼数值
                    if (userBagInfo.HideProLists.Count > 0)
                    {
                        occInitAttribute.AddRange(userBagInfo.HideProLists);
                    }
                }

                //存储洗炼数值
                if (userBagInfo.XiLianHideProLists.Count > 0)
                {
                    occInitAttribute.AddRange(userBagInfo.XiLianHideProLists);
                }

                //存储洗炼数值
                if (userBagInfo.XiLianHideTeShuProLists.Count > 0)
                {
                    occInitAttribute.AddRange(userBagInfo.XiLianHideTeShuProLists);
                }

                //存储附魔属性
                if (userBagInfo.FumoProLists .Count > 0)
                {
                    occInitAttribute.AddRange(userBagInfo.FumoProLists); 
                }

                // 存储增幅属性
                if (userBagInfo.IncreaseProLists != null && userBagInfo.IncreaseProLists.Count > 0)
                {
                    occInitAttribute.AddRange(userBagInfo.IncreaseProLists); 
                 
                }
                //.InheritSkills //传承技能
                // 存储增幅技能属性
                if (userBagInfo.IncreaseSkillLists != null && userBagInfo.IncreaseSkillLists.Count > 0)
                {
                    for (int s = 0; s < userBagInfo.IncreaseSkillLists.Count; s++)
                    {
                        HideProListConfig hideProListConfig = HideProListConfigCategory.Instance.Get(userBagInfo.IncreaseSkillLists[s]);
                        LDSkill ldSkill = LDSkillCategory.Instance.Get(hideProListConfig.PropertyType);
                    }
                }

                //存储装备ID
                equipIDList.Add(itemCof.Id);

                //存储装备套装
                if (LDEquipCategory.Instance.Contain(itemCof.Id))
                {
                    LDEquip ldEquipCnf = LDEquipCategory.Instance.Get(itemCof.Id);
                    if (ldEquipCnf.EquipSuitID != 0)
                    {
                        if (equipSuitIDList.Contains(ldEquipCnf.EquipSuitID) == false)
                        {
                            equipSuitIDList.Add(ldEquipCnf.EquipSuitID);
                        }
                    }
                }
                else
                {
                    //Log.Debug($"无效的装备: {itemCof.Id}");
                }
            }
            
            
               ///职业套装
            List<int> occsuit = new List<int>();
            /*quipSuitConfigCategory.Instance.OccSuiList.TryGetValue(userInfo.Occ, out occsuit);
            if(occsuit!=null)
            {
                equipSuitIDList.AddRange(occsuit);
            }*/
            
            //装备套装属性
            for (int i = 0; i < equipSuitIDList.Count; i++)
            {
                if (!LDEquip_SuitCategory.Instance.Contain(equipSuitIDList[i]))
                {
                    continue;
                }
                LDEquip_Suit ldEquipSuitCof = LDEquip_SuitCategory.Instance.Get(equipSuitIDList[i]);
                int num = 0;
                /*if (ldEquipSuitCof.SuitType == 0) //默认套装
                {
                    
                }
                else  //时装套装
                {
                    int[] needEquipList = ldEquipSuitCof.NeedEquipID;
                    for (int y = 0; y < needEquipList.Length; y++)
                    {
                        if (self.FashionActiveIds.Contains(needEquipList[y]))
                        {
                            num++;
                        }
                    }
                }*/
                int[] needEquipList = ldEquipSuitCof.Equip_Id;
                for (int y = 0; y < needEquipList.Length; y++)
                {
                    int needEquipID = needEquipList[y];
                    if (equipIDList.Contains(needEquipID))
                    {
                        num = num + 1;
                    }
                }

                string[] equipSuitProList = ldEquipSuitCof.Property.Split('|');
                for (int y = 0; y < equipSuitProList.Length; y++)
                {
                    int NeedNum = int.Parse(equipSuitProList[y].Split('_')[0]);
                    int NeedID = int.Parse(equipSuitProList[y].Split('_')[1]);
                    if (num >= NeedNum)
                    {
                        //激活对应套装属性
                        LDEquip_Suit_Property ldEquipSuitProCof = LDEquip_Suit_PropertyCategory.Instance.Get(NeedID);
                        // return list<hirepro>
                        //BaseDamgeSubAdd_EquipSuit += ldEquipSuitProCof.Equip_Hp;

                        /*
                        if (ldEquipSuitProCof.AddPropreListStr != "0")
                        {
                            string[] AddPropreList = ldEquipSuitProCof.AddPropreListStr.Split(';');
                            for (int z = 0; z < AddPropreList.Length; z++)
                            {
                                int addProType = int.Parse(AddPropreList[z].Split(',')[0]);
                                int type = NumericHelp.GetNumericValueType(addProType);
                                int addProValue = 0;
                                if (type == 1)
                                {
                                    addProValue = int.Parse(AddPropreList[z].Split(',')[1]);
                                }
                                else
                                {
                                    addProValue = (int)(float.Parse(AddPropreList[z].Split(',')[1]) * 10000);
                                }

                                AddUpdateProDicList(addProType, addProValue, UpdateProDicList);
                            }
                        }*/
                    }
                }
            }
            
            for (int i = 0; i < equipList.Count; i++)
            {
                LDEquip mLdEquipCon = LDEquipCategory.Instance.Get(equipList[i].ItemID);
                int equipType = ItemHelper.GetNewEquipType(equipList[i]);

                //生肖装备没激活直接跳出来
             
                if (equipType > 0 && equipList[i].ItemID == 0)
                {
                    /*List<int> itemSkills = ItemHelper.GetItemSkill(itemCof.SkillID);
                    foreach (int skillid in itemSkills)
                    {
                        SkillConfig skillConfig = SkillConfigCategory.Instance.Get(skillid);
                        skillAddCombat += skillConfig.AddCombat;
                    }*/
                    continue;
                }
                
                //职业专精
                if (occ != 0)
                {
                    //if (Occupation_TransferCategory.Instance.Get(userInfo.OccTwo).ArmorMastery == ItemCategory.Instance.Get(equipIDList[i]).EquipType)
                    //{
                    //    //occMastery = 0.2f;
                    //    occMastery = 0f;
                    //}
                }

                //极品属性
                //强化登录（List长度13， 13个位置）
                int caowei = ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID);
                int qianghuaLv = unit.GetComponent<BagComponent>().GetQiangHuaLevel(caowei);

                EquipQiangHuaConfig equipQiangHuaConfig = QiangHuaHelper.GetQiangHuaConfig(caowei, qianghuaLv);
                if (equipQiangHuaConfig != null)
                {
                    
                    //addPro += float.Parse(EquipQiangHuaConfigCategory.Instance.Get(QiangHuaHelper.GetQiangHuaId(itemCof.ItemSubType, qianghuaLv[itemCof.ItemSubType])).EquipPropreAdd);
                }

                occInitAttribute.AddRange( LDEquipCategory.Instance.GetEquipAttribute(equipList[i].ItemID) );

                //获取宝石属性
                if (string.IsNullOrEmpty(equipList[i].GemIDNew))
                {
                    equipList[i].GemIDNew = ItemHelper.DefaultGem;
                    //Log.Debug($"GemIDNew==null  unit.Id: {unit.Id} BagInfoID:{equipList[i].BagInfoID}");
                }

                string[] gemList = equipList[i].GemIDNew.Split('_');

                for (int z = 0; z < gemList.Length; z++)
                {

                    int gemID = int.Parse(gemList[z]);
                    if (gemID == 0)
                    {
                        continue;
                    }

                    //史诗宝石数量最多4个
                 

                    // "100403;10@100203;60
                    LDItem gemitemCof = LDItemCategory.Instance.Get(gemID);
                    string[] attributeList = gemitemCof.ItemUsePar.Split('@');
                    for (int a = 0; a < attributeList.Length; a++)
                    {
                        //100203;113
                        string attributeItem = attributeList[a];
                        string[] attributeInfo = attributeItem.Split(';');
                        int gemPro = 0;
                        try
                        {
                            gemPro = int.Parse(attributeInfo[0]);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("attri: " + ex.ToString());
                            continue;
                        }

                        long gemValue = long.Parse(attributeInfo[1]);

                        //浮点数处理
                        if (NumericHelp.GetNumericValueType(gemPro) == 2)
                        {
                            //gemValue = gemValue * 10000;
                        }

                        //宝石专精
                        if (equipList[i].HideSkillLists.Contains(68000108) || equipList[i].IncreaseSkillLists.Contains(2108) || equipList[i].IncreaseSkillLists.Contains(3902))
                        {
                            gemValue = (long)((float)gemValue * 1.2f);
                        }

                        //AddUpdateProDicList(gemPro, gemValue, UpdateProDicList);
                    }
                }
            }
        }

        public static void GetSeasonHexinAttribute(this BagComponent self)
        {
            
            ///晶核列表
            /*
            List<BagInfo> jingHeList = unit.GetComponent<BagComponent>().GetCurJingHeList();

            for (int i = 0; i < jingHeList.Count; i++)
            {
                //存储装备精炼数值
                if (jingHeList[i].XiLianHideProLists != null)
                {
                    for (int y = 0; y < jingHeList[i].XiLianHideProLists.Count; y++)
                    {
                        HideProList hidePro = jingHeList[i].XiLianHideProLists[y];
                        AddUpdateProDicList(hidePro.HideID, hidePro.HideValue, UpdateProDicList);
                    }
                }
            }
            */
            
            /*SeasonLevelConfig seasonLevelConfig = SeasonLevelConfigCategory.Instance.Get(userInfo.SeasonLevel);
            if (!CommonHelper.IfNull(seasonLevelConfig.PripertySet))
            {
                string[] addProList = seasonLevelConfig.PripertySet.Split("@");
                for (int p = 0; p < addProList.Length; p++)
                {
                    string[] addPro = addProList[p].Split(";");
                    if (addPro.Length < 2)
                    {
                        break;
                    }
                    int key = int.Parse(addPro[0]);
                    try
                    {
                        if (NumericHelp.GetNumericValueType(key) == 1)
                        {
                            AddUpdateProDicList(key, long.Parse(addPro[1]), UpdateProDicList);
                        }
                        else
                        {
                            AddUpdateProDicList(key, (int)(float.Parse(addPro[1]) * 10000), UpdateProDicList);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"赛季属性配置错误：{ex.ToString()} {seasonLevelConfig.PripertySet}");
                    }
                }
            }*/
        }

        public static bool OnCostItemData(this BagComponent self, BagInfo bagInfo, ItemLocType locType, int number)
        {
            List<BagInfo> bagInfos = self.GetItemByLoc(locType);

            if (bagInfo.ItemNum >= number)
            {
                bagInfo.ItemNum -= number;

                if (bagInfo.ItemNum <= 0)
                {
                    bagInfos.Remove(bagInfo);
                }
                LogHelper.LogWarning($"消耗道具: {self.GetParent<Unit>().Id} {bagInfo.ItemID} {number}", false);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}