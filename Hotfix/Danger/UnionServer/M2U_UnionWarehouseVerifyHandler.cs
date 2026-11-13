using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehouseVerifyHandler : AMActorRpcHandler<Scene, M2U_UnionWarehouseVerifyRequest, U2M_UnionWarehouseVerifyResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehouseVerifyRequest request, U2M_UnionWarehouseVerifyResponse response, Action reply)
        {
            DBUnionInfo data = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (data?.UnionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            response.Error = UnionWarehouseHelper.ValidateTakePassword(data.UnionInfo, request.UnitId, data.WarehousePassword, request.Password);
            if (response.Error == ErrorCode.ERR_Success)
            {
                response.AccessToken = UnionWarehouseServerHelper.GrantAccess(scene.GetComponent<UnionSceneComponent>(), data.UnionInfo, request.UnitId);
            }
            reply();
        }
    }
}
