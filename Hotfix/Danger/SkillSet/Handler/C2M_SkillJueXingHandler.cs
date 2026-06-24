using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_SkillJueXingHandler : AMActorLocationRpcHandler<Unit, C2M_SkillJueXingRequest, M2C_SkillJueXingResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillJueXingRequest request, M2C_SkillJueXingResponse response, Action reply)
        {
            //判断条件
        
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
