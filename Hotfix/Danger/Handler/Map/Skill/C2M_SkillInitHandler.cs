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

            int occ = unit.GetComponent<UserInfoComponent>().UserInfo.Occ;
            int occTwo = unit.GetComponent<UserInfoComponent>().UserInfo.OccTwo;
            SkillSetComponent skillSetComponent = unit.GetComponent<SkillSetComponent>();
            response.SkillSetInfo = new SkillSetInfo();
            
            //刷新转职技能
            if (occTwo != 0)
            {
                ///移除重复的转职技能

                LDOccupation_Transfer occupationTwo = LDOccupation_TransferCategory.Instance.Get(occTwo);

                List<int> occTwoSkillList = new List<int>() { };
                List<int> selfoccTwoSkill = new List<int>() { };

            }

            List<int> allskill = new List<int>();
          
            
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
