using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2T_TeamDungeonApplyHandler : AMActorRpcHandler<Scene, M2T_TeamDungeonApplyRequest, T2M_TeamDungeonApplyResponse>
    {
        protected override async ETTask Run(Scene scene, M2T_TeamDungeonApplyRequest request, T2M_TeamDungeonApplyResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.UnitID);
            if (teamInfo == null)
            {
                response.Error = ErrorCode.Err_TeamIsNull;
                reply();
                return;
            }

            // 只有队长能申请，Apply 必须为 1。
            if (teamInfo.TeamId != request.UnitID || request.Apply != 1)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            // 副本已经开着：不再开第二本，只再广播一次可进入。
            if (teamInfo.FubenInstanceId != 0)
            {
                await teamSceneComponent.BroadcastDungeonApply(teamInfo);
                response.TeamInfo = teamInfo;
                reply();
                return;
            }

            // 还在 3 秒等待里，忽略重复申请。
            if (teamSceneComponent.OpeningTeamIds.Contains(teamInfo.TeamId))
            {
                response.TeamInfo = teamInfo;
                reply();
                return;
            }

            if (request.SceneId > 0)
            {
                teamInfo.SceneId = request.SceneId;
            }

            if (teamInfo.SceneId == 0 || !LDSceneCategory.Instance.Contain(teamInfo.SceneId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 先广播申请中（此时 FubenInstanceId 仍为 0），RPC 立刻返回。3 秒后再创建副本并广播可进入。
            teamSceneComponent.OpeningTeamIds.Add(teamInfo.TeamId);
            await teamSceneComponent.BroadcastDungeonApply(teamInfo);
            response.TeamInfo = teamInfo;
            reply();
            teamSceneComponent.OpenTeamDungeonLater(teamInfo.TeamId).Coroutine();
        }
    }
}
