using UnityEngine;

namespace ET
{


    /// <summary>
    /// 单人副本怪物AI
    /// </summary>
    [AIHandler]
    public class AI_LocalDungeon : AAIHandler
    {
        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            if (aiComponent.TargetID != 0 || aiComponent.IsRetreat!=0)
            {
                return false;
            }
            Unit nearest = null;
            Unit unit = aiComponent.GetParent<Unit>();
            if (aiComponent.LocalDungeonUnit == null)
            {
                aiComponent.Stop();
                Log.Error($"aiComponent.LocalDungeonUnit == null: scenetype:{ aiComponent.SceneTypeEnum}  confidid: {unit.ConfigId}");
                return true;
            }
            if (PositionHelper.Distance2D(unit, aiComponent.LocalDungeonUnit) <= aiComponent.SearchRange)
            {
                nearest = aiComponent.LocalDungeonUnit;
            }
            if (nearest == null || nearest.IsCanBeAttack(true, false))
            {
                
            }
            
            if (nearest == null || !nearest.IsCanBeAttack())
            {
                aiComponent.TargetID = 0;
                aiComponent.noCheckStatus = true;
                return true;
            }

            if (aiComponent.Unit.IsBoss())
            {
                aiComponent.Unit.GetComponent<NumericComponent>().ApplyValue(NumericType.BossInCombat, 1, true, true);
            }
            aiComponent.TargetID = nearest.Id;
            return aiComponent.TargetID == 0;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            while (true)
            {
                if (aiComponent.PatrolRange > 0f)
                {
                    Vector3 initVec3 = unit.GetBornPostion();
                    float new_x = initVec3.x + RandomHelper.RandomNumberFloat(-1 * aiComponent.PatrolRange, aiComponent.PatrolRange);
                    float new_z = initVec3.z + RandomHelper.RandomNumberFloat(-1 * aiComponent.PatrolRange, aiComponent.PatrolRange);
                    Vector3 nextTarget = new Vector3(new_x, initVec3.y, new_z);

                    unit.FindPathMoveToAsync(nextTarget, cancellationToken, true).Coroutine();
                }


                bool timeRet = await TimerComponent.Instance.WaitAsync(10000, cancellationToken);
                if (!timeRet)
                {
                    //Log.Debug("巡逻被打断！！" );
                    return;
                }
            }
        }

    }
}
