using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UserInfoRequestHandler : AMActorLocationRpcHandler<Unit, C2M_UserInfoRequest, M2C_UserInfoInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UserInfoRequest request, M2C_UserInfoInitResponse response, Action reply)
        {
            unit.GetComponent<ShoujiComponentServer>().UpdateShouJIStar();

            response.RoleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            response.ReddontList =  unit.GetComponent<ReddotComponent>().ReddontList;
            response.TreasureInfo = unit.GetComponent<ShoujiComponentServer>().TreasureInfo;
            response.ShouJiChapterInfos = unit.GetComponent<ShoujiComponentServer>().ShouJiChapterInfos;
            response.TitleList = unit.GetComponent<TitleComponent>().TitleList;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
