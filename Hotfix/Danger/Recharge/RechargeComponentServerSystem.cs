using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class RechargeComponentAwakeSystem : AwakeSystem<RechargeComponentServer>
    {
        public override void Awake(RechargeComponentServer self)
        {
            self.InitRechargePro();
        }
    }

    [ObjectSystem]
    public class RechargeComponentDeserializeSystem : DeserializeSystem<RechargeComponentServer>
    {
        public override void Deserialize(RechargeComponentServer self)
        {
            self.InitRechargePro();
        }
    }

    public static class RechargeComponentServerSystem
    {
        /// <summary>仅 Awake / Deserialize 调用，业务接口不要再补列表。</summary>
        public static void InitRechargePro(this RechargeComponentServer self)
        {
            self.RechargePro ??= new RechargePro();
            self.PaidOrderIds ??= new List<string>();
            VipHelp.EnsureLists(self.RechargePro);
        }

        public static bool HasFirstBuy(this RechargeComponentServer self, int payId)
        {
            return self.RechargePro.FirstBuyPayIds.Contains(payId);
        }

        public static void AddFirstBuy(this RechargeComponentServer self, int payId)
        {
            if (!self.RechargePro.FirstBuyPayIds.Contains(payId))
            {
                self.RechargePro.FirstBuyPayIds.Add(payId);
            }
        }

        public static long GetTotalRechargeNum(this RechargeComponentServer self)
        {
            return self.RechargePro.TotalRechargeNum;
        }

        /// <param name="rewardList">本次发放的原始列表。不要传入合并后的结果。</param>
        public static void NotifyClient(this RechargeComponentServer self, List<RewardItem> rewardList = null)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            M2C_RechargeUpdate message = new M2C_RechargeUpdate()
            {
                RechargePro = self.RechargePro,
            };
            if (rewardList != null)
            {
                message.RewardList.AddRange(rewardList);
            }

            MessageHelper.SendToClient(unit, message);
        }

        public static void OnLogin(this RechargeComponentServer self)
        {
            self.NotifyClient();
        }
    }
}
