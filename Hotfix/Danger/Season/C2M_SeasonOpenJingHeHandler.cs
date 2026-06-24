using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_SeasonOpenJingHeHandler : AMActorLocationRpcHandler<Unit, C2M_SeasonOpenJingHeRequest, M2C_SeasonOpenJingHeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SeasonOpenJingHeRequest request, M2C_SeasonOpenJingHeResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();   
            if (roleInfoComponentServer.RoleInfo.OpenJingHeIds.Contains(request.JingHeId))
            {
                response.Error = ErrorCode.ERR_AlreadyLearn;
                reply();
                return;
            }

            roleInfoComponentServer.RoleInfo.OpenJingHeIds.Add(request.JingHeId);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
