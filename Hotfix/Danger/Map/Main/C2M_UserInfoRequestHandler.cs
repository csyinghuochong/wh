using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UserInfoRequestHandler : AMActorLocationRpcHandler<Unit, C2M_UserInfoRequest, M2C_UserInfoInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UserInfoRequest request, M2C_UserInfoInitResponse response, Action reply)
        {
            ShoujiComponentServer shoujiComponentServer = unit.GetComponent<ShoujiComponentServer>();
            shoujiComponentServer.UpdateShouJIStar();

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            ReddotComponentServer reddotComponentServer = unit.GetComponent<ReddotComponentServer>();
            TitleComponentServer titleComponentServer = unit.GetComponent<TitleComponentServer>();
            response.RoleInfo = roleInfoComponentServer.RoleInfo;
            response.ReddontList = reddotComponentServer.ReddontList;
            response.TreasureInfo = shoujiComponentServer.TreasureInfo;
            response.ShouJiChapterInfos = shoujiComponentServer.ShouJiChapterInfos;
            response.TitleList = titleComponentServer.TitleList;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
