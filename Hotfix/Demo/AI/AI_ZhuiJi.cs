using System;
using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_ZhuiJi : AAIHandler
    {
        private const float RepathDistance = 0.5f;

        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            if (aiComponent.TargetID == 0 || aiComponent.IsRetreat !=0)
            {
                return false;
            }
            Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
            if (target == null)
            {
                aiComponent.TargetID = 0;
                return false;
            }
            Unit unit = aiComponent.GetParent<Unit>();
            float distanceToStand = PositionHelper.Distance2D(unit.Position, target.Position);
            bool zhuiji = distanceToStand > aiComponent.ActDistance && aiComponent.IsCanZhuiJi() == 0;
            return zhuiji;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            StateComponent stateComponent = unit.GetComponent<StateComponent>();
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();

            long checktime = 100;

            for (int i = 0; i < 10000; i++)
            {
                Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
                if (target != null)
                {
                    Vector3 standPos = AIGetTargetHelp.GetChaseApproachPosition(unit, target, aiComponent.ActDistance);
                    float distanceToTarget = PositionHelper.Distance2D(unit.Position, target.Position);
                    bool inAttackRange = distanceToTarget <= aiComponent.ActDistance;
                    bool canMove = stateComponent.CanMove() == ErrorCode.ERR_Success;
                    bool moving = !moveComponent.IsArrived();

                    if (!canMove)
                    {
                        if (moving)
                        {
                            unit.Stop(-2);
                        }
                    }
                    else if (inAttackRange)
                    {
                        if (moving)
                        {
                            unit.Stop(0);
                        }
                    }
                    else
                    {
                        float destMoved = PositionHelper.Distance2D(standPos, aiComponent.TargetZhuiJi);
                        if (!moving || destMoved > RepathDistance)
                        {
                            aiComponent.TargetZhuiJi = standPos;
                            unit.FindPathMoveToAsync(standPos, cancellationToken, false).Coroutine();
                        }
                    }
                }
                bool timeRet = await TimerComponent.Instance.WaitAsync(checktime, cancellationToken);
                if (!timeRet)
                {
                    return;
                }
            }
        }
    }


}
