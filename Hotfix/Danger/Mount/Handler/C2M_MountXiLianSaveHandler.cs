using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountXiLianSaveHandler : AMActorLocationRpcHandler<Unit, C2M_MountXiLianSave, M2C_MountXiLianSave>
    {
        protected override async ETTask Run(Unit unit, C2M_MountXiLianSave request, M2C_MountXiLianSave response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            if (!MountHelper.SaveXiLianResult(mountInfo))
            {
                response.Error = ErrorCode.ERR_Mount_NoXiLianResult;
                reply();
                return;
            }

            MountHelper.EnsureAddPoint(mountInfo);
            MountHelper.ApplyAptitudeAttributes(mountInfo);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            mountComponentServer.RefreshPlayerMountAttrs();
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
