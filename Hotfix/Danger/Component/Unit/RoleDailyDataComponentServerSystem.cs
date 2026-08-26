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
            data.DayFubenTimes ??= new List<KeyValuePairInt>();
            data.ChouKaRewardIds ??= new List<int>();
            data.MysteryItems ??= new List<KeyValuePairInt>();
            data.DayItemUse ??= new List<KeyValuePairInt>();
            data.DayMonsters ??= new List<KeyValuePairInt>();
            data.DayJingLing ??= new List<int>();
            data.BuyStoreItems ??= new List<KeyValuePairInt>();
            self.PersonalRandomShops ??= new Dictionary<int, List<ShopGoodsItem>>();
        }

        public static void OnDailyReset(this RoleDailyDataComponentServer self, bool notice = false)
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
                return;
            }

            // 默认日清：不含周活跃
            data.DayFubenTimes.Clear();
            data.ChouKaRewardIds.Clear();
            data.MysteryItems.Clear();
            data.DayItemUse.Clear();
            data.DayMonsters.Clear();
            data.DayJingLing.Clear();
            data.BuyStoreItems.Clear();
            data.DailyActivePoint = 0;
            data.RechargeSign = 0;
            data.TeamDungeonTimes = 0;
            data.HongBao = 0;
            data.NowXiLian = 0;
            data.YueKaAwardTime = 0;
            data.TiLiKillNumber = 0;
            data.ChouKaNumber = 0;
            data.HappyMoveNumber = 0;
            self.PersonalRandomShops.Clear();
        }

        /// <summary>增加日/周活跃点数并推送</summary>
        public static void AddActivePoint(this RoleDailyDataComponentServer self, int userDataType, int add, bool notice = true)
        {
            if (add <= 0)
            {
                return;
            }

            if (userDataType == UserDataType.DailyActive)
            {
                self.Data.DailyActivePoint += add;
            }
            else if (userDataType == UserDataType.WeeklyActive)
            {
                self.Data.WeeklyActivePoint += add;
            }
            else
            {
                return;
            }

            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        public static int GetDailyActivePoint(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().DailyActivePoint;
        }

        public static int GetWeeklyActivePoint(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().WeeklyActivePoint;
        }

        /// <summary>
        /// 个人随机商店（Type 2/3）货架：当日首次打开时生成，零点 Clear 后重新生成。
        /// </summary>
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

        public static int GetCount(List<KeyValuePairInt> list, int keyId)
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

        public static void AddCount(List<KeyValuePairInt> list, int keyId, int add)
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

            list.Add(new KeyValuePairInt { KeyId = keyId, Value = add });
        }

        public static void SetCount(List<KeyValuePairInt> list, int keyId, long value)
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

            list.Add(new KeyValuePairInt { KeyId = keyId, Value = value });
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

        #region 神秘店 MysteryItems

        public static int GetMysteryBuy(this RoleDailyDataComponentServer self, int mysteryId)
        {
            return GetCount(self.GetDailyData().MysteryItems, mysteryId);
        }

        public static void OnMysteryBuy(this RoleDailyDataComponentServer self, int mysteryId)
        {
            AddCount(self.GetDailyData().MysteryItems, mysteryId, 1);
        }

        #endregion

        #region 每日道具 DayItemUse

        public static int GetDayItemUse(this RoleDailyDataComponentServer self, int itemId)
        {
            return GetCount(self.GetDailyData().DayItemUse, itemId);
        }

        public static void OnDayItemUse(this RoleDailyDataComponentServer self, int itemId)
        {
            AddCount(self.GetDailyData().DayItemUse, itemId, 1);
        }

        #endregion

        #region 抽卡奖励 ChouKaRewardIds

        public static bool HasChouKaReward(this RoleDailyDataComponentServer self, int rewardId)
        {
            return self.GetDailyData().ChouKaRewardIds.Contains(rewardId);
        }

        public static void AddChouKaReward(this RoleDailyDataComponentServer self, int rewardId)
        {
            List<int> list = self.GetDailyData().ChouKaRewardIds;
            if (!list.Contains(rewardId))
            {
                list.Add(rewardId);
            }
        }

        #endregion

        #region 商店限购

        public static int GetBuyStorePeriod(this RoleDailyDataComponentServer self, int goodsId)
        {
            return GetCount(self.GetDailyData().BuyStoreItems, goodsId);
        }

        public static int GetBuyStoreForever(this RoleDailyDataComponentServer self, int goodsId)
        {
            RoleInfo roleInfo = self.GetParent<Unit>()?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            if (roleInfo == null)
            {
                return 0;
            }

            roleInfo.BuyStoreItemsForever ??= new List<KeyValuePairInt>();
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
                    roleInfo.BuyStoreItemsForever ??= new List<KeyValuePairInt>();
                    AddCount(roleInfo.BuyStoreItemsForever, goodsId, buyNumber);
                }
            }

            self.NotifyUpdate(RoleDailyDataComponentServer.ReasonShopLimit);
        }

        #endregion

        #region 签到充值 RechargeSign 0不能领取 1可以领取 2已领取

        public static int GetRechargeSign(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().RechargeSign;
        }

        public static void SetRechargeSign(this RoleDailyDataComponentServer self, int value, bool notice = true)
        {
            self.GetDailyData().RechargeSign = value;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 组队副本次数 TeamDungeonTimes

        public static int GetTeamDungeonTimes(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().TeamDungeonTimes;
        }

        public static void AddTeamDungeonTimes(this RoleDailyDataComponentServer self, bool notice = true)
        {
            self.GetDailyData().TeamDungeonTimes++;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 红包 HongBao 0未领 1已领

        public static int GetHongBao(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().HongBao;
        }

        public static void SetHongBao(this RoleDailyDataComponentServer self, int value, bool notice = true)
        {
            self.GetDailyData().HongBao = value;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 今日洗练 NowXiLian

        public static int GetNowXiLian(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().NowXiLian;
        }

        public static void AddNowXiLian(this RoleDailyDataComponentServer self, int add = 1, bool notice = true)
        {
            self.GetDailyData().NowXiLian += add;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 月卡今日领取 YueKaAwardTime

        public static int GetYueKaAwardTime(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().YueKaAwardTime;
        }

        public static void SetYueKaAwardTime(this RoleDailyDataComponentServer self, int value, bool notice = true)
        {
            self.GetDailyData().YueKaAwardTime = value;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 体力击杀计数 TiLiKillNumber

        public static int GetTiLiKillNumber(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().TiLiKillNumber;
        }

        public static void SetTiLiKillNumber(this RoleDailyDataComponentServer self, int value, bool notice = false)
        {
            self.GetDailyData().TiLiKillNumber = value;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        public static void AddTiLiKillNumber(this RoleDailyDataComponentServer self, bool notice = false)
        {
            self.GetDailyData().TiLiKillNumber++;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 今日抽卡 ChouKaNumber

        public static int GetChouKaNumber(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().ChouKaNumber;
        }

        public static void AddChouKaNumber(this RoleDailyDataComponentServer self, int add = 1, bool notice = true)
        {
            self.GetDailyData().ChouKaNumber += add;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        #endregion

        #region 欢乐副本移动 HappyMoveNumber

        public static int GetHappyMoveNumber(this RoleDailyDataComponentServer self)
        {
            return self.GetDailyData().HappyMoveNumber;
        }

        public static void AddHappyMoveNumber(this RoleDailyDataComponentServer self, bool notice = true)
        {
            self.GetDailyData().HappyMoveNumber++;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
            }
        }

        public static void SetHappyMoveNumber(this RoleDailyDataComponentServer self, int value, bool notice = true)
        {
            self.GetDailyData().HappyMoveNumber = value;
            if (notice)
            {
                self.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
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

        public static void NotifyUpdate(this RoleDailyDataComponentServer self, int reason)
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
                ChouKaRewardIds = src.ChouKaRewardIds != null ? new List<int>(src.ChouKaRewardIds) : new List<int>(),
                MysteryItems = CloneKvList(src.MysteryItems),
                DayItemUse = CloneKvList(src.DayItemUse),
                DayMonsters = CloneKvList(src.DayMonsters),
                DayJingLing = src.DayJingLing != null ? new List<int>(src.DayJingLing) : new List<int>(),
                BuyStoreItems = CloneKvList(src.BuyStoreItems),
                DailyActivePoint = src.DailyActivePoint,
                WeeklyActivePoint = src.WeeklyActivePoint,
                RechargeSign = src.RechargeSign,
                TeamDungeonTimes = src.TeamDungeonTimes,
                HongBao = src.HongBao,
                NowXiLian = src.NowXiLian,
                YueKaAwardTime = src.YueKaAwardTime,
                TiLiKillNumber = src.TiLiKillNumber,
                ChouKaNumber = src.ChouKaNumber,
                HappyMoveNumber = src.HappyMoveNumber,
            };
        }

        private static List<KeyValuePairInt> CloneKvList(List<KeyValuePairInt> src)
        {
            List<KeyValuePairInt> list = new List<KeyValuePairInt>();
            if (src == null)
            {
                return list;
            }

            for (int i = 0; i < src.Count; i++)
            {
                list.Add(new KeyValuePairInt { KeyId = src[i].KeyId, Value = src[i].Value });
            }

            return list;
        }
    }
}
