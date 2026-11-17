using UnityEngine;

namespace ET
{
    /// <summary>怪物围攻站位：避开邻近怪物及其移动终点，不修改攻击距离。</summary>
    public static class AIChaseStandHelper
    {
        // 均为单位坐标在 XZ 平面上的距离，不包含模型体积。
        private const float MinPlayerDistance = 0.5f;
        private const float Separation = 0.3f;
        private const float ArrivalDistance = 0.25f;
        private const int CandidateCount = 16;

        public static bool IsInAttackPosition(AIComponent ai, Unit target)
        {
            Unit unit = ai.GetParent<Unit>();
            float distance = PositionHelper.Distance2D(unit.Position, target.Position);
            if (distance > ai.ActDistance)
            {
                return false;
            }

            // 禁止移动或正在吟唱时，不因让位切换 AI、打断已有动作。
            if (unit.Type != UnitType.Monster || ai.IsCanZhuiJi() != ErrorCode.ERR_Success)
            {
                return true;
            }

            // 实际间距必须达标，不能因落点容差或周围无空位而在过近处停下。
            return (target.Type != UnitType.Player || distance >= MinPlayerDistance)
                && !IsOccupied(unit, unit.Position, true);
        }

        public static Vector3 GetStandPosition(AIComponent ai, Unit target)
        {
            Unit unit = ai.GetParent<Unit>();
            bool inRange = PositionHelper.Distance2D(unit.Position, target.Position) <= ai.ActDistance;
            Vector3 approach = AIGetTargetHelp.GetChaseApproachPosition(unit, target, ai.ActDistance);
            if (unit.Type != UnitType.Monster)
            {
                return inRange ? unit.Position : approach;
            }

            float minimumDistance = target.Type == UnitType.Player ? MinPlayerDistance : 0f;
            bool hasTargetClearance = PositionHelper.Distance2D(unit.Position, target.Position) >= minimumDistance;
            Vector3 offset = approach - target.Position;
            float standDistance = Mathf.Max(offset.magnitude, minimumDistance + ArrivalDistance);
            if (offset.sqrMagnitude < 0.0001f)
            {
                offset = Vector3.forward;
            }
            offset = offset.normalized * standDistance;
            approach = target.Position + offset;

            // 重叠时 ID 较大者让位，避免双方同时左右移动。
            if (inRange && hasTargetClearance && !IsOccupied(unit, unit.Position, true))
            {
                return unit.Position;
            }

            MoveComponent move = unit.GetComponent<MoveComponent>();
            float reservedDistance = PositionHelper.Distance2D(ai.TargetZhuiJi, target.Position);
            if (!move.IsArrived() && reservedDistance >= minimumDistance + ArrivalDistance && reservedDistance <= ai.ActDistance
                && !IsOccupied(unit, ai.TargetZhuiJi, false))
            {
                return ai.TargetZhuiJi;
            }

            for (int i = 0; i < CandidateCount; ++i)
            {
                // 从最近的方向依次向两侧找位置，不使用每次变化的随机偏移。
                int step = (i + 1) / 2;
                float angle = step * (360f / CandidateCount) * (i % 2 == 0 ? -1 : 1);
                Vector3 candidate = target.Position + Quaternion.Euler(0, angle, 0) * offset;
                if (!IsOccupied(unit, candidate, false))
                {
                    return candidate;
                }
            }

            // 没有空位时继续按接近点寻路，不能将间距不足的当前位置判为已到位。
            return approach;
        }

        private static bool IsOccupied(Unit unit, Vector3 position, bool yieldToLowerId)
        {
            foreach (AOIEntity entity in unit.GetSeeUnits().Values)
            {
                Unit other = entity.Unit;
                if (other.IsDisposed || other.Id == unit.Id || other.Type != UnitType.Monster
                    || (yieldToLowerId && other.Id > unit.Id))
                {
                    continue;
                }

                NumericComponent numeric = other.GetComponent<NumericComponent>();
                if (numeric.GetAsInt(NumericType.Now_Dead) != 0 || numeric.GetAsLong(NumericType.HP_Current_8) <= 0)
                {
                    continue;
                }

                float clearance = Separation + (yieldToLowerId ? 0f : ArrivalDistance);
                if (PositionHelper.Distance2D(position, other.Position) < clearance)
                {
                    return true;
                }

                MoveComponent move = other.GetComponent<MoveComponent>();
                // 规划终点预留双方到位误差；检查当前位置时只预留移动方的误差。
                float reservedClearance = Separation + ArrivalDistance * (yieldToLowerId ? 1f : 2f);
                if (!move.IsArrived()
                    && PositionHelper.Distance2D(position, move.FinalTarget) < reservedClearance)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
