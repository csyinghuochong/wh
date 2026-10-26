using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountAddPointHandler : AMActorLocationRpcHandler<Unit, C2M_MountAddPoint, M2C_MountAddPoint>
    {
        protected override async ETTask Run(Unit unit, C2M_MountAddPoint request, M2C_MountAddPoint response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            MountHelper.TryApplyAddPoint(mountInfo, request.AddPropretyValue);
            MountHelper.ApplyAptitudeAttributes(mountInfo);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            mountComponentServer.RefreshPlayerMountAttrs();
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
