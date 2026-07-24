using System;

namespace ET
{
    /// <summary>切换技能栏方案（0/1）</summary>
    [ActorMessageHandler]
    public class C2M_SkillBarPlanHandler : AMActorLocationRpcHandler<Unit, C2M_SkillBarPlanRequest, M2C_SkillBarPlanResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillBarPlanRequest request, M2C_SkillBarPlanResponse response, Action reply)
        {
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            response.Error = skillSetComponentServer.UpdateSkillBarPlan(request.SkillBarPlan);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
