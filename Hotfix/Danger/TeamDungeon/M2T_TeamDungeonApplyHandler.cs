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

            if (teamInfo.TeamId != request.UnitID)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            if (request.TeamId != teamInfo.TeamId || (request.Apply != 0 && request.Apply != 1))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 副本已经创建，不能再申请或取消申请。
            if (teamInfo.FubenInstanceId != 0)
            {
                response.Error = ErrorCode.ERR_AlreadyOpen;
                reply();
                return;
            }

            if (request.Apply == 0)
            {
                if (teamSceneComponent.OpeningTeams.Remove(teamInfo.TeamId))
                {
                    await teamSceneComponent.BroadcastDungeonApply(teamInfo, true);
                }

                response.TeamInfo = teamInfo;
                reply();
                return;
            }

            // 还在 10 秒等待里，重复申请不重新计时。
            if (teamSceneComponent.OpeningTeams.ContainsKey(teamInfo.TeamId))
            {
                response.TeamInfo = teamInfo;
                reply();
                return;
            }

            if (request.SceneId == 0 || !LDSceneCategory.Instance.Contain(request.SceneId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 申请计时独立于广播耗时；取消后重申会获得新的申请 Id。
            teamInfo.SceneId = request.SceneId;
            long applyId = IdGenerater.Instance.GenerateInstanceId();
            teamSceneComponent.OpeningTeams.Add(teamInfo.TeamId, applyId);
            teamSceneComponent.OpenTeamDungeonLater(teamInfo.TeamId, applyId).Coroutine();
            await teamSceneComponent.BroadcastDungeonApply(teamInfo);
            response.TeamInfo = teamInfo;
            reply();
        }
    }
}
