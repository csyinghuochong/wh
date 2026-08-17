using System;
using System.Collections.Generic;

namespace ET
{
    public class C2M_TowerOfSealNextHandler: AMActorLocationRpcHandler<Unit, C2M_TowerOfSealNextRequest, M2C_TowerOfSealNextResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TowerOfSealNextRequest request, M2C_TowerOfSealNextResponse response, Action reply)
        {
            Scene domainScene = unit.DomainScene();
            TowerOfSealComponent towerOfSealComponent = domainScene.GetComponent<TowerOfSealComponent>();
            if (towerOfSealComponent == null)
            {
                reply();
                return;
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            reply();
            await ETTask.CompletedTask;
        }
    }
}