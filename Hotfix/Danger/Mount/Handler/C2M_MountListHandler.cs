using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountListHandler : AMActorLocationRpcHandler<Unit, C2M_MountList, M2C_MountList>
    {
        protected override async ETTask Run(Unit unit, C2M_MountList request, M2C_MountList response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo useMount = mountComponentServer.GetUseMount();
            mountComponentServer.UseMountId = useMount != null ? useMount.Id : 0;
            response.MountInfos = mountComponentServer.GetAllMounts();
            response.UseMountId = mountComponentServer.UseMountId;
            response.RideMountId = mountComponentServer.RideMountId;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
