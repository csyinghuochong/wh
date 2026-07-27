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
            for (int i = self.AdditionalCellNum.Count; i < (int)ItemLocType.ItemLocMax; i++)
            {
                self.AdditionalCellNum.Add(0);
            }


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


        public static List<BagInfo> GetItemByLoc(this BagComponentServer self, ItemLocType itemEquipType)
        {
            List<BagInfo> ItemTypeList = null;
            switch (itemEquipType)
            {
                case ItemLocType.ItemLocEquip:
                    ItemTypeList = self.EquipList;
                    break;
                case ItemLocType.ItemLocBag:
                    ItemTypeList = self.BagItemList;
                    break;
                case ItemLocType.ItemLocBagTreasure:
                    ItemTypeList = self.TreasureList;
                    break;
                case ItemLocType.ItemLocBagMaterial:
                    ItemTypeList = self.MaterialList;
                    break;
                case ItemLocType.ItemLocBagConsume:
                    ItemTypeList = self.ConsumeList;
                    break;
                case ItemLocType.ItemLocBagLife:
                    ItemTypeList = self.LifeList;
                    break;
                case ItemLocType.ItemLocBagHome:
                    ItemTypeList = self.HomeList;
                    break;

                case ItemLocType.ItemWareHouse1:
                    ItemTypeList = self.Warehouse1;
                    break;
            }
            return ItemTypeList;
        }

        /// <summary>
        /// 是否需要背包格子 Position（穿戴栏不用格子号）。Position 从 1 起。
        /// </summary>
        public static bool NeedBagGridPosition(ItemLocType loc)
        {
            return loc != ItemLocType.ItemLocEquip;
        }

        /// <summary>
        /// 登录校验：Position&lt;1 或冲突时补最小空位。
        /// </summary>
        public static void EnsureBagPositions(List<BagInfo> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return;
            }

            HashSet<int> used = new HashSet<int>();
            List<BagInfo> needAssign = new List<BagInfo>();
            for (int i = 0; i < itemList.Count; i++)
            {
                BagInfo info = itemList[i];
                if (info.Position >= 1 && used.Add(info.Position))
                {
                    continue;
                }
                needAssign.Add(info);
            }

            int pos = 1;
            for (int i = 0; i < needAssign.Count; i++)
            {
                while (used.Contains(pos))
                {
                    pos++;
                }
                needAssign[i].Position = pos;
                used.Add(pos);
                pos++;
            }
        }

        /// <summary>
        /// 分配最小空位：1,2,3… 中间卖掉空出后优先填回。
        /// </summary>
        public static int AllocBagPosition(List<BagInfo> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return 1;
            }

            HashSet<int> used = new HashSet<int>();
            for (int i = 0; i < itemList.Count; i++)
            {
                int p = itemList[i].Position;
                if (p >= 1)
                {
                    used.Add(p);
                }
            }

            int pos = 1;
            while (used.Contains(pos))
            {
                pos++;
            }
            return pos;
        }

        public static int AllocBagPosition(this BagComponentServer self, ItemLocType loc)
        {
            List<BagInfo> itemList = self.GetItemByLoc(loc);
            return AllocBagPosition(itemList);
        }


        public static void OnRecvItemSort(this BagComponentServer self, ItemLocType itemEquipType)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemEquipType);
            if (ItemTypeList == null || ItemTypeList.Count == 0)
            {
                return;
            }

            // 穿戴栏不占背包格子号；其余 Loc：按 ItemID 排序后 Position 压成 1..n
            // 不通知客户端，客户端用 BagSortHelper 同一规则自行整理表现
            BagSortHelper.SortBagItems(ItemTypeList, NeedBagGridPosition(itemEquipType));
        }

        public static void CheckValiedItem(this BagComponentServer self, List<BagInfo> bagInfos, int occ, int occTwo)
        {
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
            self.CheckValiedItem(self.EquipList, occ, occTwo);
            self.CheckValiedItem(self.BagItemList, occ, occTwo);
            self.CheckValiedItem(self.TreasureList, occ, occTwo);
            self.CheckValiedItem(self.MaterialList, occ, occTwo);
            self.CheckValiedItem(self.ConsumeList, occ, occTwo);
            self.CheckValiedItem(self.LifeList, occ, occTwo);
            self.CheckValiedItem(self.HomeList, occ, occTwo);
            self.CheckValiedItem(self.Warehouse1, occ, occTwo);

            EnsureBagPositions(self.BagItemList);
            EnsureBagPositions(self.TreasureList);
            EnsureBagPositions(self.MaterialList);
            EnsureBagPositions(self.ConsumeList);
            EnsureBagPositions(self.LifeList);
            EnsureBagPositions(self.HomeList);
            EnsureBagPositions(self.Warehouse1);
        }

        //获取自身所有的道具
        public static List<BagInfo> GetAllItems(this BagComponentServer self, int occ, int occTwo)
        {
            List<BagInfo> bagList = new List<BagInfo>();

            self.CheckAllItem(occ, occTwo);

            bagList.AddRange(self.EquipList);
            bagList.AddRange(self.BagItemList);
            bagList.AddRange(self.TreasureList);
            bagList.AddRange(self.MaterialList);
            bagList.AddRange(self.ConsumeList);
            bagList.AddRange(self.LifeList);
            bagList.AddRange(self.HomeList);

            bagList.AddRange(self.Warehouse1);
         
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
                case UserDataType.BindGold:
                case UserDataType.Diamond:
                case UserDataType.BindDiamond:
                case UserDataType.JiaYuanFund:
                case UserDataType.UnionContri:
                {
                    Unit unit = self.GetParent<Unit>();
                    RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
                    switch (userDataType)
                    {
                        case UserDataType.Gold:
                            number = roleInfo.Gold;
                            break;
                        case UserDataType.BindGold:
                            number = roleInfo.BindGold;
                            break;
                        case UserDataType.Diamond:
                            number = roleInfo.Diamond;
                            break;
                        case UserDataType.BindDiamond:
                            number = roleInfo.BindDiamond;
                            break;
                        case UserDataType.JiaYuanFund:
                            number = roleInfo.JiaYuanFund;
                            break;
                        case UserDataType.UnionContri:
                            number = roleInfo.UnionZiJin;
                            break;
                    }
                    break;
                }
                case UserDataType.DailyActive:
                {
                    number = self.GetParent<Unit>().GetComponent<RoleDailyDataComponentServer>()?.GetDailyActivePoint() ?? 0;
                    break;
                }
                case UserDataType.WeeklyActive:
                {
                    number = self.GetParent<Unit>().GetComponent<RoleDailyDataComponentServer>()?.GetWeeklyActivePoint() ?? 0;
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


        public static bool IsBagFullByLoc(this BagComponentServer self, int hourseId)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return ItemTypeList.Count >= self.GeBagTotalCell(hourseId);
        }

        public static int GetBagLeftCell(this BagComponentServer self, int hourseId = (int)ItemLocType.ItemLocBag)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc((ItemLocType)hourseId);
            return self.GeBagTotalCell(hourseId) - ItemTypeList.Count;
        }

        public static int GeBagTotalCell(this BagComponentServer self, int hourseId)
        {
            int storeCapacity = LDGlobalValueCategory.Instance.BagInitCapacity[hourseId];
            return storeCapacity + self.AdditionalCellNum[hourseId];
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
            if (NeedBagGridPosition(itemLocTypeTo))
            {
                bagInfo.Position = AllocBagPosition(ItemTypeListDest);
            }
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
            if (bagInfos == null || bagInfos.Count == 0)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            M2C_RoleBagUpdate uniqueUpdate = new M2C_RoleBagUpdate();
            bool hasUniqueAdd = false;
            Dictionary<string, List<RewardItem>> stackableByGetWay = null;

            for (int i = 0; i < bagInfos.Count; i++)
            {
                BagInfo bagInfo = bagInfos[i];
                LDItem ldItemCof = LDItemCategory.Instance.Get(bagInfo.ItemID);
                int maxPileSum = ldItemCof.ItemPileSum;

                if (maxPileSum > 1 || bagInfo.BagInfoID == 0)
                {
                    string way = string.IsNullOrEmpty(bagInfo.GetWay) ? getType : bagInfo.GetWay;
                    if (stackableByGetWay == null)
                    {
                        stackableByGetWay = new Dictionary<string, List<RewardItem>>();
                    }
                    if (!stackableByGetWay.TryGetValue(way, out List<RewardItem> rewardList))
                    {
                        rewardList = new List<RewardItem>();
                        stackableByGetWay[way] = rewardList;
                    }
                    int itemType = bagInfo.ItemType != 0 ? bagInfo.ItemType : ItemBigType.Type_Item;
                    rewardList.Add(new RewardItem()
                    {
                        ItemType = itemType,
                        ItemID = bagInfo.ItemID,
                        ItemNum = bagInfo.ItemNum
                    });
                    continue;
                }

                self.BagItemList.Add(bagInfo);
                uniqueUpdate.BagInfoAdd.Add(bagInfo);
                hasUniqueAdd = true;

                string[] getWayParts = getType.Split('_');
                int getTypeValue = int.Parse(getWayParts[0]);
                ItemAddHelper.OnGetItem(unit, getTypeValue, bagInfo);
            }

            // 可堆叠按 GetWay 聚合后一次入包
            if (stackableByGetWay != null)
            {
                foreach (KeyValuePair<string, List<RewardItem>> kv in stackableByGetWay)
                {
                    self.OnAddItemData(kv.Value, string.Empty, kv.Key);
                }
            }

            if (hasUniqueAdd)
            {
                MessageHelper.SendToClient(unit, uniqueUpdate);
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
                bagInfo.Position = self.AllocBagPosition(ItemLocType.ItemLocBag);
                self.BagItemList.Add(bagInfo);

                Unit parentUnit = self.GetParent<Unit>();
                M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
                m2c_bagUpdate.BagInfoAdd.Add(bagInfo);
                //通知客户端背包道具发生改变
                MessageHelper.SendToClient(parentUnit, m2c_bagUpdate);

                //检测任务需求道具
                string[] getWayParts = getType.Split('_');
                int getTypeValue = int.Parse(getWayParts[0]);
                ItemAddHelper.OnGetItem(parentUnit, getTypeValue, bagInfo);
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
            ItemLocType storeLoc = (ItemLocType)useBagInfo.Loc;
            List<BagInfo> storeList = self.GetItemByLoc(storeLoc);
            if (NeedBagGridPosition(storeLoc))
            {
                useBagInfo.Position = AllocBagPosition(storeList);
            }
            storeList.Add(useBagInfo);

            Unit parentUnit = self.GetParent<Unit>();
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(parentUnit, m2c_bagUpdate);
        }

        public static void OnAddItemDataNewCell(this BagComponentServer self, BagInfo bagInfo, int itemnumber)
        {
            int itemid = bagInfo.ItemID;
            BagInfo useBagInfo = new BagInfo();
            useBagInfo.ItemID = itemid;
            useBagInfo.ItemNum = itemnumber;
            LDItem ldItemCof = LDItemCategory.Instance.Get(itemid);
            useBagInfo.Loc = (int)ItemLocType.ItemLocBag;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GetWay = bagInfo.GetWay;
            useBagInfo.isBinging = bagInfo.isBinging;
            List<BagInfo> bagList = self.GetItemByLoc(ItemLocType.ItemLocBag);
            useBagInfo.Position = AllocBagPosition(bagList);
            bagList.Add(useBagInfo);

            Unit parentUnit = self.GetParent<Unit>();
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(parentUnit, m2c_bagUpdate);
        }

        public static HashSet<int> BuildUsedBagPositions(List<BagInfo> itemList)
        {
            HashSet<int> used = new HashSet<int>();
            if (itemList == null)
            {
                return used;
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                int p = itemList[i].Position;
                if (p >= 1)
                {
                    used.Add(p);
                }
            }
            return used;
        }

        /// <summary>
        /// 在已有 used 集合上取最小空位，并写入 used（同批连开多格时避免反复扫列表）。
        /// </summary>
        public static int AllocBagPosition(HashSet<int> used)
        {
            if (used == null)
            {
                return 1;
            }

            int pos = 1;
            while (used.Contains(pos))
            {
                pos++;
            }
            used.Add(pos);
            return pos;
        }

        public static ItemLocType ResolveAddItemLoc(ItemLocType specLocType, RewardItem rewardItem)
        {
            if (specLocType == ItemLocType.ItemLocMax)
            {
                return ItemNewHelper.GetToItemLocType(rewardItem);
            }

            return specLocType;
        }

        //添加背包道具道具[支持同时添加多个]
        public static bool OnAddItemData(this BagComponentServer self, List<RewardItem> rewardItems_init, string makeUserID, string getWay, bool notice = true, bool gm = false, ItemLocType specLocType = ItemLocType.ItemLocMax)
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
            
            string[] getWayInfo = getWay.Split('_');
            int getType = int.Parse(getWayInfo[0]);
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (unit.IsRobot() && getType == ItemGetWay.PickItem)
            {
                return true;
            }

            if (getType == ItemGetWay.GM)
            {
                gm = true;
            }

            // 同 ItemType+ItemID 合并数量，减少后续扫描与满格误判
            Dictionary<long, RewardItem> rewardItemMap = new Dictionary<long, RewardItem>();
            for (int i = 0; i < rewardItems_init.Count; i++)
            {
                RewardItem src = rewardItems_init[i];
                long key = ((long)src.ItemType << 32) | (uint)src.ItemID;
                if (rewardItemMap.TryGetValue(key, out RewardItem merged))
                {
                    merged.ItemNum += src.ItemNum;
                }
                else
                {
                    rewardItemMap[key] = new RewardItem()
                    {
                        ItemType = src.ItemType,
                        ItemID = src.ItemID,
                        ItemNum = src.ItemNum
                    };
                }
            }

            List<RewardItem> rewardItems = new List<RewardItem>(rewardItemMap.Count);
            Dictionary<long, long> pileSumCache = new Dictionary<long, long>();
            Dictionary<int, int> leftCellByLoc = new Dictionary<int, int>();

            foreach (RewardItem rewardItem in rewardItemMap.Values)
            {
                // 货币大类 / 道具里映射为货币的：不占背包格，但仍进入发奖列表
                int userDataType = ItemNewHelper.GetItemToUserDataType(rewardItem);
                if (userDataType != UserDataType.None)
                {
                    rewardItems.Add(rewardItem);
                    continue;
                }

                if (rewardItem.ItemType == ItemBigType.Type_Exp)
                {
                    // Type_Money 但 ItemID 非法：丢弃
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
                    continue;
                }

                long itemKey = ((long)rewardItem.ItemType << 32) | (uint)rewardItem.ItemID;
                if (!pileSumCache.TryGetValue(itemKey, out long itemPileSum))
                {
                    itemPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                    pileSumCache[itemKey] = itemPileSum;
                }

                ItemLocType toLocType = ResolveAddItemLoc(specLocType, rewardItem);
                List<BagInfo> locList = self.GetItemByLoc(toLocType);
                if (locList == null)
                {
                    Log.Error($"OnAddItemData invalid loc={(int)toLocType} item={rewardItem.ItemID}");
                    return false;
                }

                int locKey = (int)toLocType;
                if (!leftCellByLoc.TryGetValue(locKey, out int leftCell))
                {
                    leftCell = self.GetBagLeftCell(locKey);
                    leftCellByLoc[locKey] = leftCell;
                }

                int needCells = ItemNewHelper.CalcNeedNewCells(locList, rewardItem.ItemType, rewardItem.ItemID, rewardItem.ItemNum, (int)itemPileSum);
                if (needCells > leftCell)
                {
                    return false;
                }

                leftCellByLoc[locKey] = leftCell - needCells;
                rewardItems.Add(rewardItem);
            }

            if (rewardItems.Count == 0)
            {
                return true;
            }

            M2C_RoleBagUpdate m2c_bagUpdate = self.message;
            m2c_bagUpdate.BagInfoAdd.Clear();
            m2c_bagUpdate.BagInfoUpdate.Clear();
            m2c_bagUpdate.BagInfoDelete.Clear();
            Dictionary<int, long> currencyAdds = null;
            Dictionary<int, HashSet<int>> usedPosByLoc = null;
            bool forceBindByGetWay = false;

            for (int i = 0; i < rewardItems.Count; i++)
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
                if (userDataType != UserDataType.None)
                {
                    if (currencyAdds == null)
                    {
                        currencyAdds = new Dictionary<int, long>();
                    }
                    currencyAdds.TryGetValue(userDataType, out long currencySum);
                    currencyAdds[userDataType] = currencySum + leftNum;
                    continue;
                }

                long itemKey = ((long)rewardItem.ItemType << 32) | (uint)rewardItem.ItemID;
                if (!pileSumCache.TryGetValue(itemKey, out long cachedPileSum))
                {
                    cachedPileSum = ItemNewHelper.GetNewItemPileSum(rewardItem);
                    pileSumCache[itemKey] = cachedPileSum;
                }
                int maxPileSum = (int)cachedPileSum;
                ItemLocType toLocType = ResolveAddItemLoc(specLocType, rewardItem);
                List<BagInfo> itemlist = self.GetItemByLoc(toLocType);
                if (itemlist == null)
                {
                    Log.Error($"OnAddItemData insert invalid loc={(int)toLocType} item={itemID}");
                    return false;
                }
                
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
                        break;
                    }
                }

                HashSet<int> usedPos = null;
                while (leftNum > 0)
                {
                    BagInfo useBagInfo = new BagInfo();
                    
                    useBagInfo.ItemType = itemtype;
                    useBagInfo.ItemID = itemID;
                    useBagInfo.ItemNum = (leftNum > maxPileSum) ? maxPileSum : leftNum;
                    useBagInfo.Loc = (int)toLocType;
                    useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
                    useBagInfo.GetWay = getWay;
                    leftNum -= useBagInfo.ItemNum;
                    useBagInfo.MakePlayer = makeUserID;
                    useBagInfo.isBinging = forceBindByGetWay || ItemNewHelper.CheckItemIfLock(rewardItem);

                    if (itemtype == ItemBigType.Type_Equip && useBagInfo.BaseAttrList.Count <= 0)
                    {
                        LDEquip equipconfig = LDEquipCategory.Instance.Get(itemID);
                        useBagInfo.EnhanceLevel = RandomHelper.RandomNumber(0, equipconfig.Enhance);
                        useBagInfo.BaseAttrList = (LDEquipCategory.Instance.GetEquipAttribute(itemID));
                        ItemNewHelper.SortBaseAttrList(useBagInfo.BaseAttrList);
                    }
                    if (itemtype == ItemBigType.Type_Item)
                    {
                        int subType = LDItemCategory.Instance.Get(itemID).ItemType;
                    }

                    if (NeedBagGridPosition(toLocType))
                    {
                        if (usedPos == null)
                        {
                            if (usedPosByLoc == null)
                            {
                                usedPosByLoc = new Dictionary<int, HashSet<int>>();
                            }
                            int locKey = (int)toLocType;
                            if (!usedPosByLoc.TryGetValue(locKey, out usedPos))
                            {
                                usedPos = BuildUsedBagPositions(itemlist);
                                usedPosByLoc[locKey] = usedPos;
                            }
                        }
                        useBagInfo.Position = AllocBagPosition(usedPos);
                    }

                    itemlist.Add(useBagInfo);
                    m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
                }

                if (notice)
                {
                    ItemAddHelper.OnGetItem(unit, getType, itemtype, itemID, rewardItem.ItemNum);
                }
            }

            if (currencyAdds != null)
            {
                RoleDailyDataComponentServer dailyData = unit.GetComponent<RoleDailyDataComponentServer>();
                foreach (KeyValuePair<int, long> kv in currencyAdds)
                {
                    if (kv.Key == UserDataType.DailyActive || kv.Key == UserDataType.WeeklyActive)
                    {
                        dailyData?.AddActivePoint(kv.Key, (int)kv.Value, notice);
                        continue;
                    }

                    roleInfoComponent.UpdateRoleMoneyAdd(kv.Key, kv.Value.ToString(), true, getType);
                }
            }

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
                int itemType = itemInfo.ItemType > 0 ? itemInfo.ItemType : ItemBigType.Type_Item;
                if (self.GetItemNumber(itemType, itemInfo.ItemID) < itemInfo.ItemNum)
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
                int itemType = costItems[i].ItemType > 0 ? costItems[i].ItemType : ItemBigType.Type_Item;
                int itemID = costItems[i].ItemID;
                int itemNum = costItems[i].ItemNum;

                //获取背包内的道具是否足够
                if (self.GetItemNumber(itemType, itemID, itemLocType) < itemNum)
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
                ItemAddHelper.OnCostItem(unit, ItemBigType.Type_Item, itemID);
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
            Unit unit = self.GetParent<Unit>();

            if (bagInfo.ItemNum >= number)
            {
                bagInfo.ItemNum -= number;

                if (bagInfo.ItemNum <= 0)
                {
                    bagInfos.Remove(bagInfo);
                }
                LogHelper.LogWarning($"消耗道具: {unit.Id} {bagInfo.ItemID} {number}", false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static List<RewardItem> ParseSemicolonRewardItems(string rewardItems)
        {
            return ItemNewHelper.GetRewardItemsAtSemicolon(rewardItems);
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