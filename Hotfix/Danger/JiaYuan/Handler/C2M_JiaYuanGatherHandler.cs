using System;


namespace ET
{

    /// <summary>
    /// 家园收获
    /// </summary>
    [ActorMessageHandler]
    public class C2M_JiaYuanGatherHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanGatherRequest, M2C_JiaYuanGatherResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanGatherRequest request, M2C_JiaYuanGatherResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            Unit unitplan = unit.GetParent<UnitComponent>().Get(request.UnitId);
            if (unitplan == null)
            {
                reply();
                return;
            }

            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            switch (request.OperateType)
            {
                case 1:
                    JiaYuanPlant jiaYuanPlan = jiaYuanComponentServer.GetJiaYuanPlant(request.UnitId);
                    if (jiaYuanPlan == null)
                    {
                        Log.Error($"jiaYuanPlan == null  {unit.Id}  {request.CellIndex}");
                        reply();
                        return;
                    }

                    response.Error = JiaYuanHelper.GetPlanShouHuoItem(jiaYuanPlan.ItemId, jiaYuanPlan.StartTime, jiaYuanPlan.GatherNumber, jiaYuanPlan.GatherLastTime);
                    if (response.Error != ErrorCode.ERR_Success)
                    {
                        reply();
                        return;
                    }

                    LDHome_Farm ldHomeFarm = LDHome_FarmCategory.Instance.Get(unitplan.ConfigId);
                    unit.GetComponent<BagComponentServer>().OnAddItemData(ldHomeFarm.Reward, $"{ItemGetWay.JiaYuanGather}_{TimeHelper.ServerNow()}");

                    unitplan.GetComponent<NumericComponent>().ApplyValue(NumericType.GatherLastTime, TimeHelper.ServerNow());
                    unitplan.GetComponent<NumericComponent>().ApplyChange(null, NumericType.GatherNumber, 1, 0);

                    jiaYuanPlan.GatherNumber += 1;
                    jiaYuanPlan.GatherLastTime = TimeHelper.ServerNow();
                    jiaYuanPlan.GatherRecord.Add(new JiaYuanRecord()
                    {
                        PlayerId = unit.Id,
                        Time = TimeHelper.ServerNow(),
                        PlayerName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name,
                    }) ;
                    unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanGatherPlant_401, 0, 1);
                    unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanGatherPlant_93, 0, 1);
                    break;
                case 2:
                    JiaYuanPastures jiaYuanPasture = jiaYuanComponentServer.GetJiaYuanPastures(request.UnitId);
                    if (jiaYuanPasture == null)
                    {
                        Log.Error($"jiaYuanPlan == null  {unit.Id}  {request.UnitId}");
                        reply();
                        return;
                    }

                    response.Error = JiaYuanHelper.GetPastureShouHuoItem(jiaYuanPasture.ConfigId, jiaYuanPasture.StartTime, jiaYuanPasture.GatherNumber, jiaYuanPasture.GatherLastTime);
                    if (response.Error != ErrorCode.ERR_Success)
                    {
                        reply();
                        return;
                    }

                    JiaYuanPastureConfig jiaYuanPastureConfig = JiaYuanPastureConfigCategory.Instance.Get(jiaYuanPasture.ConfigId);
                    unit.GetComponent<BagComponentServer>().OnAddItemData($"{jiaYuanPastureConfig.GetItemID};1", $"{ItemGetWay.JiaYuanGather}_{TimeHelper.ServerNow()}");

                    unitplan.GetComponent<NumericComponent>().ApplyValue(NumericType.GatherLastTime, TimeHelper.ServerNow());
                    unitplan.GetComponent<NumericComponent>().ApplyChange(null, NumericType.GatherNumber, 1, 0);

                    jiaYuanPasture.GatherNumber += 1;
                    jiaYuanPasture.GatherLastTime = TimeHelper.ServerNow();
                    unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanGatherPasture_402, 0, 1);

                    unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanGatherPasture_95, 0, 1);
                    break;
            }

            DBHelper.SaveComponentCache( unit.DomainZone(), unit.Id, jiaYuanComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}
