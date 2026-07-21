using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class BagComponentAwakeSystem : AwakeSystem<BagComponentServer>
    {

        public override void Awake(BagComponentServer self)
        {
                
        }
    }

    public static class BagComponentServerSystem
    {


        public static void OnInit(this BagComponentServer self,  CreateRoleInfo createRoleInfo)
        {
            LDOccupation ldOccupation = LDOccupationCategory.Instance.Get(createRoleInfo.PlayerOcc);
            int[] equipIinit = ldOccupation.Equip_Init;

            List<RewardItem> rewardItems = new List<RewardItem>();
            for (int i = 0; i <equipIinit.Length; i++)
            {
                rewardItems.Add(new RewardItem()
                {
                    ItemType = ItemBigType.Type_Equip,
                    ItemID = equipIinit[i],
                    ItemNum = 1
                });
            }

            self.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", false);
            
            List<BagInfo> equipList = new List<BagInfo>();
            equipList.AddRange( self.BagItemList);
            self.BagItemList.Clear();
            
            for (int i = 0; i <equipList.Count; i++)
            {
                LDEquip ldEquip = LDEquipCategory.Instance.Get(equipList[i].ItemID);
                Log.Debug($"槽位： {ldEquip.Sub_Type} {ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID)}");
                
                equipList[i].Loc = (int)ItemLocType.ItemLocEquip;
                self.EquipList.Add(equipList[i]);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool HaveOccEquip(this BagComponentServer self)
        {
            for (int i = 0; i < self.EquipList.Count; i++)
            {
                BagInfo equip = self.EquipList[i];
                LDItem ldItem = LDItemCategory.Instance.Get(equip.ItemID);
                int equipType = ItemNewHelper.GetNewEquipType(equip);
                if (ldItem.ItemType == 3
                    && equipType >= 0 && equipType <= 100
                    && ldItem.ItemType >= 0 && ldItem.ItemType <= 12)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<AttributeItem> GetGemProLists(this BagComponentServer self)
        {
            List<AttributeItem> list = new List<AttributeItem>();
            for (int i = 0; i < self.GemList.Count; i++)
            {
                string itemUsePar = LDItemCategory.Instance.Get(self.GemList[i].ItemID).ItemUsePar;
                if (string.IsNullOrEmpty(itemUsePar) || itemUsePar == "0")
                {
                    continue;
                }
                string[] attributes = itemUsePar.Split('@');
                for (int a = 0; a < attributes.Length; a++)
                {
                    int sep = attributes[a].IndexOf(';');
                    if (sep <= 0 || sep >= attributes[a].Length - 1)
                    {
                        continue;
                    }
                    int hideId = int.Parse(attributes[a].Substring(0, sep));
                    string valueStr = attributes[a].Substring(sep + 1);
                    long hide_value = 0;
                    if (NumericHelp.GetNumericValueType(hideId) == 2)
                    {
                        hide_value = NumericHelp.ParseConfigToStored(hideId, valueStr);
                    }
                    else
                    {
                        hide_value = long.Parse(valueStr);
                    }
                    list.Add(new AttributeItem() { AttributeID = hideId, AttributeValue = hide_value });
                }
            }

            return list;
        }
        
        public static List<BagInfo> GetItemByLoc(this BagComponentServer self, ItemLocType itemEquipType)
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
               
            }
            return ItemTypeList;
        }

        public static void ZhengLiItemList(this BagComponentServer self, Dictionary<int, List<BagInfo>> ItemSameList, M2C_RoleBagUpdate m2c_bagUpdate)
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


        public static void OnRecvItemSort(this BagComponentServer self, ItemLocType itemEquipType)
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
                    if (!ItemSameList_1.TryGetValue(bagInfo.ItemID, out List<BagInfo> sameList_1))
                    {
                        sameList_1 = new List<BagInfo>();
                        ItemSameList_1[bagInfo.ItemID] = sameList_1;
                    }
                    sameList_1.Add(bagInfo);
                }
                else
                {
                    if (!ItemSameList_2.TryGetValue(bagInfo.ItemID, out List<BagInfo> sameList_2))
                    {
                        sameList_2 = new List<BagInfo>();
                        ItemSameList_2[bagInfo.ItemID] = sameList_2;
                    }
                    sameList_2.Add(bagInfo);
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

            ItemNewHelper.ItemLitSort(ItemTypeList);
        }

        public static void CheckValiedItem(this BagComponentServer self, List<BagInfo> bagInfos, int occ, int occTwo)
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

        public static void CheckAllItem(this BagComponentServer self, int occ, int occTwo)
        {
            self.CheckValiedItem(self.GemList, occ, occTwo);
            self.CheckValiedItem(self.BagItemList, occ, occTwo);
            self.CheckValiedItem(self.EquipList, occ, occTwo);
            self.CheckValiedItem(self.BagItemPetHeXin, occ, occTwo);
            self.CheckValiedItem(self.PetHeXinList, occ, occTwo);
            self.CheckValiedItem(self.Warehouse1, occ, occTwo);
            self.CheckValiedItem(self.Warehouse2, occ, occTwo);
            self.CheckValiedItem(self.Warehouse3, occ, occTwo);
            self.CheckValiedItem(self.Warehouse4, occ, occTwo);
        }

        //获取自身所有的道具
        public static List<BagInfo> GetAllItems(this BagComponentServer self, int occ, int occTwo)
        {
            List<BagInfo> bagList = new List<BagInfo>();

            self.CheckAllItem(occ, occTwo);

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

            return bagList;
        }

        public static List<BagInfo> GetIdItemListByLoc(this BagComponentServer self, int itemId, ItemLocType loc)
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

        public static List<BagInfo> GetIdItemList(this BagComponentServer self, int itemId)
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

        public static int GetNeedCell(this BagComponentServer self, List<RewardItem> itemids, ItemLocType itemLocType)
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
        public static long GetItemNumber(this BagComponentServer self, int itemType,  int itemId, ItemLocType itemLocType = ItemLocType.ItemLocBag)
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
                case UserDataType.Diamond:
                case UserDataType.JiaYuanFund:
                case UserDataType.UnionContri:
                {
                    RoleInfoComponentServer roleInfo = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>();
                    switch (userDataType)
                    {
                        case UserDataType.Gold:
                            number = roleInfo.RoleInfo.Gold;
                            break;
                        case UserDataType.Diamond:
                            number = roleInfo.RoleInfo.Diamond;
                            break;
                        case UserDataType.JiaYuanFund:
                            number = roleInfo.RoleInfo.JiaYuanFund;
                            break;
                        case UserDataType.UnionContri:
                            number = roleInfo.RoleInfo.UnionZiJin;
                            break;
                    }
                    break;
                }
                default:
                    break;
            }
            return number;
        }


        //根据ID获取对应的背包数据
        public static BagInfo GetItemByLoc(this BagComponentServer self, ItemLocType itemLocType, long bagId)
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

        public static bool IsBagFull(this BagComponentServer self)
        {
            return self.GetBagLeftCell() <= 0;
        }

        public static int GetBagLeftCell(this BagComponentServer self)
        {
            return self.GetBagTotalCell() - self.BagItemList.Count;
        }

        public static int GetBagTotalCell(this BagComponentServer self)
        {
            if (self.WarehouseAddedCell.Count == 0 || self.AdditionalCellNum.Count == 0)
            {
                return LDGlobalValueCategory.Instance.BagInitCapacity;
            }
            return self.WarehouseAddedCell[0] + self.AdditionalCellNum[0] + + LDGlobalValueCategory.Instance.BagInitCapacity;
        }

        public static bool IsHourseFullByLoc(this BagComponentServer self, int hourseId)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return ItemTypeList.Count >= self.GetHourseTotalCell(hourseId);
        }

        public static int GetHourseLeftCell(this BagComponentServer self, int hourseId)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return self.GetHourseTotalCell(hourseId) - ItemTypeList.Count;
        }

        public static int GetHourseTotalCell(this BagComponentServer self, int hourseId)
        {
            int storeCapacity = LDGlobalValueCategory.Instance.HourseInitCapacity;
            
            return storeCapacity + self.WarehouseAddedCell[hourseId] + self.AdditionalCellNum[hourseId];
        }

        /// <summary>
        /// 获取抽卡仓库剩余的格数，上限100
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetChouKaLeftSpace(this BagComponentServer self)
        {
            return 100 - self.ChouKaWarehouse.Count;
        }

        public static void OnChangeItemLoc(this BagComponentServer self, BagInfo bagInfo, ItemLocType itemLocTypeTo, ItemLocType itemLocTypeFrom)
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
        public static bool IsHaveEquipSkill(this BagComponentServer self, int skillId, long xilianequip)
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
            }
            return false;
        }

        public static void OnResetSeason(this BagComponentServer self, bool notice)
        { 
            self.ClearJingHeItem(self.BagItemList);
            self.ClearJingHeItem(self.Warehouse1);
            self.ClearJingHeItem(self.Warehouse2);
            self.ClearJingHeItem(self.Warehouse3);
            self.ClearJingHeItem(self.Warehouse4);
        }

        public static void ClearJingHeItem(this BagComponentServer self, List<BagInfo> bagInfos)
        {
            for (int i = bagInfos.Count - 1; i >= 0; i--)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(bagInfos[i].ItemID);
                int equipType = ItemNewHelper.GetNewEquipType(bagInfos[i]);
                if (equipType == 201)
                {
                    bagInfos.RemoveAt(i);
                }
            }
        }

        public static List<BagInfo> GetCurJingHeList(this BagComponentServer self)
        {
            List<BagInfo> bagInfos = new List<BagInfo>();

            return bagInfos;
        }

        public static bool IsEquipJingHe(this BagComponentServer self, int itemId)
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

        public static List<int> GetEquipTianFuIds(this BagComponentServer self)
        {
            List<int> equiptianfuids = new List<int>(); 
            List<BagInfo> equiplist = new List<BagInfo>();
            equiplist.AddRange(self.EquipList );

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

        public static BagInfo GetJingHeByWeiZhi(this BagComponentServer self, int subType)
        {
            List<BagInfo> bagInfos = self.GetCurJingHeList();
            for (int i = 0; i < bagInfos.Count; i++)
            {
               
            }
            return null;
        }

        public static List<BagInfo> GetEquipListByWeizhi(this BagComponentServer self, ItemLocType equipIndex, int position)
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

        public static int GetMaxQiangHuaLevel(this BagComponentServer self)
        {
            int maxLevel = 0;
            return maxLevel;
        }

        //获取某个装备位置的道具数据
        public static BagInfo GetEquipBySubType(this BagComponentServer self, ItemLocType equipIndex, int subType)
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
        public static BagInfo GetMagicEquipBySubType(this BagComponentServer self, ItemLocType equipIndex, int position)
        {
            List<BagInfo> equipList = self.GetItemByLoc(equipIndex);
            for (int i = 0; i < equipList.Count; i++)
            {
                LDItem ldItemCof = LDItemCategory.Instance.Get(equipList[i].ItemID);
               
            }
            return null;
        }

        public static void OnLogin(this BagComponentServer self, int robotId, int occ, int occTwo)
        {

            Unit unit = self.GetParent<Unit>();
           
            self.CheckAllItem(occ, occTwo);

            ///old
            //int warehourseNumber = (int)ItemLocType.ItemLocMax - 5;
            //if (self.WarehouseAddedCell.Count < warehourseNumber)  // 11)
            //{
            //    for (int i = self.WarehouseAddedCell.Count; i < warehourseNumber; i++)
            //    {
            //        self.WarehouseAddedCell.Add(0);
            //    }
            //}


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
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                LDRobot ldRobot = LDRobotCategory.Instance.Get(robotId);

                if (ldRobot.Behaviour != 1 && ldRobot.Level > roleInfoComponentServer.RoleInfo.Lv)
                {
                    roleInfoComponentServer.RoleInfo.Lv = ldRobot.Level;
                }
                if (ldRobot.EquipList != null)
                {
                    equipList = ldRobot.EquipList != null ? ldRobot.EquipList : equipList;
                }
                else
                {
                    equipList = LDItemCategory.Instance.GetRandomEquipList(roleInfoComponentServer.RoleInfo.Occ, roleInfoComponentServer.RoleInfo.Lv);
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
                    List<BagInfo> existingItems = self.GetIdItemList(equipList[i]);
                    if (existingItems.Count > 0)
                    {
                        continue;
                    }

                    self.OnAddItemData($"{equipList[i]};1", $"{ItemGetWay.System}_0", false);
                    existingItems = self.GetIdItemList(equipList[i]);
                    if (existingItems.Count == 0)
                    {
                        Log.Warning("机器人装备 bagInfo.Count == 0");
                        continue;
                    }

                    self.OnChangeItemLoc(existingItems[0], ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                }
            }
        }

        public static int GetZodiacnumber(this BagComponentServer self)
        {
            int number = 0;
            for (int i = 0; i < self.EquipList.Count; i++)
            {
               
            }

            return number;
        }

        public static int GetWuqiItemId(this BagComponentServer self)
        {
            BagInfo bagInfo = self.GetEquipBySubType(ItemLocType.ItemLocEquip, (int)EquipCaoWeiTypeEnum.Wuqi_1);
            return bagInfo != null ? bagInfo.ItemID : 0;
        }

        public static void OnAddJianDing(this BagComponentServer self)
        {
            self.OnAddItemData( $"11200001;1@11200002;1@11200003;1", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}" );
        }

        //字符串添加道具 
        public static bool OnAddItemData(this BagComponentServer self, string rewardItems, string getType, bool notice = true)
        {
            List<RewardItem> costItems = ItemNewHelper.GetRewardItems(rewardItems);
            return self.OnAddItemData(costItems, string.Empty, getType, notice);
        }

        public static bool OnAddItemData(this BagComponentServer self, string rewardItems, string getType, bool notice, ItemLocType useLocType)
        {
            List<RewardItem> costItems = ItemNewHelper.GetRewardItems(rewardItems);
            return self.OnAddItemData(costItems, string.Empty, getType, notice, false, useLocType);
        }

        public static void OnAddItemData(this BagComponentServer self, List<BagInfo> bagInfos, string getType)
        {
            for (int i = 0; i < bagInfos.Count; i++)
            {
                self.OnAddItemData(bagInfos[i], getType);
            }
        }

        public static bool OnAddItemData(this BagComponentServer self, BagInfo bagInfo, string getType)
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
                string[] getWayParts = getType.Split('_');
                int getTypeValue = int.Parse(getWayParts[0]);
                ItemAddHelper.OnGetItem(self.GetParent<Unit>(), getTypeValue, bagInfo);
                return true;
            }
        }

        public static void OnAddItemToStore(this BagComponentServer self, int itemlockType, int itemid, int itemnumber, string getType)
        {
            BagInfo useBagInfo = new BagInfo();
            useBagInfo.ItemID = itemid;
            useBagInfo.ItemNum = itemnumber;
            useBagInfo.Loc = itemlockType;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GetWay = getType;
            self.GetItemByLoc((ItemLocType)useBagInfo.Loc).Add(useBagInfo);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
        }

        public static void OnAddItemDataNewCell(this BagComponentServer self, BagInfo bagInfo, int itemnumber)
        {
            int itemid = bagInfo.ItemID;
            BagInfo useBagInfo = new BagInfo();
            useBagInfo.ItemID = itemid;
            useBagInfo.ItemNum = itemnumber;
            LDItem ldItemCof = LDItemCategory.Instance.Get(itemid);
            useBagInfo.Loc = ldItemCof.ItemType == (int)ItemTypeEnum.PetHeXin ? (int)ItemLocType.ItemPetHeXinBag : (int)ItemLocType.ItemLocBag;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GetWay = bagInfo.GetWay;
            useBagInfo.isBinging = bagInfo.isBinging;
            self.GetItemByLoc((ItemLocType)useBagInfo.Loc).Add(useBagInfo);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
        }

        //添加背包道具道具[支持同时添加多个]
        public static bool OnAddItemData(this BagComponentServer self, List<RewardItem> rewardItems_init, string makeUserID, string getWay, bool notice = true, bool gm = false, ItemLocType UseLocType = ItemLocType.ItemLocBag)
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
            Dictionary<long, RewardItem> rewardItemMap = new Dictionary<long, RewardItem>();
            for (int i = 0; i < rewardItems_init.Count; i++)
            {
                long key = ((long)rewardItems_init[i].ItemType << 32) | (uint)rewardItems_init[i].ItemID;
                if (rewardItemMap.TryGetValue(key, out RewardItem merged))
                {
                    merged.ItemNum += rewardItems_init[i].ItemNum;
                }
                else
                {
                    rewardItemMap[key] = new RewardItem()
                    {
                        ItemType = rewardItems_init[i].ItemType,
                        ItemID = rewardItems_init[i].ItemID,
                        ItemNum = rewardItems_init[i].ItemNum
                    };
                }
            }
            rewardItems.AddRange(rewardItemMap.Values);

            Dictionary<long, long> pileSumCache = new Dictionary<long, long>();
            for (int i = rewardItems.Count - 1; i >= 0; i--)
            {
                RewardItem rewardItem = rewardItems[i];

                //特殊类型不进背包
                if (rewardItem.ItemType == ItemBigType.Type_Money)
                {
                    rewardItems.RemoveAt(i);
                    continue;
                }
                if (rewardItem.ItemType != ItemBigType.Type_Item
                    && rewardItem.ItemType != ItemBigType.Type_Equip)
                {
                    Console.WriteLine($"{rewardItem.ItemType} 类型未处理"); 
                    continue;
                }

                if (!ItemNewHelper.CheckValiedItem(rewardItem))
                {
                    rewardItems.RemoveAt(i);
                    continue;
                }

                long itemKey = ((long)rewardItem.ItemType << 32) | (uint)rewardItem.ItemID;
                if (!pileSumCache.TryGetValue(itemKey, out long ItemPileSum))
                {
                    ItemPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                    pileSumCache[itemKey] = ItemPileSum;
                }
                if (UseLocType >= ItemLocType.ItemWareHouse1)
                {
                    continue;
                }


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

            if (bagCellNumber > self.GetBagLeftCell() && UseLocType == ItemLocType.ItemLocBag)
            {
                return false;
            }

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = self.message;
            m2c_bagUpdate.BagInfoAdd.Clear();
            m2c_bagUpdate.BagInfoUpdate.Clear();
            m2c_bagUpdate.BagInfoDelete.Clear();
            for (int i = rewardItems.Count - 1; i >= 0; i--)
            {
                RewardItem rewardItem = rewardItems[i];
                
                int itemID = rewardItem.ItemID;
                int itemtype = rewardItem.ItemType;
                if (itemID == 0 || !ItemNewHelper.IsValidItem(rewardItem))
                {
                    continue;
                }

                int leftNum = rewardItem.ItemNum;
                int userDataType = ItemNewHelper.GetItemToUserDataType(rewardItem);
                if (userDataType == UserDataType.PiLao)
                {
                    //Log.Warning($"[增加疲劳] {unit.DomainZone()}  {unit.Id}   {getType}  {rewardItems[i].ItemNum}");
                }
                if (userDataType != UserDataType.None)
                {
                    //检测任务需求道具
                    unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(userDataType, leftNum.ToString(), true, getType);
                    ItemAddHelper.OnGetItem(unit, getType, rewardItem);
                    continue;
                }


                long itemKey = ((long)rewardItem.ItemType << 32) | (uint)rewardItem.ItemID;
                if (!pileSumCache.TryGetValue(itemKey, out long cachedPileSum))
                {
                    cachedPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                    pileSumCache[itemKey] = cachedPileSum;
                }
                int maxPileSum = (int)cachedPileSum;
                
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

                        if (useBagInfo.BaseAttrList.Count <= 0)
                        {
                            useBagInfo.EnhanceLevel = RandomHelper.RandomNumber(0, LDEquipCategory.Instance.Get(itemID).Enhance);
                            useBagInfo.BaseAttrList = LDEquipCategory.Instance.GetEquipAttribute(itemID);
                        }
                    }
                    
                    //道具处理
                    if (itemtype == ItemBigType.Type_Item)
                    {
                        int subType = LDItemCategory.Instance.Get(itemID).ItemType;
                        
                           //藏宝图
                        //if (subType == ItemSubTypeEnum.CangBaoTu )
                        //{
                        //    ItemAddHelper.TreasureItem(unit, useBagInfo);
                        //}
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
                                //useBagInfo.FuLing = int.Parse(getWayInfo[2]);
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

        public static bool CheckNeedItem(this BagComponentServer self, string rewardItems)
        {
            List<RewardItem> needItems = ParseSemicolonRewardItems(rewardItems);
            for (int i = 0; i < needItems.Count; i++)
            {
                RewardItem itemInfo = needItems[i];
                if (self.GetItemNumber(ItemBigType.Type_Item, itemInfo.ItemID) < itemInfo.ItemNum)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckNeedItem(this BagComponentServer self, List<RewardItem> rewardItems)
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
        public static bool OnCostItemData(this BagComponentServer self, string rewardItems, ItemLocType itemLocType, int itemGetWay)
        {
            List<RewardItem> costItems = ParseSemicolonRewardItems(rewardItems);
            return self.OnCostItemData(costItems, itemLocType, itemGetWay);
        }

        //删除背包道具道具[支持同时添加多个]
        public static bool OnCostItemData(this BagComponentServer self, long bagInfoId, ItemLocType itemLocType = ItemLocType.ItemLocBag)
        {
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            List<BagInfo> itemTypeList = self.GetItemByLoc(itemLocType);
            for (int k = itemTypeList.Count - 1; k >= 0; k--)
            {
                if (itemTypeList[k].BagInfoID == bagInfoId)
                {
                    m2c_bagUpdate.BagInfoDelete.Add(itemTypeList[k]);
                    itemTypeList.RemoveAt(k);
                    break;
                }
            }

            MessageHelper.SendToClient(self.GetParent<Unit>(), m2c_bagUpdate);
            return true;
        }

        public static bool OnCostItemData(this BagComponentServer self, List<long> costItems, ItemLocType itemLocType = ItemLocType.ItemLocBag)
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
        public static bool OnCostItemData(this BagComponentServer self, long uid, int number)
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
        public static bool OnCostItemData(this BagComponentServer self, List<RewardItem> costItems, ItemLocType itemLocType, int itemGetWay)
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
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
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
                    roleInfo.UpdateRoleMoneySub(UserDataType.Gold, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
            
                if (itemID == (int)UserDataType.Diamond)
                {
                    itemNum = -1 * itemNum;
                    roleInfo.UpdateRoleMoneySub(UserDataType.Diamond, itemNum.ToString(), true, itemGetWay);
                    continue;
                }
             
                if (itemID == (int)UserDataType.JiaYuanFund)
                {
                    itemNum = -1 * itemNum;
                    roleInfo.UpdateRoleData(UserDataType.JiaYuanFund, itemNum.ToString());
                    continue;
                }
               
                if (itemID == (int)UserDataType.UnionContri)
                {
                    itemNum = -1 * itemNum;
                    roleInfo.UpdateRoleData(UserDataType.UnionContri, itemNum.ToString());
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

        public static int GetQiangHuaLevel(this BagComponentServer self, int subType)
        {
            return 0;
        }

        public static void OnGmGaoJi(this BagComponentServer self, int level)
        {
          
        
        }

        public static void GetEquipAttribute(this BagComponentServer self, List<AttributeItem> occInitAttribute)
        {
            Unit unit = self.GetParent<Unit>();
            List<BagInfo> equipList = self.GetItemByLoc(ItemLocType.ItemLocEquip);
            Dictionary<int, int> suitPointsMap = new Dictionary<int, int>();

            for (int i = equipList.Count - 1; i >= 0; i--)
            {
                BagInfo userBagInfo = equipList[i];
                if (!LDEquipCategory.Instance.Contain(userBagInfo.ItemID))
                {
                    equipList.RemoveAt(i);
                    continue;
                }

                LDEquip itemCof = LDEquipCategory.Instance.Get(userBagInfo.ItemID);
                if (itemCof.EquipSuitID == 0)
                {
                    continue;
                }

                if (!suitPointsMap.TryGetValue(itemCof.EquipSuitID, out int suitPoints))
                {
                    suitPoints = 0;
                }

                suitPointsMap[itemCof.EquipSuitID] = suitPoints + itemCof.EquipSuitParam;
            }

            // 套装效果：按点数获取效果ID，效果表尚未接入
            foreach (KeyValuePair<int, int> suitPoints in suitPointsMap)
            {
                if (!LDEquip_SuitCategory.Instance.Contain(suitPoints.Key))
                {
                    continue;
                }

                LDEquip_Suit ldEquipSuitCof = LDEquip_SuitCategory.Instance.Get(suitPoints.Key);
                List<int> suitEffectIds = GetActiveEquipSuitEffectIds(suitPoints.Value, ldEquipSuitCof.Effect_Id);
                if (suitEffectIds.Count > 0)
                {
                    // 效果表未配置，暂不写入属性
                }
            }

            for (int i = 0; i < equipList.Count; i++)
            {
                LDEquip mLdEquipCon = LDEquipCategory.Instance.Get(equipList[i].ItemID);
                int equipType = ItemNewHelper.GetNewEquipType(equipList[i]);

                //极品属性
                //强化登录（List长度13， 13个位置）
                int caowei = ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID);
                int qianghuaLv = self.GetQiangHuaLevel(caowei);

                occInitAttribute.AddRange( equipList[i].BaseAttrList );

                //获取宝石属性
                string gemIdNew = equipList[i].GemIDNew;
                if (string.IsNullOrEmpty(gemIdNew))
                {
                    gemIdNew = ItemNewHelper.GetDefaultGem();
                    equipList[i].GemIDNew = gemIdNew;
                    //Log.Debug($"GemIDNew==null  unit.Id: {unit.Id} BagInfoID:{equipList[i].BagInfoID}");
                }

                string[] gemList = gemIdNew.Split('_');

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
                    string[] attributeList = null;//gemitemCof.ItemUsePar.Split('@');
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

                        //AddUpdateProDicList(gemPro, gemValue, UpdateProDicList);
                    }
                }
            }
        }

        public static void GetSeasonHexinAttribute(this BagComponentServer self)
        {
            
            ///晶核列表
            /*
            List<BagInfo> jingHeList = unit.GetComponent<BagComponentServer>().GetCurJingHeList();

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
            
            /*SeasonLevelConfig seasonLevelConfig = SeasonLevelConfigCategory.Instance.Get(roleInfo.SeasonLevel);
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
                            AddUpdateProDicList(key, NumericHelp.ParseConfigToStored(key, addPro[1]), UpdateProDicList);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"赛季属性配置错误：{ex.ToString()} {seasonLevelConfig.PripertySet}");
                    }
                }
            }*/
        }

        public static bool OnCostItemData(this BagComponentServer self, BagInfo bagInfo, ItemLocType locType, int number)
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

        private static List<RewardItem> ParseSemicolonRewardItems(string rewardItems)
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
                costItems.Add(new RewardItem()
                {
                    ItemID = int.Parse(itemInfo[0]),
                    ItemNum = int.Parse(itemInfo[1])
                });
            }
            return costItems;
        }

        private static List<int> GetActiveEquipSuitEffectIds(int suitPoints, string effectIdStr)
        {
            List<int> effectIds = new List<int>();
            if (suitPoints <= 0 || string.IsNullOrEmpty(effectIdStr))
            {
                return effectIds;
            }

            string[] effectTiers = effectIdStr.Split('|');
            for (int i = 0; i < effectTiers.Length; i++)
            {
                string tier = effectTiers[i];
                if (string.IsNullOrEmpty(tier))
                {
                    continue;
                }

                int sep = tier.IndexOf('_');
                if (sep <= 0 || sep >= tier.Length - 1)
                {
                    continue;
                }

                if (!int.TryParse(tier.Substring(0, sep), out int needPoints) || !int.TryParse(tier.Substring(sep + 1), out int effectId))
                {
                    continue;
                }

                if (suitPoints >= needPoints && effectId > 0)
                {
                    effectIds.Add(effectId);
                }
            }

            return effectIds;
        }
    }
}