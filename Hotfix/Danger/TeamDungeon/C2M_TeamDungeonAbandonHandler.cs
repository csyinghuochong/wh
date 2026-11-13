using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_TeamDungeonAbandonHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonAbandonRequest, M2C_TeamDungeonAbandonResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonAbandonRequest request, M2C_TeamDungeonAbandonResponse response, Action reply)
        {
            Scene scene = unit.DomainScene();
            if (scene.GetComponent<MapComponent>().MapTypeEnum != MapTypeEnum.TeamDungeon
                || request.FubenInstanceId <= 0 || request.FubenInstanceId != scene.InstanceId)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 发起者只须身处目标副本，不限制队长。
            response.Error = await scene.GetComponent<TeamDungeonComponent>().Abandon();
            reply();
        }
    }
}
