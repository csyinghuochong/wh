using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_GetTargetMonstersHandler : AMActorLocationRpcHandler<Unit, C2M_GetTargetMonstersRequest, M2C_GetTargetMonstersResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_GetTargetMonstersRequest request, M2C_GetTargetMonstersResponse response, Action reply)
        {
            if (unit.Type != UnitType.Player)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric.GetAsInt(NumericType.Now_Dead) == 1 || numeric.GetAsLong(NumericType.HP_Current_8) <= 0)
            {
                response.Error = ErrorCode.ERR_CanNotSkillDead;
                reply();
                return;
            }

            LDGlobalValueCategory globals = LDGlobalValueCategory.Instance;
            int radius = globals.GetInt(GlobalValueKey.Global_Select_Target_Max_Radius);
            int count = globals.GetInt(GlobalValueKey.Global_Select_Target_Repeat_Num);
            if (radius <= 0 || count <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            List<(Unit Unit, float Distance)> candidates = new List<(Unit, float)>();
            foreach (Unit monster in unit.GetParent<UnitComponent>().GetAll())
            {
                if (monster.IsDisposed || monster.Type != UnitType.Monster)
                {
                    continue;
                }
                float distance = PositionHelper.Distance2D(unit, monster);
                if (distance > radius || !unit.IsCanAttackUnit(monster))
                {
                    continue;
                }
                candidates.Add((monster, distance));
            }
            candidates.Sort((left, right) =>
            {
                int compare = left.Distance.CompareTo(right.Distance);
                return compare != 0 ? compare : left.Unit.Id.CompareTo(right.Unit.Id);
            });

            int targetCount = Math.Min(count, candidates.Count);
            for (int i = 0; i < targetCount; i++)
            {
                Unit monster = candidates[i].Unit;
                response.Targets.Add(new CombatTargetInfo
                {
                    UnitId = monster.Id,
                    ConfigId = monster.ConfigId,
                    X = monster.Position.x,
                    Y = monster.Position.y,
                    Z = monster.Position.z,
                });
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
