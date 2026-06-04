using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class M2R_SerialQueryHandler : AMActorRpcHandler<Scene, M2R_SerialQueryRequest, R2M_SerialQueryResponse>
    {
        protected override async ETTask Run(Scene scene, M2R_SerialQueryRequest request, R2M_SerialQueryResponse response, Action reply)
        {
            CenterServerComponent accountCenterComponent = scene.GetComponent<CenterServerComponent>();
            (int , int) itemvalue = accountCenterComponent.GetSerialKeyId(request.SerialNumber);
            response.SerialIndex = itemvalue.Item1;
            response.IsRewarded = itemvalue.Item2;

            reply();
            await ETTask.CompletedTask;
        }
    }
}