using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetEggChouKaHandler : AMActorLocationRpcHandler<Unit, C2M_PetEggChouKaRequest, M2C_PetEggChouKaResponse>
    {
        private static string[] CachedExploreDiscountSet;
        private static string[] CachedChouKaConfigParts;
        private static string[] CachedTenChouKaConfigParts;

        private static void EnsureChouKaConfigCache()
        {
            if (CachedExploreDiscountSet != null)
            {
                return;
            }
            CachedExploreDiscountSet = LDGlobalValueCategory.Instance.Get(107).Value.Split(';');
            CachedChouKaConfigParts = LDGlobalValueCategory.Instance.Get(39).Value.Split('@');
            CachedTenChouKaConfigParts = LDGlobalValueCategory.Instance.Get(40).Value.Split('@');
        }

        protected override async ETTask Run(Unit unit, C2M_PetEggChouKaRequest request, M2C_PetEggChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            EnsureChouKaConfigCache();
            string[] exploreDiscountSet = CachedExploreDiscountSet;

            if (bagComponentServer.GetBagLeftCell() < request.ChouKaType)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            if(request.ChouKaType!=1 && request.ChouKaType!= 10)
            {
                Log.Error($"C2M_PetEggChouKaRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }


            reply();
            await ETTask.CompletedTask;
        }
    }
}
