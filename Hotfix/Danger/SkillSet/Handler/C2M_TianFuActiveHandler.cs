using System;

namespace ET
{
    //激活天赋
    [ActorMessageHandler]
    public class C2M_TianFuActiveHandler : AMActorLocationRpcHandler<Unit, C2M_TianFuActiveRequest, M2C_TianFuActiveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TianFuActiveRequest request, M2C_TianFuActiveResponse response, Action reply)
        {
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            int oldId = skillSetComponentServer.HaveSameTianFu(request.TianFuId);
            if (oldId != 0 && oldId != request.TianFuId)
            {
                // GlobalValueConfig globalValueConfig = GlobalValueConfigCategory.Instance.Get(48);
             
            }

            skillSetComponentServer.OnActiveTianfu(request);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
