using System;
using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_ZhuiJi : AAIHandler
    {
        private const float StandArriveDistance = 0.6f;

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
            Vector3 standPos = AIGetTargetHelp.GetChaseStandPosition(unit, target, aiComponent.ActDistance, aiComponent.UnitComponent);
            float distanceToStand = PositionHelper.Distance2D(unit.Position, standPos);
            bool zhuiji = distanceToStand > StandArriveDistance && aiComponent.IsCanZhuiJi() == 0;
            return zhuiji;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            StateComponent stateComponent = unit.GetComponent<StateComponent>();

            long checktime;
            switch (aiComponent.SceneTypeEnum)
            {
                case MapTypeEnum.PetDungeon:
                case MapTypeEnum.PetTianTi:
                case MapTypeEnum.PetMing:
                    checktime = 100;
                    break;
                default:
                    checktime = 200;
                    break;
            }

            for (int i = 0; i < 10000; i++)
            {
                Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
                if (target != null)
                {
                    Vector3 standPos = AIGetTargetHelp.GetChaseStandPosition(unit, target, aiComponent.ActDistance, aiComponent.UnitComponent);
                    float distanceToTarget = PositionHelper.Distance2D(unit.Position, target.Position);
                    float distanceToStand = PositionHelper.Distance2D(unit.Position, standPos);
                    bool inAttackRange = distanceToTarget <= aiComponent.ActDistance;
                    bool needMove = distanceToStand > StandArriveDistance && !inAttackRange;

                    if (!needMove)
                    {
                        unit.Stop(-2);
                    }

                    bool shouldUpdatePath = checktime == 100 || (checktime == 200 && i % 5 == 0);
                    if (needMove && shouldUpdatePath && stateComponent.CanMove() == ErrorCode.ERR_Success)
                    {
                        unit.FindPathMoveToAsync(standPos, cancellationToken, false).Coroutine();
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
