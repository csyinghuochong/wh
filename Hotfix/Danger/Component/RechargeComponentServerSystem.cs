namespace ET
{
    
    
    public static  class RechargeComponentServerSystem
    {


        public static void  OnRecharge(this  RechargeComponent self, int rechargeNum)
        {

            self.RechargePro.LastRechargeTime = TimeHelper.ServerNow();

            self.RechargePro.TotalRechargeNum += rechargeNum;
            
            self.SendRechargeUpdate();

        }

        public static void SendRechargeUpdate(this  RechargeComponent self)
        {
            M2C_RechargeUpdate m2CRecharge = new M2C_RechargeUpdate();
            m2CRecharge.RechargePro = self.RechargePro;
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2CRecharge );
        }


    }
}