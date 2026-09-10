using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2Home_EnterHandler : AMActorRpcHandler<Scene, M2Home_EnterRequest, Home2M_EnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2Home_EnterRequest request, Home2M_EnterResponse response, Action reply)
        {
            response.FubenInstanceId = await scene.GetComponent<HomeSceneComponent>().GetHomeYuanFubenId(request.MasterId, request.UnitId);
            reply();
        }
    }
}
