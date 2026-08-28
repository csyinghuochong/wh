using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountRNameHandler : AMActorLocationRpcHandler<Unit, C2M_MountRName, M2C_MountRName>
    {
        protected override async ETTask Run(Unit unit, C2M_MountRName request, M2C_MountRName response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            mountInfo.MountName = request.MountName;
            MessageHelper.SendToClient(unit, new M2C_MountDataUpdate()
            {
                UpdateType = (int)UserDataType.Name,
                MountId = request.MountInfoId,
                UpdateTypeValue = request.MountName
            });
            reply();
            await ETTask.CompletedTask;
        }
    }
}
