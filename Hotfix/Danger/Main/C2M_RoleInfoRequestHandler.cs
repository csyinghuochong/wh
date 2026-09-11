using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleInfoRequestHandler : AMActorLocationRpcHandler<Unit, C2M_RoleInfoRequest, M2C_RoleInfoInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleInfoRequest request, M2C_RoleInfoInitResponse response, Action reply)
        {
           
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            ReddotComponentServer reddotComponentServer = unit.GetComponent<ReddotComponentServer>();
            TitleComponentServer titleComponentServer = unit.GetComponent<TitleComponentServer>();
            response.RoleInfo = roleInfoComponentServer.RoleInfo;
            response.ReddontList = reddotComponentServer.ReddontList;
            response.TitleList = titleComponentServer.TitleList;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
