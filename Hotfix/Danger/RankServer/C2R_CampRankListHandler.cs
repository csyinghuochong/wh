using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2R_CampRankListHandler : AMActorRpcHandler<Scene, C2R_CampRankListRequest, R2C_CampRankListResponse>
    {
        protected override async ETTask Run(Scene scene, C2R_CampRankListRequest request, R2C_CampRankListResponse response, Action reply)
        {
            RankSceneComponent rankComponent = scene.GetComponent<RankSceneComponent>();
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
