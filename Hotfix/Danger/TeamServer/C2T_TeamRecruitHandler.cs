using System;

namespace ET
{
    /// <summary>
    /// 发布招募。无队伍则创建。Shout=1 发招募频道，ChatMsg 只放口号。
    /// </summary>
    [ActorMessageHandler]
    public class C2T_TeamRecruitHandler : AMActorRpcHandler<Scene, C2T_TeamRecruitRequest, T2C_TeamRecruitResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamRecruitRequest request, T2C_TeamRecruitResponse response, Action reply)
        {
            if (request.TeamPlayerInfo == null || request.SceneId == 0 || !LDSceneCategory.Instance.Contain(request.SceneId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.LevelMin < 0 || request.LevelMax < request.LevelMin)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID);
            if (teamInfo != null && teamInfo.TeamId != request.TeamPlayerInfo.UserID)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            if (teamInfo == null)
            {
                teamInfo = teamSceneComponent.CreateTeamInfo(request.TeamPlayerInfo, request.SceneId);
            }

            teamInfo.SceneId = request.SceneId;
            teamInfo.LevelMin = request.LevelMin;
            teamInfo.LevelMax = request.LevelMax;
            teamInfo.RecruitMsg = request.RecruitMsg ?? string.Empty;
            teamSceneComponent.SyncTeamInfo(teamInfo, teamInfo.PlayerList).Coroutine();

            if (request.Shout == 1)
            {
                int leaderLv = request.TeamPlayerInfo.PlayerLv;
                int memberMax = SceneConfigHelper.GetTeamMemberMax(teamInfo.SceneId);
                MessageHelper.SendActor(DBHelper.GetChatServerId(scene.DomainZone()), new T2Chat_WorldChatMessage()
                {
                    ChatInfo = new ChatInfo()
                    {
                        UserId = request.TeamPlayerInfo.UserID,
                        ChannelId = ChannelEnum.Recruit,
                        ParamId = teamInfo.TeamId,
                        Recruit = new RecruitChat()
                        {
                            SceneId = teamInfo.SceneId,
                            LevelMin = teamInfo.LevelMin,
                            LevelMax = teamInfo.LevelMax,
                            MemberCount = teamInfo.PlayerList.Count,
                            MemberMax = memberMax,
                        },
                        ChatMsg = teamInfo.RecruitMsg,
                        ChatMsg_EN = teamInfo.RecruitMsg,
                        PlayerName = request.TeamPlayerInfo.PlayerName,
                        PlayerLevel = leaderLv,
                        Occ = request.TeamPlayerInfo.Occ,
                        ServerId = UnitZoneHelper.GetHomeZone(teamInfo.TeamId),
                        RoleHeadId = request.TeamPlayerInfo.RoleHeadId,
                        RoleHeadFrame = request.TeamPlayerInfo.RoleHeadFrame,
                        VipLevel = request.TeamPlayerInfo.VipLevel,
                        TitleId = request.TeamPlayerInfo.TitleId,
                        Time = TimeHelper.ServerNow(),
                    }
                });
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
