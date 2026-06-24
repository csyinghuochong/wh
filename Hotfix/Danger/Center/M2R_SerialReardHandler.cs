using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class M2R_SerialReardHandler : AMActorRpcHandler<Scene, M2R_SerialReardRequest, R2M_SerialReardResponse>
    {
        protected override async ETTask Run(Scene scene, M2R_SerialReardRequest request, R2M_SerialReardResponse response, Action reply)
        {
            CenterServerComponent accountCenterComponent = scene.GetComponent<CenterServerComponent>();
            response.Error = accountCenterComponent.GetSerialReward(request.SerialNumber);
         

            reply();
            await ETTask.CompletedTask;
        }
    }
}