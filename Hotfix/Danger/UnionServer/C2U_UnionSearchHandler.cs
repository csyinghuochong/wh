using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionSearchHandler : AMActorRpcHandler<Scene, C2U_UnionSearchRequest, U2C_UnionSearchResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionSearchRequest request, U2C_UnionSearchResponse response, Action reply)
        {
            UnionSceneComponent unionScene = scene.GetComponent<UnionSceneComponent>();
            await unionScene.LoadAllUnionInfos();
            unionScene.CollectUnionList(response.UnionList, request.Keyword);
            reply();
        }
    }
}
