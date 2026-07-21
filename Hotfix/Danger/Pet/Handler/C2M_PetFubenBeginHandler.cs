using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetFubenBeginHandler : AMActorLocationRpcHandler<Unit, C2M_PetFubenBeginRequest, M2C_PetFubenBeginResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetFubenBeginRequest request, M2C_PetFubenBeginResponse response, Action reply)
        {
            Scene domainScene = unit.DomainScene();
            UnitComponent unitComponent = domainScene.GetComponent<UnitComponent>();
            List<Unit> allunits = unitComponent.GetAll();
            for (int i = 0; i < allunits.Count; i++)
            {
                Unit sceneUnit = allunits[i];
                if (sceneUnit.Type!= UnitType.Pet && sceneUnit.Type!= UnitType.Monster)
                {
                    continue;
                }
                sceneUnit.GetComponent<AIComponent>().Begin();
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
