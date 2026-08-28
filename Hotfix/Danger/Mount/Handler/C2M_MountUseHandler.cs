using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountUseHandler : AMActorLocationRpcHandler<Unit, C2M_MountUse, M2C_MountUse>
    {
        protected override async ETTask Run(Unit unit, C2M_MountUse request, M2C_MountUse response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            mountComponentServer.SetUse(mountInfo, request.Status);
            response.UseMountId = mountComponentServer.UseMountId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
