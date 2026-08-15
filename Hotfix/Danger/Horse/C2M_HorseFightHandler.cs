using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_HorseFightHandler : AMActorLocationRpcHandler<Unit, C2M_HorseFightRequest, M2C_HorseFightResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HorseFightRequest request, M2C_HorseFightResponse response, Action reply)
        {
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (!roleInfo.HorseIds.Contains(request.HorseId))
            {
                response.Error = ErrorCode.ERR_HoreseNotActive;
                reply();
                return;
            }

            if (request.HorseId == 10001 && roleInfo.Lv < 25)
            {
                response.Error = ErrorCode.ERR_EquipLvLimit;
                reply();
                return;
            }

            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.HorseFightID, request.HorseId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}