using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MonsterDropViewHandler : AMActorLocationRpcHandler<Unit, C2M_MonsterDropViewRequest, M2C_MonsterDropViewResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MonsterDropViewRequest request, M2C_MonsterDropViewResponse response, Action reply)
        {
            if (unit.Type != UnitType.Player || request.MonsterId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            Unit monster = unit.GetParent<UnitComponent>().Get(request.MonsterId);
            MonsterBodyComponent body = monster != null && monster.Type == UnitType.Monster
                ? monster.GetComponent<MonsterBodyComponent>() : null;
            // 关闭只移除当前玩家的记录，不要求仍在原地或仍然存活。
            if (!request.IsOpen)
            {
                if (body != null) body.SetViewing(unit, false);
                reply();
                return;
            }
            if (body == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1
                || monster.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 1
                || PositionHelper.Distance2D(unit, monster) > CommonConfig.PickDistance
                || monster.GetComponent<UnitInfoComponent>().Drops.Count == 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            body.SetViewing(unit, true);
            response.Drops.AddRange(monster.GetComponent<UnitInfoComponent>().Drops);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
