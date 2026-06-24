namespace ET
{

    public static class RechargeComponentServerSystem
    {

        public static void OnLogin(this RechargeComponentServer self)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            /*int rechargeBuchang = numericComponent.GetAsInt(NumericType.RechargeBuChang);
            int rechargeType = numericComponent.GetAsInt(NumericType.RechargeType);
            numericComponent.Set(NumericType.RechargeBuChang, 0);
            numericComponent.Set(NumericType.RechargeType, 0);*/
            
        }
    }
}
