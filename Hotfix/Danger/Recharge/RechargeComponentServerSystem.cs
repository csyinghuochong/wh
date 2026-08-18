using System.Collections.Generic;

namespace ET
{

    public static class RechargeComponentServerSystem
    {

        public static void EnsureRechargePro(this RechargeComponentServer self)
        {
            if (self.RechargePro == null)
            {
                self.RechargePro = new RechargePro();
            }

            self.RechargePro.FirstBuyPayIds ??= new List<int>();
        }

        public static bool HasFirstBuy(this RechargeComponentServer self, int payId)
        {
            self.EnsureRechargePro();
            return self.RechargePro.FirstBuyPayIds.Contains(payId);
        }

        public static void AddFirstBuy(this RechargeComponentServer self, int payId)
        {
            self.EnsureRechargePro();
            if (!self.RechargePro.FirstBuyPayIds.Contains(payId))
            {
                self.RechargePro.FirstBuyPayIds.Add(payId);
            }
        }

        public static void NotifyClient(this RechargeComponentServer self)
        {
            self.EnsureRechargePro();
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
            self.EnsureRechargePro();
        }
    }
}
