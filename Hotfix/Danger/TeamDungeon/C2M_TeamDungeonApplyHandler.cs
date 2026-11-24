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
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
				reply();
				return;
			}

			long teamId = unit.GetTeamId();
			if (teamId == 0)
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
				reply();
				return;
			}

			if (teamId != unit.Id)
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
				reply();
				return;
			}

			int sceneid = 0;
			// 申请只校验场景参数，不检查等级、次数和战力；取消不依赖客户端场景数据。
			if (request.Apply == 1)
			{
				if (request.TeamInfo == null || request.TeamInfo.SceneId == 0 || !LDSceneCategory.Instance.Contain(request.TeamInfo.SceneId))
				{
					response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
					reply();
					return;
				}

				sceneid = request.TeamInfo.SceneId;
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
