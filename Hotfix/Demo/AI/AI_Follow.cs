using System;
using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_Follow : AAIHandler
    {

        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            long masterid = unit.MasterId;
            Unit master = aiComponent.UnitComponent.Get(masterid);
            if (master == null || master.IsDisposed || aiComponent.AISkillIDList.Count == 0)
            {
                aiComponent.TargetID = 0;
                return true;
            }

            float distance = PositionHelper.Distance2D(unit.Position, master.Position);
            AttackRecordComponent attackRecordComponent = master.GetComponent<AttackRecordComponent>();
            if ((aiComponent.LoseAggro > 0 && distance > aiComponent.LoseAggro)
                || (aiComponent.IsRetreat != 0 && distance > 1.5f))
            {
                aiComponent.TargetID = 0;
                aiComponent.IsRetreat = TimeHelper.ServerNow();
                return true;
            }
            aiComponent.IsRetreat = 0;

            long mastaerAttackId = attackRecordComponent.PetLockId;
            Unit enemyUnit = aiComponent.UnitComponent.Get(mastaerAttackId);
            if (enemyUnit == null || enemyUnit.IsDisposed || !unit.IsCanAttackUnit(enemyUnit))
            {
                mastaerAttackId = attackRecordComponent.AttackingId;
                enemyUnit = aiComponent.UnitComponent.Get(mastaerAttackId);
            }
            if (enemyUnit == null || enemyUnit.IsDisposed || !unit.IsCanAttackUnit(enemyUnit))
            {
                mastaerAttackId = attackRecordComponent.BeAttackId;
                enemyUnit = aiComponent.UnitComponent.Get(mastaerAttackId);
            }
            if (enemyUnit == null || enemyUnit.IsDisposed || !unit.IsCanAttackUnit(enemyUnit))
            {
                enemyUnit = aiComponent.UnitComponent.Get(aiComponent.TargetID);
            }
            if (enemyUnit == null || enemyUnit.IsDisposed || !unit.IsCanAttackUnit(enemyUnit))
            {
                aiComponent.TargetID = 0;
                return true;
            }

            aiComponent.TargetID = enemyUnit.Id;
            return false;
        }

        public static Vector3 GetFollowPosition(Unit unit, Unit master)
        {
            //Vector3 cur = unit.Position;
            //Vector3 tar = master.Position;
            //Vector3 dir = (cur - tar).normalized;
            //tar = tar + (1f * dir);
            Vector3 dir = unit.Position - master.Position;
            float ange = Mathf.Rad2Deg(Mathf.Atan2(dir.x, dir.z));
            float addg = unit.Id % 10 * (unit.Id % 2 == 0 ? 5 : -5);
            addg += RandomHelper.RandFloat() * 5f;
            Quaternion rotation = Quaternion.Euler(0, ange + addg, 0);
            Vector3 tar = master.Position + rotation * Vector3.forward;
            return tar;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            long masterid = unit.MasterId;
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            unit.Stop(0);

            if (aiComponent.IsRetreat != 0)
            {
                SkillManagerComponent skillManager = unit.GetComponent<SkillManagerComponent>();
                skillManager.ClearMonsterSing();
                skillManager.OnFinish(true);
            }

            while (true)
            {
                Unit master = aiComponent.UnitComponent.Get(masterid);
                if (master == null || master.IsDisposed)
                {
                    unit.Stop(0);
                    return;
                }

                // 未骑乘用宠物配置速度，骑乘时用坐骑速度 + 10。
                if (unit.Type == UnitType.Pet)
                {
                    MountInfo ride = master.Type == UnitType.Player ? master.GetComponent<MountComponentServer>().GetRideMount() : null;
                    int speed = ride == null ? LDPetCategory.Instance.Get(unit.ConfigId).Speed : MountHelper.GetSpeedByLv(ride.MountLv) + 10;
                    if (numericComponent.GetAsLong(NumericType.Speed_Fixed_16) != speed)
                    {
                        numericComponent.Set(NumericType.Speed_Fixed_16, speed);
                    }
                }

                int errorCode = unit.GetComponent<StateComponent>().CanMove();
                float distacne = PositionHelper.Distance2D(unit.Position, master.Position);

                if (aiComponent.IsRetreat > 0 && distacne < 1.5f)
                {
                    aiComponent.IsRetreat = 0;
                }

                if (distacne > 1.5f && errorCode == ErrorCode.ERR_Success)
                {
                    Vector3 nextTarget = GetFollowPosition(unit, master);
                    unit.FindPathMoveToAsync(nextTarget, cancellationToken, false).Coroutine();
                }
                else if (!unit.GetComponent<MoveComponent>().IsArrived())
                {
                    unit.Stop(errorCode == ErrorCode.ERR_Success ? 0 : MoveStopTypeEnum.MoveStopTypeFu2);
                }
                bool result = await TimerComponent.Instance.WaitAsync(200, cancellationToken);
                if (!result)
                {
                    return;
                }

            }
        }

        public  async ETTask Execute_2(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            long masterid = unit.MasterId;
            Unit master = aiComponent.UnitComponent.Get(masterid);

            while (true)
            {
                long nowspeed = 60000;
                if (master != null && !master.IsDisposed)
                {
                    nowspeed = master.GetComponent<NumericComponent>().GetAsLong(NumericType.Numeric_Error);
                }
                int errorCode = unit.GetComponent<StateComponent>().CanMove();
                float distacne = PositionHelper.Distance2D(unit.Position, master.Position);

                if (aiComponent.IsRetreat > 0 && distacne < 1.5f)
                {
                    aiComponent.IsRetreat = 0;
                }

                if (errorCode == ErrorCode.ERR_Success && distacne > 1.5f)
                {
                    nowspeed = (long)(nowspeed * distacne / 2f);
                }
                else
                {
                    nowspeed = 0;
                }

                //宠物移动速度限制
                if (nowspeed >= 100000)
                {
                    nowspeed = 100000;
                }

                if (nowspeed > 0)
                {
                    Vector3 nextTarget = GetFollowPosition(unit, master);
                    unit.GetComponent<NumericComponent>().Set(NumericType.Numeric_Error, nowspeed);
                    unit.FindPathMoveToAsync(nextTarget, cancellationToken, false).Coroutine();
                }
                bool result = await TimerComponent.Instance.WaitAsync(200, cancellationToken);
                if (!result)
                {
                    aiComponent.IsRetreat = 0;
                    break;
                }

            }
        }
    }
}
