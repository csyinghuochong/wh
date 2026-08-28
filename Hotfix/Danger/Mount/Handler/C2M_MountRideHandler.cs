using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountRideHandler : AMActorLocationRpcHandler<Unit, C2M_MountRide, M2C_MountRide>
    {
        protected override async ETTask Run(Unit unit, C2M_MountRide request, M2C_MountRide response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            if (mountComponentServer.GetRideMount() != null)
            {
                mountComponentServer.Dismount();
                response.RideConfigId = 0;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            MountInfo useMount = mountComponentServer.GetUseMount();
            if (useMount == null)
            {
                response.Error = ErrorCode.ERR_HoreseNotFight;
                reply();
                return;
            }

            mountComponentServer.SetRide(true);
            response.RideConfigId = mountComponentServer.GetRideConfigId();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
