using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class HitFlyServerHelper
    {
        public static void GmNearbyMonsters(Unit caster)
        {
            List<Unit> units = caster.GetParent<UnitComponent>().GetAll();
            for (int i = 0; i < units.Count; ++i)
            {
                Unit monster = units[i];
                if (monster == null || monster.IsDisposed || monster.Type != UnitType.Monster)
                {
                    continue;
                }

                if (PositionHelper.Distance2D(caster, monster) > HitFlyHelper.GmSearchRange)
                {
                    continue;
                }

                Knockback(monster).Coroutine();
            }
        }

        public static async ETTask Knockback(Unit unit)
        {
            Vector3 start = unit.Position;
            Vector3 dest = BackDest(unit, HitFlyHelper.GmKnockbackDistance);
            unit.GetComponent<AIComponent>()?.Interrupt();
            unit.GetComponent<StateComponent>()?.StateTypeAdd(StateTypeEnum.PassiveMove);
            Broadcast(unit, start, dest);
            unit.Position = dest;

            await TimerComponent.Instance.WaitAsync(HitFlyHelper.GmTemp.DurationMs);
            if (!unit.IsDisposed)
            {
                unit.GetComponent<StateComponent>()?.StateTypeRemove(StateTypeEnum.PassiveMove);
            }
        }

        private static Vector3 BackDest(Unit unit, float distance)
        {
            Vector3 dir = -unit.Forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f)
            {
                dir = Vector3.back;
            }
            else
            {
                dir.Normalize();
            }

            Vector3 dest = unit.Position + dir * distance;
            MapComponent map = unit.DomainScene()?.GetComponent<MapComponent>();
            return map == null ? dest : map.GetCanChongJiPath(unit, unit.Position, dest);
        }

        private static void Broadcast(Unit unit, Vector3 start, Vector3 dest)
        {
            M2C_HitFly msg = new M2C_HitFly
            {
                UnitId = unit.Id,
                StartX = start.x,
                StartY = start.y,
                StartZ = start.z,
                DestX = dest.x,
                DestY = dest.y,
                DestZ = dest.z,
                DurationMs = HitFlyHelper.GmTemp.DurationMs,
            };

            MapComponent map = unit.Domain.GetComponent<MapComponent>();
            if (map != null && map.MapTypeEnum == MapTypeEnum.MainCityScene)
            {
                MessageHelper.BroadcastMainCity(unit, msg);
            }
            else
            {
                MessageHelper.Broadcast(unit, msg);
            }
        }
    }
}
