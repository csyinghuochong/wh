using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TeamDungeonPrepareHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonPrepareRequest, M2C_TeamDungeonPrepareResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonPrepareRequest request, M2C_TeamDungeonPrepareResponse response, Action reply)
        {
			int sceneid = request.TeamInfo.SceneId;
			if (sceneid == 0)
			{
				reply();
				return;
			}

			LDScene ldScene = LDSceneCategory.Instance.Get(sceneid);
			RoleDailyDataComponentServer dailyData = unit.GetComponent<RoleDailyDataComponentServer>();
			int enterError = ldScene.CheckSceneEnter(
				unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv,
				dailyData.GetSceneFubenTimes(sceneid),
				dailyData.GetSceneWeekFubenTimes(sceneid));
			if (enterError != ErrorCode.ERR_Success)
			{
				response.Error = enterError;
				reply();
				return;
			}

			int errorcode = ErrorCode.ERR_Success;
			Unit leader = unit.GetParent<UnitComponent>().Get(request.TeamInfo.TeamId);
			//判断副本次数
			long teamServerId = DBHelper.GetTeamServerId(unit);
			T2M_TeamDungeonPrepareResponse createResponse = (T2M_TeamDungeonPrepareResponse)await MessageHelper.CallActor(teamServerId, new M2T_TeamDungeonPrepareRequest()
			{
				TeamId = request.TeamInfo.TeamId,
				UnitID = unit.Id,
				Prepare = request.Prepare,
				ErrorCode = errorcode
			});

			reply();
			await ETTask.CompletedTask;
        }
    }
}
