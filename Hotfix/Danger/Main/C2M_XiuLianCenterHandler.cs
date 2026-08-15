using System;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_XiuLianCenterHandler : AMActorLocationRpcHandler<Unit, C2M_XiuLianCenterRequest, M2C_XiuLianCenterResponse>
    {
        private static float xiuLianExpCoefficient;
        private static float xiuLianCoinCoefficient;
        private static bool xiuLianCacheInit;

        private static void EnsureXiuLianCache()
        {
            if (xiuLianCacheInit)
            {
                return;
            }

            xiuLianExpCoefficient = float.Parse(LDGlobalValueCategory.Instance.Get(29).Value);
            xiuLianCoinCoefficient = float.Parse(LDGlobalValueCategory.Instance.Get(30).Value);
            xiuLianCacheInit = true;
        }

        protected override async ETTask Run(Unit unit, C2M_XiuLianCenterRequest request, M2C_XiuLianCenterResponse response, Action reply)
        {
            EnsureXiuLianCache();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            int level = roleInfo.Lv;
            //1 经验  2金币
            if (request.XiuLianType == 1)
            {
                
            }
            else if (request.XiuLianType == 2)
            {
                
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
