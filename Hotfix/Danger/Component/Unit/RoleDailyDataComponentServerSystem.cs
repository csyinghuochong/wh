using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class RoleDailyDataComponentAwakeSystem : AwakeSystem<RoleDailyDataComponentServer>
    {
        public override void Awake(RoleDailyDataComponentServer self)
        {
            self.InitLists();
        }
    }

    [ObjectSystem]
    public class RoleDailyDataComponentDeserializeSystem : DeserializeSystem<RoleDailyDataComponentServer>
    {
        public override void Deserialize(RoleDailyDataComponentServer self)
        {
            self.InitLists();
        }
    }

    public static class RoleDailyDataComponentServerSystem
    {
        /// <summary>仅 Awake / Deserialize 调用，业务接口不要再补列表。</summary>
        public static void InitLists(this RoleDailyDataComponentServer self)
        {
            RoleDailyData data = self.Data ??= new RoleDailyData();
            data.DayFubenTimes ??= new List<IntLongPair>();
            data.BuyStoreItems ??= new List<IntLongPair>();
            self.PersonalRandomShops ??= new Dictionary<int, List<ShopGoodsItem>>();
            self.ShopRefreshTime ??= new Dictionary<int, long>();
        }

        public static void OnDailyReset(this RoleDailyDataComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.Type != UnitType.Player)
            {
                return;
            }

            self.ClearDayLists(RoleDailyClearType.Day);
        }

        /// <summary>
        /// 按类型清理：Day=日清字段+日活跃；Week=周活跃。
        /// </summary>
        public static void ClearDayLists(this RoleDailyDataComponentServer self, int clearType = RoleDailyClearType.Day)
        {
            RoleDailyData data = self.GetDailyData();

            if (clearType == RoleDailyClearType.Week)
            {
                data.WeeklyActivePoint = 0;
                SetCount(data.Currencies, UserDataType.WeeklyActive, 0);
                return;
            }

            // 日清不含商店。本次购买数量按各商店 Auto_Refresh 清，终身购买数量不清。
            data.DayFubenTimes.Clear();
            data.DailyActivePoint = 0;
            SetCount(data.Currencies, UserDataType.DailyActive, 0);
            data.VipDailyClaimed = 0;
        }

        /// <summary>增加日/周活跃点数并推送</summary>
        public static void AddActivePoint(this RoleDailyDataComponentServer self, int userDataType, int add, bool notice = true)
        {
            if (add <= 0)
            {
                return;
            }

            RoleDailyData data = self.GetDailyData();

            if (userDataType == UserDataType.DailyActive)
            {
                data.DailyActivePoint += add;
                AddCount(data.Currencies, UserDataType.DailyActive, add);
            }
            else if (userDataType == UserDataType.WeeklyActive)
            {
                data.WeeklyActivePoint += add;
                AddCount(data.Currencies, UserDataType.WeeklyActive, add);
            }
            else
            {
                return;
            }

            if (notice)
            {
                self.NotifyUpdate();
            }
        }

        public static int GetDailyActivePoint(this RoleDailyDataComponentServer self)
        {
            RoleDailyData data = self.GetDailyData();
            int fromDict = GetCount(data.Currencies, UserDataType.DailyActive);
            return fromDict != 0 || HasCount(data.Currencies, UserDataType.DailyActive)
                    ? fromDict
                    : data.DailyActivePoint;
        }

        public static int GetWeeklyActivePoint(this RoleDailyDataComponentServer self)
        {
            RoleDailyData data = self.GetDailyData();
            int fromDict = GetCount(data.Currencies, UserDataType.WeeklyActive);
            return fromDict != 0 || HasCount(data.Currencies, UserDataType.WeeklyActive)
                    ? fromDict
                    : data.WeeklyActivePoint;
        }

        public static bool HasCount(List<IntLongPair> list, int keyId)
        {
            if (list == null)
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].KeyId == keyId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 请求时刷新。到点清该店本次购买数量；Type 2/3 重抽货架。
        /// Type 9 只清玩家本次购买数量，货架在 ActivityScene。
        /// </summary>
        public static void TryRefreshShop(this RoleDailyDataComponentServer self, int shopId)
        {
            if (!LDShopCategory.Instance.Contain(shopId))
            {
                return;
            }

            LDShop shop = LDShopCategory.Instance.Get(shopId);
            long now = TimeHelper.ServerNow();
            self.ShopRefreshTime.TryGetValue(shopId, out long last);
            bool refresh = ActivityHelper.IsShopRefreshDue(shop.Auto_Refresh, last, now);
            if (last > 0 && !refresh)
            {
                return;
            }

            if (refresh && self.ClearShopPeriodBuys(shopId))
            {
                self.NotifyUpdate();
            }

            self.EnsurePersonalShelf(shop, refresh);
            self.ShopRefreshTime[shopId] = now;
        }

        /// <summary>清空该商店商品的本次购买数量。终身购买数量不动。</summary>
        public static bool ClearShopPeriodBuys(this RoleDailyDataComponentServer self, int shopId)
        {
            List<LDShop_Goods> goodsList = LDShop_GoodsCategory.Instance.GetShopGoodsList(shopId);
            HashSet<int> goodsIds = new HashSet<int>();
            for (int i = 0; i < goodsList.Count; i++)
            {
                goodsIds.Add(goodsList[i].Id);
            }

            List<IntLongPair> buys = self.GetDailyData().BuyStoreItems;
            return buys.RemoveAll(pair => goodsIds.Contains(pair.KeyId)) > 0;
        }

        /// <summary>Type 2/3 个人货架。forceRegen 为 true 时按刷新重抽。</summary>
        private static void EnsurePersonalShelf(this RoleDailyDataComponentServer self, LDShop shop, bool forceRegen)
        {
            if (shop.Type != ShopType.RandomRepeat && shop.Type != ShopType.RandomUnique)
            {
                return;
            }

            if (!forceRegen
                && self.PersonalRandomShops.TryGetValue(shop.Id, out List<ShopGoodsItem> list)
                && list != null
                && list.Count > 0)
            {
                return;
            }

            self.PersonalRandomShops[shop.Id] = RandomShopHelper.InitShopItemInfos(shop.Id);
        }

        /// <summary>个人随机商店（Type 2/3）货架。先走 TryRefreshShop，再取已生成的列表。</summary>
        public static List<ShopGoodsItem> GetOrInitPersonalRandomShop(this RoleDailyDataComponentServer self, int shopId)
        {
            if (self.PersonalRandomShops.TryGetValue(shopId, out List<ShopGoodsItem> list)
                && list != null
                && list.Count > 0)
            {
                return list;
            }

            list = RandomShopHelper.InitShopItemInfos(shopId);
            self.PersonalRandomShops[shopId] = list;
            return list;
        }

        public static RoleDailyData GetDailyData(this RoleDailyDataComponentServer self)
        {
            return self.Data ??= new RoleDailyData();
        }

        public static int GetCount(List<IntLongPair> list, int keyId)
        {
            if (list == null)
            {
                return 0;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].KeyId == keyId)
                {
                    return (int)list[i].Value;
                }
            }

            return 0;
        }

        public static void AddCount(List<IntLongPair> list, int keyId, int add)
        {
            if (list == null || add <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].KeyId == keyId)
                {
                    list[i].Value += add;
                    return;
                }
            }

            list.Add(new IntLongPair { KeyId = keyId, Value = add });
        }

        public static void SetCount(List<IntLongPair> list, int keyId, long value)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].KeyId == keyId)
                {
                    list[i].Value = value;
                    return;
                }
            }

            list.Add(new IntLongPair { KeyId = keyId, Value = value });
        }

        #region 副本次数 DayFubenTimes

        public static long GetSceneFubenTimes(this RoleDailyDataComponentServer self, int sceneId)
        {
            return GetCount(self.GetDailyData().DayFubenTimes, sceneId);
        }

        public static void AddSceneFubenTimes(this RoleDailyDataComponentServer self, int sceneId)
        {
            AddCount(self.GetDailyData().DayFubenTimes, sceneId, 1);
        }

        public static void ClearFubenTimes(this RoleDailyDataComponentServer self, int sceneId)
        {
            SetCount(self.GetDailyData().DayFubenTimes, sceneId, 0);
        }

        /// <summary>扣减副本次数（AddFubenTimes 命名沿用旧接口）。</summary>
        public static void AddFubenTimes(this RoleDailyDataComponentServer self, int sceneId, int times)
        {
            long cur = GetCount(self.GetDailyData().DayFubenTimes, sceneId) - times;
            if (cur < 0)
            {
                cur = 0;
            }

            SetCount(self.GetDailyData().DayFubenTimes, sceneId, cur);
        }

        #endregion


        #region 商店限购

        /// <summary>本次购买数量。本刷新周期已购，商店按 Auto_Refresh 刷新时清空。上限 Limit_Num。</summary>
        public static int GetBuyStorePeriod(this RoleDailyDataComponentServer self, int goodsId)
        {
            return GetCount(self.GetDailyData().BuyStoreItems, goodsId);
        }

        /// <summary>终身购买数量。不随商店刷新清空。上限 Limit_Num_Forever。</summary>
        public static int GetBuyStoreForever(this RoleDailyDataComponentServer self, int goodsId)
        {
            RoleInfo roleInfo = self.GetParent<Unit>()?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            if (roleInfo == null)
            {
                return 0;
            }

            roleInfo.BuyStoreItemsForever ??= new List<IntLongPair>();
            return GetCount(roleInfo.BuyStoreItemsForever, goodsId);
        }

        /// <summary>增加本次+终身购买次数，并推送 Update。</summary>
        public static void AddShopBuy(this RoleDailyDataComponentServer self, int goodsId, int buyNumber, bool period, bool forever)
        {
            if (buyNumber <= 0)
            {
                return;
            }

            if (period)
            {
                AddCount(self.GetDailyData().BuyStoreItems, goodsId, buyNumber);
            }

            if (forever)
            {
                RoleInfo roleInfo = self.GetParent<Unit>()?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
                if (roleInfo != null)
                {
                    roleInfo.BuyStoreItemsForever ??= new List<IntLongPair>();
                    AddCount(roleInfo.BuyStoreItemsForever, goodsId, buyNumber);
                }
            }

            self.NotifyUpdate();
        }

        #endregion

        #region 组队副本次数 TeamDungeonTimes

        public static int GetTeamDungeonTimes(this RoleDailyDataComponentServer self)
        {
            return 0;
        }

        public static void AddTeamDungeonTimes(this RoleDailyDataComponentServer self, bool notice = true)
        {
            
            if (notice)
            {
                self.NotifyUpdate();
            }
        }

        #endregion

        #region VIP专属福利今日领取 VipDailyClaimed 0未领 1已领

        public static int GetVipDailyClaimed(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().VipDailyClaimed;
        }

        public static void SetVipDailyClaimed(this RoleDailyDataComponentServer self, int value, bool notice = true)
        {
            self.GetDailyData().VipDailyClaimed = value;
            if (notice)
            {
                self.NotifyUpdate();
            }
        }

        #endregion



        public static void OnLogin(this RoleDailyDataComponentServer self)
        {
            // 全量由客户端 LoginHelper 请求 C2M_RoleDailyDataRequest，不再登录主动推 Init
        }

        public static void FillInitResponse(this RoleDailyDataComponentServer self, M2C_RoleDailyDataInit response)
        {
            RoleInfo roleInfo = self.GetParent<Unit>()?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            response.Data = self.CloneDailyData();
            response.BuyStoreItemsForever = CloneKvList(roleInfo?.BuyStoreItemsForever);
            response.Error = ErrorCode.ERR_Success;
        }

        public static void NotifyInit(this RoleDailyDataComponentServer self)
        {
            // 保留空实现避免旧调用编译失败；请走 C2M_RoleDailyDataRequest
        }

        public static void NotifyUpdate(this RoleDailyDataComponentServer self, int reason = 0)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.GetComponent<UnitGateComponent>() == null)
            {
                return;
            }

            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            M2C_RoleDailyDataUpdate msg = new M2C_RoleDailyDataUpdate
            {
                Data = self.CloneDailyData(),
                BuyStoreItemsForever = CloneKvList(roleInfo?.BuyStoreItemsForever),
                Reason = reason,
            };
            MessageHelper.SendToClient(unit, msg);
        }

        private static RoleDailyData CloneDailyData(this RoleDailyDataComponentServer self)
        {
            RoleDailyData src = self.GetDailyData();
            return new RoleDailyData
            {
                DayFubenTimes = CloneKvList(src.DayFubenTimes),
                BuyStoreItems = CloneKvList(src.BuyStoreItems),
                DailyActivePoint = src.DailyActivePoint,
                WeeklyActivePoint = src.WeeklyActivePoint,
                VipDailyClaimed = src.VipDailyClaimed,
            };
        }

        private static List<IntLongPair> CloneKvList(List<IntLongPair> src)
        {
            List<IntLongPair> list = new List<IntLongPair>();
            if (src == null)
            {
                return list;
            }

            for (int i = 0; i < src.Count; i++)
            {
                list.Add(new IntLongPair { KeyId = src[i].KeyId, Value = src[i].Value });
            }

            return list;
        }
    }
}
