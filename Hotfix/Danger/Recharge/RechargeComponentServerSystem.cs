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

        public static void NotifyClient(this RechargeComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            MessageHelper.SendToClient(unit, new M2C_RechargeUpdate()
            {
                RechargePro = self.RechargePro,
            });
        }

        public static void OnLogin(this RechargeComponentServer self)
        {
            self.NotifyClient();
        }
    }
}
