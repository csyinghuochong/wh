using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionListHandler : AMActorRpcHandler<Scene, C2U_UnionListRequest, U2C_UnionListResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionListRequest request, U2C_UnionListResponse response, Action reply)
        {
            UnionSceneComponent unionScene = scene.GetComponent<UnionSceneComponent>();
            await unionScene.LoadAllUnionInfos();

            unionScene.CollectUnionList(response.UnionList, null);

            reply();
        }
    }
}
