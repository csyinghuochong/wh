using System;
using System.Collections.Generic;

namespace ET{
    //设置技能位置
    [ActorMessageHandler]
    public class C2M_SkillInitHandler : AMActorLocationRpcHandler<Unit, C2M_SkillInitRequest, M2C_SkillInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillInitRequest request, M2C_SkillInitResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            int occ = roleInfoComponentServer.RoleInfo.Occ;
            response.SkillSetInfo = new SkillSetInfo();
            
            //检测一下初始技能
            skillSetComponentServer.CheckOccSkill(occ);

            for (int i = skillSetComponentServer.SkillList.Count - 1; i >= 0; i--)
            {
                SkillPro skillPro = skillSetComponentServer.SkillList[i];
                bool shouldRemove = skillPro.SkillSetType == (int)SkillSetEnum.Item
                    ? !LDItemCategory.Instance.Contain(skillPro.SkillID)
                    : !LDSkill_BattleCategory.Instance.Contain(skillPro.SkillID);
                if (shouldRemove)
                {
                    skillSetComponentServer.SkillList.RemoveAt(i);
                }
            }
           
            response.SkillSetInfo.SkillList = skillSetComponentServer.SkillList;
            response.SkillSetInfo.LifeShieldList = skillSetComponentServer.LifeShieldList;
            response.SkillSetInfo.TianFuPlan = skillSetComponentServer.TianFuPlan;
            skillSetComponentServer.CurrentSkillBarList();
            response.SkillSetInfo.SkillBarList = skillSetComponentServer.SkillBarList0;
            response.SkillSetInfo.SkillBarList1 = skillSetComponentServer.SkillBarList1;
            response.SkillSetInfo.SkillBarPlan = skillSetComponentServer.SkillBarPlan;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
