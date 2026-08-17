using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetHeXinChouKaHandler: AMActorLocationRpcHandler<Unit, C2M_PetHeXinChouKaRequest, M2C_PetHeXinChouKaResponse>
    {
        private static string[] CachedExploreDiscountSet;
        private static string[] CachedChouKaConfigParts;
        private static string[] CachedTenChouKaConfigParts;
        private static string[] CachedTenCostItemParts;

        private static void EnsureChouKaConfigCache()
        {
            if (CachedExploreDiscountSet != null)
            {
                return;
            }
            CachedExploreDiscountSet = LDGlobalValueCategory.Instance.Get(112).Value.Split(';');
            CachedChouKaConfigParts = LDGlobalValueCategory.Instance.Get(110).Value.Split('@');
            CachedTenChouKaConfigParts = LDGlobalValueCategory.Instance.Get(111).Value.Split('@');
            CachedTenCostItemParts = CachedTenChouKaConfigParts[0].Split(';');
        }

        protected override async ETTask Run(Unit unit, C2M_PetHeXinChouKaRequest request, M2C_PetHeXinChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            EnsureChouKaConfigCache();
           
            reply();
            await ETTask.CompletedTask;
        }
    }
}