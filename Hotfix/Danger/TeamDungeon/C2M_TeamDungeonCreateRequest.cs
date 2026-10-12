using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_TeamDungeonCreateHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonCreateRequest, M2C_TeamDungeonCreateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonCreateRequest request, M2C_TeamDungeonCreateResponse response, Action reply)
        {
            int enterError = ErrorCode.ERR_Success;
            if (request.FubenId > 0 && LDSceneCategory.Instance.Contain(request.FubenId))
            {
                LDScene ldScene = LDSceneCategory.Instance.Get(request.FubenId);
                RoleDailyDataComponentServer dailyData = unit.GetComponent<RoleDailyDataComponentServer>();
                enterError = ldScene.CheckSceneEnter(
                    unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv,
                    dailyData.GetSceneFubenTimes(request.FubenId),
                    dailyData.GetSceneWeekFubenTimes(request.FubenId));
            }
            if (enterError != ErrorCode.ERR_Success)
            {
                response.Error = enterError;
                reply();
                return;
            }

            long teamServerId = DBHelper.GetTeamServerId(unit);
            T2M_TeamDungeonCreateResponse createResponse = (T2M_TeamDungeonCreateResponse)await MessageHelper.CallActor(teamServerId, new M2T_TeamDungeonCreateRequest()
            {
                FubenId = request.FubenId,
                TeamPlayerInfo = request.TeamPlayerInfo,
                FubenType = request.FubenType,
            });

            if (createResponse.Error != ErrorCode.ERR_Success)
            {
                response.Error = createResponse.Error;
                reply();
                return;
            }
            response.FubenType = request.FubenType;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
