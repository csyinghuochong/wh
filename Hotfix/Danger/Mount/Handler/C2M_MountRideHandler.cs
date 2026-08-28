using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountRideHandler : AMActorLocationRpcHandler<Unit, C2M_MountRide, M2C_MountRide>
    {
        protected override async ETTask Run(Unit unit, C2M_MountRide request, M2C_MountRide response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            if (mountComponentServer.RideMountId > 0)
            {
                mountComponentServer.Dismount();
                response.RideMountId = 0;
                response.RideConfigId = 0;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (mountComponentServer.MountInfos.Count == 0)
            {
                response.Error = ErrorCode.ERR_HoreseNotActive;
                reply();
                return;
            }

            MountInfo ride = mountComponentServer.PickRideMount();
            if (ride == null)
            {
                response.Error = ErrorCode.ERR_HoreseNotFight;
                reply();
                return;
            }

            mountComponentServer.SetRide(ride);
            response.RideMountId = mountComponentServer.RideMountId;
            response.RideConfigId = mountComponentServer.GetRideConfigId();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
