using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehousePasswordHandler : AMActorRpcHandler<Scene, M2U_UnionWarehousePasswordRequest, U2M_UnionWarehousePasswordResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehousePasswordRequest request, U2M_UnionWarehousePasswordResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo?.UnionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            if (dBUnionInfo.UnionInfo.LeaderId != request.UnitId)
            {
                response.Error = ErrorCode.ERR_Union_NotLeader;
                reply();
                return;
            }

            string oldPwd = dBUnionInfo.WarehousePassword ?? string.Empty;
            if (!string.IsNullOrEmpty(oldPwd) && oldPwd != (request.OldPassword ?? string.Empty))
            {
                response.Error = ErrorCode.ERR_Union_WarehousePassword;
                reply();
                return;
            }

            dBUnionInfo.WarehousePassword = request.NewPassword ?? string.Empty;
            dBUnionInfo.UnionInfo.HasWarehousePassword = string.IsNullOrEmpty(dBUnionInfo.WarehousePassword) ? 0 : 1;
            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            response.HasWarehousePassword = dBUnionInfo.UnionInfo.HasWarehousePassword;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
