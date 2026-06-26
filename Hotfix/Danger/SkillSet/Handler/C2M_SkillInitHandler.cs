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
            int occ = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ;
            int occTwo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.OccTwo;
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            response.SkillSetInfo = new SkillSetInfo();
            
            List<int> allskill = new List<int>();
            
            //检测一下初始技能
            LDOccupation ldOccupation = LDOccupationCategory.Instance.Get(occ);
            
            skillSetComponentServer.CheckOccSkill(occ);

            for (int i = skillSetComponentServer.SkillList.Count - 1; i >= 0; i--)
            {
                SkillPro skillPro = skillSetComponentServer.SkillList[i];
                
                if (skillPro.SkillSetType == (int)SkillSetEnum.Item)
                {
                    if (!LDItemCategory.Instance.Contain(skillPro.SkillID))
                    {
                        skillSetComponentServer.SkillList.RemoveAt(i);
                    }
                    continue;
                }
                else
                {
                    if (!LDSkillCategory.Instance.Contain(skillPro.SkillID))
                    {
                        skillSetComponentServer.SkillList.RemoveAt(i);
                    }
                    continue;
                }
            }
           
            response.SkillSetInfo.SkillList = skillSetComponentServer.SkillList;
            response.SkillSetInfo.LifeShieldList = skillSetComponentServer.LifeShieldList;
            response.SkillSetInfo.TianFuPlan = skillSetComponentServer.TianFuPlan;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
