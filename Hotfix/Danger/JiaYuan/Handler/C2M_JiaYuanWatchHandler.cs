using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanWatchHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanWatchRequest, M2C_JiaYuanWatchResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanWatchRequest request, M2C_JiaYuanWatchResponse response, Action reply)
        {
            Unit boxUnit = unit.GetParent<UnitComponent>().Get(request.OperateId);
            if (boxUnit == null)
            {
                response.Error = ErrorCode.ERR_PlantNotExist;
                reply();
                return;
            }
            if (boxUnit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                response.Error = ErrorCode.ERR_PlantNotExist;
                reply();
                return;
            }

            if (unit.Id == request.MasterId)
            {
                JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
                JiaYuanPlant jiaYuanPlant = jiaYuanComponentServer.GetJiaYuanPlant(request.OperateId);

                response.JiaYuanRecord = jiaYuanPlant.GatherRecord;
            }
            else
            {
                JiaYuanComponentServer jiaYuanComponentServer2 = await DBHelper.GetComponentCache<JiaYuanComponentServer>(UnitZoneHelper.GetHomeZone(request.MasterId), request.MasterId);
                JiaYuanPlant jiaYuanPlant_2 = jiaYuanComponentServer2.GetJiaYuanPlant(request.OperateId);

                response.JiaYuanRecord = jiaYuanPlant_2.GatherRecord;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
