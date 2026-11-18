using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MonsterDropsHandler : AMActorLocationRpcHandler<Unit, C2M_MonsterDropsRequest, M2C_MonsterDropsResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MonsterDropsRequest request, M2C_MonsterDropsResponse response, Action reply)
        {
            response.MonsterId = request.MonsterId;
            UnitComponent units = unit.GetParent<UnitComponent>();
            Unit monster = units.Get(request.MonsterId);
            if (request.MonsterId <= 0 || unit.Type != UnitType.Player
                    || unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1
                    || monster == null || monster.Type != UnitType.Monster
                    || monster.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 1
                    || PositionHelper.Distance2D(unit, monster) > CommonConfig.PickDistance)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            response.Drops.AddRange(monster.GetComponent<UnitInfoComponent>().Drops);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
