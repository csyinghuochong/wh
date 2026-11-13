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

            List<DropInfo> privateDrops = unit.GetComponent<UnitInfoComponent>().Drops;
            long now = TimeHelper.ServerNow();
            foreach (DropInfo info in monster.GetComponent<UnitInfoComponent>().Drops)
            {
                if (info.DropType == 1)
                {
                    if (privateDrops.Exists(drop => drop.UnitId == info.UnitId))
                    {
                        response.Drops.Add(info);
                    }
                    continue;
                }
                Unit dropUnit = units.Get(info.UnitId);
                if (dropUnit == null || dropUnit.Type != UnitType.DropItem)
                {
                    continue;
                }
                DropComponent drop = dropUnit.GetComponent<DropComponent>();
                if (drop.OwnerId != 0 && drop.OwnerId != unit.Id
                        && (info.DropType == 3 || info.DropType == 2 && now < drop.ProtectTime))
                {
                    continue;
                }
                response.Drops.Add(drop.ToDropInfo(dropUnit));
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
