using System;

namespace ET
{
    /// <summary>
    /// 技能设置：Position+Direction 装配；同一技能可装多个槽；SkillID=0 卸下该槽
    /// </summary>
    [ActorMessageHandler]
    public class C2M_SkillSetHandler : AMActorLocationRpcHandler<Unit, C2M_SkillSet, M2C_SkillSet>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillSet request, M2C_SkillSet response, Action reply)
        {
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();

            int error = skillSetComponentServer.SetSkillIdByPosition(request);
            response.Error = error;
            if (error != ErrorCode.ERR_Success)
            {
                reply();
                await ETTask.CompletedTask;
                return;
            }

            skillSetComponentServer.UpdateSkillSet();

            reply();
            await ETTask.CompletedTask;
        }
    }
}
