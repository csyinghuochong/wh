using System;

namespace ET
{

    [ActorMessageHandler]
    public class Actor_JiaYuanPickHandler : AMActorLocationRpcHandler<Unit, Actor_JiaYuanPickRequest, Actor_JiaYuanPickResponse>
    {
        protected override async ETTask Run(Unit unit, Actor_JiaYuanPickRequest request, Actor_JiaYuanPickResponse response, Action reply)
        {
            Unit boxUnit = unit.GetParent<UnitComponent>().Get(request.UnitId);
            if (boxUnit == null)
            {
                response.Error = ErrorCode.ERR_PlantNotExist;
                reply();
                return;
            }
            NumericComponent boxNumeric = boxUnit.GetComponent<NumericComponent>();
            if (boxNumeric.GetAsInt(NumericType.Now_Dead) == 1)
            {
                response.Error = ErrorCode.ERR_PlantNotExist;
                reply();
                return;
            }

            NumericComponent unitNumeric = unit.GetComponent<NumericComponent>();
            if (unit.Id != request.MasterId)
            {
                if (unitNumeric.GetAsInt(NumericType.JiaYuanPickOther) >= 5)
                {
                    response.Error = ErrorCode.ERR_TimesIsNot;
                    reply();
                    return;
                }
            }

            boxUnit.GetComponent<UnitLifeComponent>()?.OnDead(unit);

            if (unit.Id == request.MasterId)
            {
                JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
                jiaYuanComponentServer.OnRemoveUnit(request.UnitId);

                await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer);
            }
            else
            {
                unitNumeric.ApplyChange(null, NumericType.JiaYuanPickOther, 1, 0);

                JiaYuanComponentServer jiaYuanComponentServer2 = await DBHelper.GetComponent<JiaYuanComponentServer>(UnitZoneHelper.GetHomeZone(request.MasterId), request.MasterId);
                long gateServerId = DBHelper.GetGateServerId(unit);
                G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                    (gateServerId, new T2G_GateUnitInfoRequest()
                    {
                        UserID = request.MasterId
                    });

                //玩家在线
                if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
                {
                    RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
                    JiaYuanOperate jiaYuanOperate = new JiaYuanOperate();
                    jiaYuanOperate.OperateType = JiaYuanOperateType.Pick;
                    jiaYuanOperate.UnitId = request.UnitId;
                    jiaYuanOperate.PlayerName = roleInfoComponent.RoleInfo.Name;
                    jiaYuanOperate.OperateId = boxUnit.ConfigId;
                    M2M_JiaYuanOperateMessage opmessage = new M2M_JiaYuanOperateMessage()
                    {
                        JiaYuanOperate = jiaYuanOperate,
                    };
                    MessageHelper.SendToLocationActor(request.MasterId, opmessage);
                }
                else
                {
                    jiaYuanComponentServer2.OnRemoveUnit(request.UnitId);
                    await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(request.MasterId), request.MasterId, jiaYuanComponentServer2);
                }
            }
            
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
