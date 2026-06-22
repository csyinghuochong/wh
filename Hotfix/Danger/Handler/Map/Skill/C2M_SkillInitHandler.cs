using System;
using System.Collections.Generic;

namespace ET
{
    //设置技能位置
    [ActorMessageHandler]
    public class C2M_SkillInitHandler : AMActorLocationRpcHandler<Unit, C2M_SkillInitRequest, M2C_SkillInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillInitRequest request, M2C_SkillInitResponse response, Action reply)
        {
            int occ = unit.GetComponent<RoleInfoComponent>().RoleInfo.Occ;
            int occTwo = unit.GetComponent<RoleInfoComponent>().RoleInfo.OccTwo;
            SkillSetComponent skillSetComponent = unit.GetComponent<SkillSetComponent>();
            response.SkillSetInfo = new SkillSetInfo();
            
            List<int> allskill = new List<int>();
            
            //检测一下初始技能
            LDOccupation ldOccupation = LDOccupationCategory.Instance.Get(occ);

          
            skillSetComponent.CheckNormalSkill(occ);
            skillSetComponent.CheckWeaponSkill(occ);

            for (int i = skillSetComponent.SkillList.Count - 1; i >= 0; i--)
            {
                SkillPro skillPro = skillSetComponent.SkillList[i];
                
                if (skillPro.SkillSetType == (int)SkillSetEnum.Item)
                {
                    if (!LDItemCategory.Instance.Contain(skillPro.SkillID))
                    {
                        skillSetComponent.SkillList.RemoveAt(i);
                    }
                    continue;
                }
                else
                {
                    if (!LDSkillCategory.Instance.Contain(skillPro.SkillID))
                    {
                        skillSetComponent.SkillList.RemoveAt(i);
                    }
                    continue;
                }
            }
           
            response.SkillSetInfo.SkillList = skillSetComponent.SkillList;
            response.SkillSetInfo.LifeShieldList = skillSetComponent.LifeShieldList;
            response.SkillSetInfo.TianFuPlan = skillSetComponent.TianFuPlan;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
