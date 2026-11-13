using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehousePasswordHandler : AMActorRpcHandler<Scene, M2U_UnionWarehousePasswordRequest, U2M_UnionWarehousePasswordResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehousePasswordRequest request, U2M_UnionWarehousePasswordResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                await ChangePassword(scene, request, response, reply);
            }
        }

        private static async ETTask ChangePassword(Scene scene, M2U_UnionWarehousePasswordRequest request, U2M_UnionWarehousePasswordResponse response, Action reply)
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
            if (UnionWarehouseHelper.GetWarehousePasswordState(oldPwd) == 2 && oldPwd != request.OldPassword)
            {
                response.Error = ErrorCode.ERR_Union_WarehousePassword;
                reply();
                return;
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                response.Error = ErrorCode.ERR_Union_WarehousePassword;
                reply();
                return;
            }

            if (oldPwd == request.NewPassword)
            {
                response.Error = ErrorCode.ERR_Union_WarehousePasswordSame;
                reply();
                return;
            }

            dBUnionInfo.WarehousePassword = request.NewPassword;
            UnionWarehouseServerHelper.RevokeAccess(scene.GetComponent<UnionSceneComponent>(), request.UnionId);
            dBUnionInfo.UnionInfo.HasWarehousePassword = UnionWarehouseHelper.GetWarehousePasswordState(dBUnionInfo.WarehousePassword);
            await DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo);
            response.HasWarehousePassword = dBUnionInfo.UnionInfo.HasWarehousePassword;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
