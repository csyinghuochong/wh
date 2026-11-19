using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [AIHandler]
    public class AI_Retreat : AAIHandler
    {

        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            // 返回途中保持当前节点，避免重新索敌或切回攻击。
            if (aiComponent.IsRetreat != 0)
            {
                return true;
            }

            Unit unit = aiComponent.Unit;
            float bornDistance = PositionHelper.Distance2D(unit.GetBornPostion(), unit.Position);
            if (aiComponent.LoseAggro > 0 && bornDistance > aiComponent.LoseAggro)
            {
                return true;
            }

            Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
            if (target == null || target.IsDisposed)
            {
                return false;
            }

            return aiComponent.ChaseRange > 0
                && PositionHelper.Distance2D(unit.Position, target.Position) > aiComponent.ChaseRange;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            bool haveStealth = false;
            if (unit.IsBoss())
            {
                unit.GetComponent<NumericComponent>().ApplyValue(NumericType.BossInCombat, 0);
                unit.GetComponent<UnitLifeComponent>()?.OnKillZhaoHuan(null);
                unit.GetComponent<AttackRecordComponent>().ClearBeAttack();
                haveStealth = AIGetTargetHelp.GetNearestStealth(unit);
            }
           
            //boss 检测， 回撤的一瞬间周围有隐身单位， 至少五秒后才回血
            //5秒后才回血，如果节点被打断不回血。

            aiComponent.TargetID = 0;
            aiComponent.IsRetreat = TimeHelper.ServerNow()+ (haveStealth ? TimeHelper.Second * 5 : 0);
            SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();
            skillManagerComponent.ClearMonsterSing();
            skillManagerComponent.OnFinish(true);
            unit.Stop(0);
            List<Unit> units = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player);
            for (int i = 0; i < units.Count; i++)
            {
                units[i].GetComponent<BuffManagerComponent>().OnRetreatRemoveBuff(unit.Id);
            }
            Vector3 bornVector3 = unit.GetBornPostion();

            while (true)
            {
                if (unit.GetComponent<StateComponent>().CanMove()== ErrorCode.ERR_Success)
                {
                    unit.FindPathMoveToAsync(bornVector3, cancellationToken, false).Coroutine();
                }
                bool timeRet = await TimerComponent.Instance.WaitAsync(1000, cancellationToken);
                if (unit.IsDisposed)
                {
                    return;
                }
                if (!timeRet)
                {
                    // 被击退等行为打断后，下次 AI 检查仍继续返回；中断时不回血。
                    return;
                }

                long serverNow = TimeHelper.ServerNow();
                if (PositionHelper.Distance2D(bornVector3, unit.Position) < 0.5f && serverNow > aiComponent.IsRetreat)
                {
                    NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                    if (aiComponent.SceneTypeEnum != MapTypeEnum.TeamDungeon && numericComponent.GetAsInt(NumericType.Now_Dead) == 0)
                    {
                        long maxHp = numericComponent.GetAsLong(NumericType.HP_Max_10);
                        numericComponent.ApplyValue(NumericType.HP_Current_8, maxHp);
                    }
                    aiComponent.IsRetreat = 0;
                }

                if (aiComponent.IsRetreat == 0)
                {
                    return;
                }
            }
        }
    }
}
