using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_UnitTransfer_0_RequestHandler : AMActorRpcHandler<Scene, M2M_UnitTransfer_0_Request, M2M_UnitTransfer_0_Response>
    {
        protected override async ETTask Run(Scene scene, M2M_UnitTransfer_0_Request request, M2M_UnitTransfer_0_Response response, Action reply)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            bool continuing = unitComponent.UnitComponents.TryGetValue(request.UnitId, out UnitTransferBuffer buffer)
                    && buffer.TransferId == request.TransferId && buffer.ExpireTime > TimeHelper.ServerNow();
            if (scene.GetComponent<MapComponent>().MapTypeEnum == MapTypeEnum.TeamDungeon && !continuing)
            {
                int enterError = scene.GetComponent<TeamDungeonComponent>().CheckEnter(request.UnitId);
                if (enterError != ErrorCode.ERR_Success)
                {
                    response.Error = enterError;
                    reply();
                    return;
                }
            }

            response.Error = UnitTransferHelper.Receive(unitComponent, request, out bool started);
            if (started)
            {
                TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + UnitTransferHelper.Timeout,
                    TimerType.UnitTransferTimeout, Tuple.Create(unitComponent, request.UnitId, request.TransferId));
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
