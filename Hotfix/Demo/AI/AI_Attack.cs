using System;
using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_Attack: AAIHandler
    {

        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
            if (target == null || target.IsDisposed)
            {
                aiComponent.TargetID = 0;
                return false;
            }

            return AIChaseStandHelper.IsInAttackPosition(aiComponent, target);
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();
            // 切入攻击时同步停止，避免取消追击协程后客户端仍沿旧路径移动。
            unit.Stop(0);

            for (int i = 0; i < 100000; ++i)
            {
                int skillId = aiComponent.GetCastSkillId();

                Unit target = aiComponent.UnitComponent.Get(aiComponent.TargetID);
                if (target == null || !target.IsCanBeAttack())
                {
                    aiComponent.TargetID = 0;
                }

                float distance = 0f;
                if (target != null)
                {
                    distance = PositionHelper.Distance2D(target.Position, unit.Position);
                }

                if (skillId != 0 && aiComponent.TargetID != 0 && distance <= aiComponent.ActDistance
                    && AIChaseStandHelper.IsInAttackPosition(aiComponent, target)
                    && skillManagerComponent.IsCanUseSkill(skillId) == ErrorCode.ERR_Success)
                {
                    LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(skillId);
                    Vector3 direction = target.Position - unit.Position;
                    
                    C2M_SkillCmd cmd = aiComponent.c2M_SkillCmd;
                    //触发技能
                    cmd.TargetID = target.Id;
                    cmd.SkillID = skillId;

                    if (ldSkill.NeedTarget == SkillNeedTargetType.NoTarget_0)  //自身点
                    {
                        cmd.TargetAngle = 0;
                        cmd.TargetDistance = 0;
                    }
                    else
                    {
                        float ange = Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
                        cmd.TargetAngle = Mathf.FloorToInt(ange);
                        cmd.TargetDistance = PositionHelper.Distance2D(unit.Position, target.Position);
                    }

                    long serverNow = TimeHelper.ServerNow();
                    aiComponent.LastAttackTime = serverNow;
                    // true=已起条/正在读条，到点由 OnMonsterSingComplete 出手；false=瞬发，立刻 OnUseSkill
                    if (!skillManagerComponent.TryBeginMonsterSing(cmd))
                    {
                        skillManagerComponent.OnUseSkill(cmd, true);
                    }
                }

                // 因为协程可能被中断，任何协程都要传入cancellationToken，判断如果是中断则要返回
                bool timeRet = await TimerComponent.Instance.WaitAsync(200, cancellationToken);
                if (!timeRet)
                {
                    return;
                }
            }
        }
    }
}
