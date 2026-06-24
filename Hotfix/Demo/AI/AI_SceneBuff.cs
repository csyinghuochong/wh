using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_SceneBuff : AAIHandler
    {
        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            return true;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            LDMonster ldMonsterCof = LDMonsterCategory.Instance.Get(unit.ConfigId);
            LDSkill ldSkill = LDSkillCategory.Instance.Get(-1);

            bool remove = false;
            long instanceId = aiComponent.InstanceId;
            for (int i = 0; i < 100000; ++i)
            {
                Unit target = AIGetTargetHelp.GetNearestEnemy(unit, (float)ldSkill.Cast_Range, true);

                //检测目标是否在技能范围
                if (!remove &&  target != null )
                {
                    remove = true;
                    Vector3 direction = target.Position - unit.Position;
                    
                    C2M_SkillCmd cmd = new C2M_SkillCmd();
                    cmd.SkillID =-1;
                    cmd.TargetID = target.Id;
                    if (ldSkill.NeedTarget == SkillNeedTargetType.NoTarget_0)  //自身点
                    {
                        cmd.TargetAngle = 0;
                        cmd.TargetDistance = 0;
                    }
                    else
                    {
                        float ange = Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
                        cmd.TargetAngle = Mathf.FloorToInt(ange);
                        cmd.TargetDistance = Vector3.Distance(unit.Position, target.Position);
                    }

                    //触发技能
                    unit.GetComponent<SkillManagerComponent>().OnUseSkill(cmd, true);
                }

                long waitTime = remove ? 1000 : 200;
                if (unit.ConfigId == 80000003)
                {
                    waitTime = 200;
                }
                await TimerComponent.Instance.WaitAsync(waitTime, cancellationToken);
                if (instanceId != aiComponent.InstanceId)
                {
                    return;
                }
                if (remove)
                {
                    //unit.GetParent<UnitComponent>().Remove(unit.Id);
                    unit.GetComponent<HeroDataComponent>().OnDead(null);
                    return;
                }
            }
        }
    }
}
