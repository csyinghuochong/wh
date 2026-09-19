using System;

namespace ET
{
    /// <summary>
    /// 邀请回复。ReplyCode：0 拒绝，1 同意。
    /// </summary>
    [ActorMessageHandler]
    public class C2T_TeamInviteReplyHandler : AMActorRpcHandler<Scene, C2T_TeamInviteReplyRequest, T2C_TeamInviteReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamInviteReplyRequest request, T2C_TeamInviteReplyResponse response, Action reply)
        {
            if (request.TeamPlayerInfo_1 == null || request.TeamPlayerInfo_2 == null)
            {
                reply();
                return;
            }

            if (request.ReplyCode != 1)
            {
                reply();
                return;
            }

            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo inviterTeam = teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo_1.UserID);
            TeamInfo selfTeam = teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo_2.UserID);
            if (selfTeam != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            if (inviterTeam != null && inviterTeam.TeamId != request.TeamPlayerInfo_1.UserID)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            if (inviterTeam != null && inviterTeam.PlayerList.Count >= 3)
            {
                response.Error = ErrorCode.ERR_TeamIsFull;
                reply();
                return;
            }

            if (!await ServerMessageHelper.IsInGame(scene.DomainZone(), request.TeamPlayerInfo_1.UserID))
            {
                reply();
                return;
            }

            if (inviterTeam == null)
            {
                inviterTeam = teamSceneComponent.CreateTeamInfo(request.TeamPlayerInfo_1, 0);
            }

            bool haveplayer = false;
            for (int i = 0; i < inviterTeam.PlayerList.Count; i++)
            {
                if (inviterTeam.PlayerList[i].UserID == request.TeamPlayerInfo_2.UserID)
                {
                    haveplayer = true;
                    break;
                }
            }
            if (!haveplayer)
            {
                inviterTeam.PlayerList.Add(request.TeamPlayerInfo_2);
            }

            teamSceneComponent.SyncTeamInfo(inviterTeam, inviterTeam.PlayerList).Coroutine();
            reply();
        }
    }
}
