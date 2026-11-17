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
            if (target == null || target.IsDisposed)
            {
                aiComponent.TargetID = 0;
                return false;
            }
            return aiComponent.IsCanZhuiJi() == ErrorCode.ERR_Success
                && !AIChaseStandHelper.IsInAttackPosition(aiComponent, target);
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();

            long checktime = 100;

            for (int i = 0; i < 10000; i++)
            {
                Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
                if (target != null && !target.IsDisposed)
                {
                    Vector3 standPos = AIChaseStandHelper.GetStandPosition(aiComponent, target);
                    bool inAttackRange = AIChaseStandHelper.IsInAttackPosition(aiComponent, target);
                    bool canMove = aiComponent.IsCanZhuiJi() == ErrorCode.ERR_Success;
                    bool moving = !moveComponent.IsArrived();

                    if (!canMove || inAttackRange)
                    {
                        if (moving)
                        {
                            unit.Stop(canMove ? 0 : MoveStopTypeEnum.MoveStopTypeFu2);
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
