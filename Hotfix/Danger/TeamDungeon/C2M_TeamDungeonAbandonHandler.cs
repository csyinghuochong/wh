using System;

namespace ET
{

	//队长关闭副本
    [ActorMessageHandler]
    public class C2M_TeamDungeonAbandonHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonAbandonRequest, M2C_TeamDungeonAbandonResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonAbandonRequest request, M2C_TeamDungeonAbandonResponse response, Action reply)
        {
            if (request.FubenInstanceId <= 0)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            if (unit.GetTeamId() == 0)
            {
                response.Error = ErrorCode.Err_TeamIsNull;
                reply();
                return;
            }
            if (unit.GetTeamId() != unit.Id)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            T2M_TeamDungeonAbandonResponse result = (T2M_TeamDungeonAbandonResponse)await MessageHelper.CallActor(
                DBHelper.GetTeamServerId(unit), new M2T_TeamDungeonAbandonRequest
                {
                    UserId = unit.Id,
                    FubenInstanceId = request.FubenInstanceId,
                });
            response.Error = result.Error;
            reply();
        }
    }
}
