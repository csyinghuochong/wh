using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class BagComponentAwakeSystem : AwakeSystem<BagComponentServer>
    {

        public override void Awake(BagComponentServer self)
        {
            self.CangKuNumber = LDGlobalValueCategory.Instance.DefaultCangKuNumber;
            self.EnsureItemLists();
        }
    }

    [ObjectSystem]
    public class BagComponentDeserializeSystem : DeserializeSystem<BagComponentServer>
    {
        public override void Deserialize(BagComponentServer self)
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
            int[] equipInit = ldOccupation.Equip_Init;
            if (equipInit == null || equipInit.Length == 0)
            {
                return;
            }

            // 装备栏无格子容量，先入背包（走属性/绑定等生成），再 OnChangeItemLoc 穿上
            string getWay = $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}";
            List<RewardItem> rewardItems = new List<RewardItem>(equipInit.Length);
            for (int i = 0; i < equipInit.Length; i++)
            {
                int equipId = equipInit[i];
                if (equipId <= 0)
                {
                    continue;
                }

                rewardItems.Add(new RewardItem()
                {
                    ItemType = ItemBigType.Type_Equip,
                    ItemID = equipId,
                    ItemNum = 1
                });
            }

            if (rewardItems.Count == 0)
            {
                return;
            }

            if (!self.OnAddItemData(rewardItems, string.Empty, getWay, false))
            {
                return;
            }

            for (int i = 0; i < rewardItems.Count; i++)
            {
                List<BagInfo> bagInfos = self.GetIdItemList(ItemBigType.Type_Equip, rewardItems[i].ItemID, ItemLocType.ItemLocBag);
                if (bagInfos.Count == 0)
                {
                    continue;
                }

                self.OnChangeItemLoc(bagInfos[0], ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
            }
        }


        public static void EnsureItemLists(this BagComponentServer self)
        {
            self.AllItemList ??= new Dictionary<int, List<BagInfo>>();
        }

        static List<BagInfo> GetOrCreateItemList(this BagComponentServer self, ItemLocType loc)
        {
            self.AllItemList ??= new Dictionary<int, List<BagInfo>>();
            int locKey = (int)loc;
            if (!self.AllItemList.TryGetValue(locKey, out List<BagInfo> bagList) || bagList == null)
            {
                bagList = new List<BagInfo>();
                self.AllItemList[locKey] = bagList;
            }

            return bagList;
        }

        public static List<BagInfo> GetItemByLoc(this BagComponentServer self, ItemLocType itemEquipType)
        {
            return self.GetOrCreateItemList(itemEquipType);
        }

        public static void OnRecvItemSort(this BagComponentServer self, ItemLocType itemEquipType)
        {
            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemEquipType);
            if (ItemTypeList == null || ItemTypeList.Count == 0)
            {
                return;
            }

            // 不通知客户端，客户端用 BagSortHelper 同一规则自行整理表现
            BagSortHelper.SortIfNeeded(ItemTypeList, itemEquipType);
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
            foreach (KeyValuePair<int, List<BagInfo>> kv in self.AllItemList)
            {
                if (kv.Value == null)
                {
                    continue;
                }

                self.CheckValiedItem(kv.Value, occ, occTwo);
                BagSortHelper.SortIfNeeded(kv.Value, kv.Key);
            }
        }

        //获取自身所有的道具
        public static List<BagInfo> GetAllItems(this BagComponentServer self, int occ, int occTwo)
        {
            List<BagInfo> bagList = new List<BagInfo>();

            self.CheckAllItem(occ, occTwo);

            foreach (List<BagInfo> locList in self.AllItemList.Values)
            {
                if (locList == null || locList.Count == 0)
                {
                    continue;
                }

                bagList.AddRange(locList);
            }
         
            return bagList;
        }


        public static List<BagInfo> GetIdItemList(this BagComponentServer self, int itemType,  int itemId, ItemLocType loc =ItemLocType.ItemLocBag)
        {
            List<BagInfo> baginfo = new List<BagInfo>();
            List<BagInfo> bagList = self.GetItemByLoc(loc);
            for (int i = 0; i < bagList.Count; i++)
            {
                if (bagList[i].ItemID == itemId && bagList[i].ItemType == itemType)
                {
                    baginfo.Add(bagList[i]);
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
                case UserDataType.DailyActive:
                case UserDataType.WeeklyActive:
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
                        case UserDataType.TiLi:
                            number = roleInfo.TiLi;
                            break;
                        case UserDataType.HuoLi:
                            number = roleInfo.HuoLi;
                            break;
                        case UserDataType.DailyActive:
                            number = unit.GetComponent<RoleDailyDataComponentServer>()?.GetDailyActivePoint() ?? 0;
                            break;
                        case UserDataType.WeeklyActive:
                            number = unit.GetComponent<RoleDailyDataComponentServer>()?.GetWeeklyActivePoint() ?? 0;
                            break;
                        case UserDataType.JiaYuanFund:
                            number = unit.GetComponent<JiaYuanComponentServer>()?.JiaYuanFund ?? 0;
                            break;
                        case UserDataType.UnionContri:
                            number = roleInfo.UnionZiJin;
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
            ItemTypeListDest.Add(bagInfo);
            BagSortHelper.SortIfNeeded(ItemTypeListDest, itemLocTypeTo);
        }

        /// <summary>
        /// 是否有装备技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool IsHaveEquipSkill(this BagComponentServer self, int skillId, long xilianequip)
        {
            List<BagInfo> equipList = self.GetItemByLoc(ItemLocType.ItemLocEquip);
            for (int i = 0; i < equipList.Count; i++)
            {
                if (equipList[i].BagInfoID == xilianequip)
                {
                    continue;
                }

                LDItem ldItem = LDItemCategory.Instance.Get(equipList[i].ItemID);
                /*if (Item.SkillID.Contains(skillId.ToString()))
                {
                    return true;
                }*/
            }
            return false;
        }

        public static void OnResetSeason(this BagComponentServer self, bool notice)
        { 
            self.ClearJingHeItem(self.GetItemByLoc(ItemLocType.ItemLocBag));
            self.ClearJingHeItem(self.GetItemByLoc(ItemLocType.ItemWareHouse1));
          
        }

        public static void ClearJingHeItem(this BagComponentServer self, List<BagInfo> bagInfos)
        {
            for (int i = bagInfos.Count - 1; i >= 0; i--)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(bagInfos[i].ItemID);
               
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
            List<BagInfo> equiplist = self.GetItemByLoc(ItemLocType.ItemLocEquip);

            for (int i = 0; i < equiplist.Count; i++)
            {
                if (equiplist[i].ItemType != ItemBigType.Type_Equip)
                {
                    continue;
                }

                if (!LDItemCategory.Instance.Contain(equiplist[i].ItemID))
                {
                    continue;
                }

                LDItem ldItem = LDItemCategory.Instance.Get(equiplist[i].ItemID);

                LDEquip ldEquip = LDEquipCategory.Instance.Get(equiplist[i].ItemID);
                /*if (equip.TianFuId != 0)
                {
                    equiptianfuids.Add(equip.TianFuId);
                }*/
            }

            return equiptianfuids;
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
        }

        public static int GetWuqiItemId(this BagComponentServer self)
        {
            BagInfo bagInfo = self.GetEquipBySubType(ItemLocType.ItemLocEquip, (int)EquipCaoWeiTypeEnum.Wuqi_1);
            return bagInfo != null ? bagInfo.ItemID : 0;
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

                self.GetItemByLoc(ItemLocType.ItemLocBag).Add(bagInfo);
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
                BagSortHelper.SortIfNeeded(self.GetItemByLoc(ItemLocType.ItemLocBag), ItemLocType.ItemLocBag);
                MessageHelper.SendToClient(unit, uniqueUpdate);
            }
        }

        public static bool OnAddItemData(this BagComponentServer self, BagInfo bagInfo, string getType)
        {
            LDItem ldItemCof = LDItemCategory.Instance.Get(bagInfo.ItemID);
            int maxPileSum = ldItemCof.ItemPileSum;

            if (maxPileSum > 1 || bagInfo.BagInfoID == 0)
            {
                return self.OnAddItemData($"{bagInfo.ItemType}_{bagInfo.ItemID}_{bagInfo.ItemNum}", string.IsNullOrEmpty(bagInfo.GetWay) ? getType : bagInfo.GetWay);
            }
            else
            {
                self.GetItemByLoc(ItemLocType.ItemLocBag).Add(bagInfo);
                BagSortHelper.SortIfNeeded(self.GetItemByLoc(ItemLocType.ItemLocBag), ItemLocType.ItemLocBag);

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
            storeList.Add(useBagInfo);
            BagSortHelper.SortIfNeeded(storeList, storeLoc);

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
            useBagInfo.ItemType = bagInfo.ItemType;
            useBagInfo.Loc = (int)ItemLocType.ItemLocBag;
            useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            useBagInfo.GetWay = bagInfo.GetWay;
            useBagInfo.SetBinding(bagInfo.IsBinding());
            List<BagInfo> bagList = self.GetItemByLoc(ItemLocType.ItemLocBag);
            bagList.Add(useBagInfo);
            BagSortHelper.SortIfNeeded(bagList, ItemLocType.ItemLocBag);

            Unit parentUnit = self.GetParent<Unit>();
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoAdd.Add(useBagInfo);
            //通知客户端背包道具发生改变
            MessageHelper.SendToClient(parentUnit, m2c_bagUpdate);
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

            int getType = int.Parse(getWay.Split('_')[0]);
            Unit unit = self.GetParent<Unit>();
            List<RewardItem> bagItems = new List<RewardItem>();
            Dictionary<int, int> leftCellByLoc = new Dictionary<int, int>();

            foreach (RewardItem item in MergeRewardItems(rewardItems_init).Values)
            {
                if (TryAddCurrency(unit, item, getType, notice) || TrySkipBag(unit, item, getType, notice))
                {
                    continue;
                }

                if (!TryReserveBagCell(self, item, specLocType, leftCellByLoc))
                {
                    return false;
                }

                bagItems.Add(item);
            }

            if (bagItems.Count > 0)
            {
                InsertBagItems(self, unit, bagItems, makeUserID, getWay, getType, notice, specLocType);
            }

            return true;
        }

        private static Dictionary<long, RewardItem> MergeRewardItems(List<RewardItem> srcList)
        {
            Dictionary<long, RewardItem> map = new Dictionary<long, RewardItem>();
            for (int i = 0; i < srcList.Count; i++)
            {
                RewardItem src = srcList[i];
                long key = ((long)src.ItemType << 32) | (uint)src.ItemID;
                if (map.TryGetValue(key, out RewardItem merged))
                {
                    merged.ItemNum += src.ItemNum;
                    continue;
                }

                map[key] = new RewardItem { ItemType = src.ItemType, ItemID = src.ItemID, ItemNum = src.ItemNum };
            }

            return map;
        }

        private static bool TryAddCurrency(Unit unit, RewardItem item, int getType, bool notice)
        {
            int dataType = ItemNewHelper.GetItemToUserDataType(item);
            if (dataType == UserDataType.None)
            {
                return false;
            }

            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(dataType, item.ItemNum.ToString(), true, getType);
            return true;
        }

        private static bool TrySkipBag(Unit unit, RewardItem item, int getType, bool notice)
        {
            if (item.ItemType == ItemBigType.Type_Exp)
            {
                return true;
            }

            if (item.ItemType != ItemBigType.Type_Item && item.ItemType != ItemBigType.Type_Equip)
            {
                Console.WriteLine($"{item.ItemType} 类型未处理");
                return true;
            }

            if (!ItemNewHelper.CheckValiedItem(item))
            {
                return true;
            }

            if (item.ItemType != ItemBigType.Type_Item)
            {
                return false;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(item.ItemID);
            if (ldItem.IfAutoUse != 1 && ldItem.IfBag != 0)
            {
                return false;
            }

            if (ldItem.IfAutoUse == 1)
            {
                int useTimes = item.ItemNum > 0 ? item.ItemNum : 1;
                for (int n = 0; n < useTimes; n++)
                {
                    ItemUseHelper.UseItem(unit, item.ItemID, null, null, out _);
                }
            }

            if (notice)
            {
                ItemAddHelper.OnGetItem(unit, getType, item.ItemType, item.ItemID, item.ItemNum);
            }

            return true;
        }

        private static bool TryReserveBagCell(BagComponentServer self, RewardItem item, ItemLocType specLocType, Dictionary<int, int> leftCellByLoc)
        {
            ItemLocType toLocType = ResolveAddItemLoc(specLocType, item);
            List<BagInfo> locList = self.GetItemByLoc(toLocType);
            if (locList == null)
            {
                Log.Error($"OnAddItemData invalid loc={(int)toLocType} item={item.ItemID}");
                return false;
            }

            int locKey = (int)toLocType;
            if (!leftCellByLoc.TryGetValue(locKey, out int leftCell))
            {
                leftCell = self.GetBagLeftCell(locKey);
            }

            int needCells = ItemNewHelper.CalcNeedNewCells(locList, item.ItemType, item.ItemID, item.ItemNum, ItemNewHelper.GetNewItemPileSum(item));
            if (needCells > leftCell)
            {
                return false;
            }

            leftCellByLoc[locKey] = leftCell - needCells;
            return true;
        }

        private static void InsertBagItems(BagComponentServer self, Unit unit, List<RewardItem> bagItems, string makeUserID, string getWay, int getType, bool notice, ItemLocType specLocType)
        {
            M2C_RoleBagUpdate bagUpdate = self.message;
            bagUpdate.BagInfoAdd.Clear();
            bagUpdate.BagInfoUpdate.Clear();
            bagUpdate.BagInfoDelete.Clear();

            for (int i = 0; i < bagItems.Count; i++)
            {
                RewardItem item = bagItems[i];
                int maxPileSum = ItemNewHelper.GetNewItemPileSum(item);
                ItemLocType toLocType = ResolveAddItemLoc(specLocType, item);
                List<BagInfo> itemList = self.GetItemByLoc(toLocType);
                int leftNum = FillExistPile(itemList, item, maxPileSum, bagUpdate);
                CreateNewCells(itemList, item, leftNum, maxPileSum, toLocType, makeUserID, getWay, bagUpdate);
                BagSortHelper.SortIfNeeded(itemList, toLocType);
                if (notice)
                {
                    ItemAddHelper.OnGetItem(unit, getType, item.ItemType, item.ItemID, item.ItemNum);
                }
            }

            if (notice)
            {
                MessageHelper.SendToClient(unit, bagUpdate);
            }
        }

        private static int FillExistPile(List<BagInfo> itemList, RewardItem item, int maxPileSum, M2C_RoleBagUpdate bagUpdate)
        {
            int leftNum = item.ItemNum;
            for (int k = 0; k < itemList.Count && leftNum > 0; k++)
            {
                BagInfo bagInfo = itemList[k];
                if (bagInfo.ItemID != item.ItemID || bagInfo.ItemType != item.ItemType || bagInfo.ItemNum >= maxPileSum)
                {
                    continue;
                }

                int newNum = leftNum + bagInfo.ItemNum;
                if (newNum > maxPileSum)
                {
                    leftNum = newNum - maxPileSum;
                    newNum = maxPileSum;
                }
                else
                {
                    leftNum = 0;
                }

                bagInfo.ItemNum = newNum;
                bagUpdate.BagInfoUpdate.Add(bagInfo);
            }

            return leftNum;
        }

        private static void CreateNewCells(List<BagInfo> itemList, RewardItem item, int leftNum, int maxPileSum, ItemLocType toLocType, string makeUserID, string getWay, M2C_RoleBagUpdate bagUpdate)
        {
            while (leftNum > 0)
            {
                BagInfo bagInfo = new BagInfo
                {
                    ItemType = item.ItemType,
                    ItemID = item.ItemID,
                    ItemNum = leftNum > maxPileSum ? maxPileSum : leftNum,
                    Loc = (int)toLocType,
                    BagInfoID = IdGenerater.Instance.GenerateId(),
                    GetWay = getWay,
                    MakePlayer = makeUserID
                };
                leftNum -= bagInfo.ItemNum;
                bagInfo.SetBinding(ItemNewHelper.CheckItemIfBound(item));
                if (item.ItemType == ItemBigType.Type_Equip && bagInfo.BaseAttrList.Count <= 0)
                {
                    LDEquip equipConfig = LDEquipCategory.Instance.Get(item.ItemID);
                    bagInfo.EnhanceLevel = RandomHelper.RandomNumber(0, equipConfig.Enhance);
                    bagInfo.BaseAttrList = LDEquipCategory.Instance.GetEquipAttribute(item.ItemID);
                    ItemNewHelper.SortBaseAttrList(bagInfo.BaseAttrList);
                }

                itemList.Add(bagInfo);
                bagUpdate.BagInfoAdd.Add(bagInfo);
            }
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
        public static bool OnCostItemData(this BagComponentServer self, long uid, int number, ItemLocType itemLocType = ItemLocType.ItemLocBag)
        {
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            List<BagInfo> ItemTypeList = self.GetItemByLoc(itemLocType);
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
                int itemType = costItems[i].ItemType > 0 ? costItems[i].ItemType : ItemBigType.Type_Item;
                int itemID = costItems[i].ItemID;
                int itemNum = costItems[i].ItemNum;
                int userDataType = ItemNewHelper.GetItemToUserDataType(itemType, itemID);
                if (userDataType != UserDataType.None)
                {
                    roleInfo.UpdateRoleData(userDataType, (-itemNum).ToString(), true, itemGetWay);
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
                
                //极品属性
                //强化登录（List长度13， 13个位置）
                int caowei = ItemNewHelper.GetNewEquipCaoWei(equipList[i].ItemID);
                int qianghuaLv = self.GetQiangHuaLevel(caowei);

                occInitAttribute.AddRange( equipList[i].BaseAttrList );

                //获取宝石属性


                List<int> gemList = equipList[i].GemIdList;

                for (int z = 0; z < gemList.Count; z++)
                {
                    int gemID = gemList[z];
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