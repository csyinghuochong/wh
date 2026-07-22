using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 家园偷取
    /// </summary>
    [ActorMessageHandler]
    public class C2M_JiaYuanGatherOtherHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanGatherOtherRequest, M2C_JiaYuanGatherOtherResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanGatherOtherRequest request, M2C_JiaYuanGatherOtherResponse response, Action reply)
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
                response.Error = ErrorCode.ERR_PlantNotExist;
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.JiaYuan, unit.Id))
            {
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                string playerName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                if (numericComponent.GetAsInt(NumericType.JiaYuanGatherOther) >= 5)
                {
                    response.Error = ErrorCode.ERR_TimesIsNot;
                    reply();
                    return;
                }

                JiaYuanComponentServer jiaYuanComponentServer = await DBHelper.GetComponent<JiaYuanComponentServer>(UnitZoneHelper.GetHomeZone(request.MasterId), request.MasterId);
                if (jiaYuanComponentServer == null)
                {
                    reply();
                    return;
                }

                JiaYuanOperate jiaYuanOperate = null;
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
                        if (jiaYuanPlan.StealNumber >= 1)
                        {
                            response.Error = ErrorCode.ERR_JiaYuanSteal;
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
                        bagComponentServer.OnAddItemData(ldHomeFarm.Reward, $"{ItemGetWay.JiaYuanGather}_{TimeHelper.ServerNow()}");

                        unitplan.GetComponent<NumericComponent>().ApplyValue(NumericType.GatherLastTime, TimeHelper.ServerNow());
                        unitplan.GetComponent<NumericComponent>().ApplyChange(null, NumericType.GatherNumber, 1, 0);
                        unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanGatherPlant_401, 0, 1);
                        unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanGatherPlant_93, 0, 1);
                        jiaYuanPlan.GatherNumber += 1;
                        jiaYuanPlan.StealNumber += 1;
                        jiaYuanPlan.GatherLastTime = TimeHelper.ServerNow();

                        jiaYuanOperate  = new JiaYuanOperate();
                        jiaYuanOperate.OperateType = JiaYuanOperateType.GatherPlant;
                        jiaYuanOperate.UnitId = request.UnitId;
                        jiaYuanOperate.PlayerId = unit.Id;
                        jiaYuanOperate.PlayerName = playerName;

                        JiaYuanRecord jiaYuanRecord = new JiaYuanRecord()
                        {
                            OperateType = JiaYuanOperateType.GatherPlant,
                            OperateId = jiaYuanPlan.ItemId,
                            PlayerName = playerName,
                            Time = TimeHelper.ServerNow(),
                            PlayerId = unit.Id,
                        };
                        jiaYuanPlan.GatherRecord.Add(jiaYuanRecord);
                        jiaYuanComponentServer.AddJiaYuanRecord(jiaYuanRecord);
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
                        bagComponentServer.OnAddItemData($"{jiaYuanPastureConfig.GetItemID};1", $"{ItemGetWay.JiaYuanGather}_{TimeHelper.ServerNow()}");

                        unitplan.GetComponent<NumericComponent>().ApplyValue(NumericType.GatherLastTime, TimeHelper.ServerNow());
                        unitplan.GetComponent<NumericComponent>().ApplyChange(null, NumericType.GatherNumber, 1, 0);

                        unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanGatherPasture_402, 0, 1);
                        unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanGatherPasture_95, 0, 1);
                        jiaYuanPasture.GatherNumber += 1;
                        jiaYuanPasture.StealNumber += 1;
                        jiaYuanPasture.GatherLastTime = TimeHelper.ServerNow();

                        jiaYuanOperate = new JiaYuanOperate();
                        jiaYuanOperate.OperateType = JiaYuanOperateType.GatherPasture;
                        jiaYuanOperate.UnitId = request.UnitId;
                        jiaYuanOperate.PlayerName = playerName;
                        JiaYuanRecord jiaYuanRecord_1 = new JiaYuanRecord()
                        {
                            OperateType = JiaYuanOperateType.GatherPasture,
                            OperateId = jiaYuanPasture.ConfigId,
                            PlayerName = playerName,
                            Time = TimeHelper.ServerNow(),
                        };
                        jiaYuanComponentServer.AddJiaYuanRecord(jiaYuanRecord_1);
                        break;
                }


                M2M_JiaYuanOperateRequest opRequest = new M2M_JiaYuanOperateRequest() { JiaYuanOperate = jiaYuanOperate };
                M2M_JiaYuanOperateResponse opResponse = (M2M_JiaYuanOperateResponse)await MessageHelper.CallLocationActor(request.MasterId, opRequest);
                if (opResponse.Error != ErrorCode.ERR_Success)
                {
                    await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(request.MasterId), request.MasterId, jiaYuanComponentServer);
                }
               
                numericComponent.ApplyChange( null, NumericType.JiaYuanGatherOther,1, 0 );
            }

            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}