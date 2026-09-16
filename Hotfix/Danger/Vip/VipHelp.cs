using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// VIP：LDVIP.Exp 为升到下一级所需累计经验（0=满级）；经验不清空。
    /// </summary>
    public static class VipHelp
    {
        public static void EnsureLists(RechargePro pro)
        {
            if (pro == null)
            {
                return;
            }

            pro.FirstBuyPayIds ??= new List<int>();
            pro.VipPrivilegeClaimed ??= new List<int>();
            pro.VipGiftBought ??= new List<int>();
        }

        public static RechargePro GetPro(Unit unit)
        {
            return unit?.GetComponent<RechargeComponentServer>()?.RechargePro;
        }

        public static LDVIP GetConfig(int vipLevel)
        {
            if (!LDVIPCategory.Instance.Contain(vipLevel))
            {
                return null;
            }

            return LDVIPCategory.Instance.Get(vipLevel);
        }

        public static int CalcLevel(long vipExp)
        {
            int level = 0;
            while (true)
            {
                LDVIP cfg = GetConfig(level);
                if (cfg == null || cfg.Exp <= 0)
                {
                    break;
                }

                if (vipExp < cfg.Exp)
                {
                    break;
                }

                level++;
            }

            return level;
        }

        public static void OnVipExpAdd(Unit unit, int addExp)
        {
            if (unit == null || addExp <= 0)
            {
                return;
            }

            RechargePro pro = GetPro(unit);
            if (pro == null)
            {
                return;
            }

            int oldLevel = pro.VipLevel;
            pro.VipExp += addExp;
            pro.VipLevel = CalcLevel(pro.VipExp);
            if (pro.VipLevel != oldLevel)
            {
                RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
                daily?.SetVipDailyClaimed(0);
            }
        }

        public static bool HasPrivilegeClaimed(RechargePro pro, int vipLevel)
        {
            return pro.VipPrivilegeClaimed.Contains(vipLevel);
        }

        public static void AddPrivilegeClaimed(RechargePro pro, int vipLevel)
        {
            if (!pro.VipPrivilegeClaimed.Contains(vipLevel))
            {
                pro.VipPrivilegeClaimed.Add(vipLevel);
            }
        }

        public static bool HasGiftBought(RechargePro pro, int vipLevel)
        {
            return pro.VipGiftBought.Contains(vipLevel);
        }

        public static void AddGiftBought(RechargePro pro, int vipLevel)
        {
            if (!pro.VipGiftBought.Contains(vipLevel))
            {
                pro.VipGiftBought.Add(vipLevel);
            }
        }

        public static int TryAddReward(Unit unit, string reward, int itemGetWay)
        {
            if (string.IsNullOrEmpty(reward))
            {
                return ErrorCode.ERR_Error;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (bag == null)
            {
                return ErrorCode.ERR_Error;
            }

            List<RewardItem> items = ItemNewHelper.GetRewardItems(reward);
            if (items.Count == 0)
            {
                return ErrorCode.ERR_Error;
            }

            if (bag.GetBagLeftCell() < ItemNewHelper.GetNeedCell(items))
            {
                return ErrorCode.ERR_BagIsFull;
            }

            bag.OnAddItemData(items, string.Empty, $"{itemGetWay}_{TimeHelper.ServerNow()}");
            return ErrorCode.ERR_Success;
        }

        public static int TryBuyGift(Unit unit, LDVIP cfg)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.Package_Pay))
            {
                return ErrorCode.ERR_Error;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (bag == null)
            {
                return ErrorCode.ERR_Error;
            }

            List<RewardItem> rewards = ItemNewHelper.GetRewardItems(cfg.Package_Pay);
            if (rewards.Count == 0)
            {
                return ErrorCode.ERR_Error;
            }

            if (bag.GetBagLeftCell() < ItemNewHelper.GetNeedCell(rewards))
            {
                return ErrorCode.ERR_BagIsFull;
            }

            List<RewardItem> costs = ItemNewHelper.GetRewardItems(cfg.Package_Consume);
            if (costs.Count > 0 && !bag.CheckNeedItem(costs))
            {
                return ErrorCode.ERR_ItemNotEnoughError;
            }

            if (costs.Count > 0 && !bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.VipGift))
            {
                return ErrorCode.ERR_ItemNotEnoughError;
            }

            bag.OnAddItemData(rewards, string.Empty, $"{ItemGetWay.VipGift}_{TimeHelper.ServerNow()}");
            return ErrorCode.ERR_Success;
        }
    }
}
