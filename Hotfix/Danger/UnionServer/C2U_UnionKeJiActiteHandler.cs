using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionKeJiActiteHandler : AMActorRpcHandler<Scene, C2U_UnionKeJiActiteRequest, U2C_UnionKeJiActiteResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionKeJiActiteRequest request, U2C_UnionKeJiActiteResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            int keijiId = dBUnionInfo.UnionInfo.UnionKeJiList[request.Position];
          
            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            reply();
        }
    }
}
