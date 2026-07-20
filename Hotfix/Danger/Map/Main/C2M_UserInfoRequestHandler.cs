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

            response.RoleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            response.ReddontList =  unit.GetComponent<ReddotComponentServer>().ReddontList;
            response.TreasureInfo = shoujiComponentServer.TreasureInfo;
            response.ShouJiChapterInfos = shoujiComponentServer.ShouJiChapterInfos;
            response.TitleList = unit.GetComponent<TitleComponentServer>().TitleList;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
