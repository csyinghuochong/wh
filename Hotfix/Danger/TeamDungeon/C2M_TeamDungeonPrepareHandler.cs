using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TeamDungeonPrepareHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonPrepareRequest, M2C_TeamDungeonPrepareResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonPrepareRequest request, M2C_TeamDungeonPrepareResponse response, Action reply)
        {
			if (request.TeamInfo == null || request.TeamInfo.SceneId == 0 || !LDSceneCategory.Instance.Contain(request.TeamInfo.SceneId))
			{
				response.Error = ErrorCode.ERR_ModifyData;
				reply();
				return;
			}

			int sceneid = request.TeamInfo.SceneId;
			long teamId = unit.GetTeamId();
			bool leaderOpen = teamId == 0 || teamId == unit.Id;
			if (leaderOpen)
			{
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
			}

			long teamServerId = DBHelper.GetTeamServerId(unit);
			T2M_TeamDungeonPrepareResponse createResponse = (T2M_TeamDungeonPrepareResponse)await MessageHelper.CallActor(teamServerId, new M2T_TeamDungeonPrepareRequest()
			{
				TeamId = request.TeamInfo.TeamId,
				UnitID = unit.Id,
				Prepare = request.Prepare,
				ErrorCode = ErrorCode.ERR_Success,
				SceneId = leaderOpen && request.Prepare == 1 ? sceneid : 0,
			});
			response.Error = createResponse.Error;
			reply();
        }
    }
}
