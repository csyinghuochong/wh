using System;
using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_ZhuiJi : AAIHandler
    {
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
            //获取范敌人是否在攻击范围内
            float distance = Vector3.Distance(target.Position, aiComponent.GetParent<Unit>().Position);
            bool zhuiji = distance >= aiComponent.ActDistance && aiComponent.IsCanZhuiJi() == 0;
            return zhuiji;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            //获取附近最近距离的目标进行追击
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
                    bool zhuiji =   Vector3.Distance(unit.Position, target.Position) >= aiComponent.ActDistance;
                    if (!zhuiji)
                    {
                        unit.Stop(-2);
                    }
                    if (zhuiji && checktime == 100 && stateComponent.CanMove() == ErrorCode.ERR_Success)
                    {
                        unit.FindPathMoveToAsync(target.Position, cancellationToken, false).Coroutine();
                    }
                    if (zhuiji && checktime == 200 && stateComponent.CanMove() == ErrorCode.ERR_Success && i % 5 == 0)
                    {
                        //Vector3 dir = unit.Position - target.Position;
                        //float ange = Mathf.Rad2Deg(Mathf.Atan2(dir.x, dir.z));
                        //float addg = unit.Id % 10 * (unit.Id % 2 == 0 ? 2 : -2);
                        //Quaternion rotation = Quaternion.Euler(0, ange + addg, 0);
                        //Vector3 ttt = target.Position + rotation * Vector3.forward * ((float)aiComponent.ActDistance - 0.2f);
                        //unit.FindPathMoveToAsync(ttt, cancellationToken, false).Coroutine();
                        unit.FindPathMoveToAsync(target.Position, cancellationToken, false).Coroutine();
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
