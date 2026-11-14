using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TeamDungeonApplyHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonApplyRequest, M2C_TeamDungeonApplyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonApplyRequest request, M2C_TeamDungeonApplyResponse response, Action reply)
        {
			if (request.Apply != 0 && request.Apply != 1)
			{
				response.Error = ErrorCode.ERR_ModifyData;
				reply();
				return;
			}

			long teamId = unit.GetTeamId();
			if (teamId == 0)
			{
				response.Error = ErrorCode.Err_TeamIsNull;
				reply();
				return;
			}

			if (teamId != unit.Id)
			{
				response.Error = ErrorCode.ERR_IsNotLeader;
				reply();
				return;
			}

			int sceneid = 0;
			// 取消只校验队长身份，不受等级、次数或客户端场景数据限制。
			if (request.Apply == 1)
			{
				if (request.TeamInfo == null || request.TeamInfo.SceneId == 0 || !LDSceneCategory.Instance.Contain(request.TeamInfo.SceneId))
				{
					response.Error = ErrorCode.ERR_ModifyData;
					reply();
					return;
				}

				sceneid = request.TeamInfo.SceneId;
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
			T2M_TeamDungeonApplyResponse createResponse = (T2M_TeamDungeonApplyResponse)await MessageHelper.CallActor(teamServerId, new M2T_TeamDungeonApplyRequest()
			{
				TeamId = teamId,
				UnitID = unit.Id,
				Apply = request.Apply,
				ErrorCode = ErrorCode.ERR_Success,
				SceneId = sceneid,
			});
			response.Error = createResponse.Error;
			reply();
        }
    }
}
