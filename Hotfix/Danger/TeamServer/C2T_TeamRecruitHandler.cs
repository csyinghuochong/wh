using System;

namespace ET
{
    /// <summary>
    /// 发布招募。无队伍则创建。Shout=1 发世界聊天。
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

            LDScene ldScene = LDSceneCategory.Instance.Get(request.SceneId);
            teamInfo.SceneId = request.SceneId;
            //teamInfo.FubenType = ldScene.Scene_Type;
            teamInfo.LevelMin = request.LevelMin;
            teamInfo.LevelMax = request.LevelMax;
            teamInfo.RecruitMsg = request.RecruitMsg ?? string.Empty;
            teamSceneComponent.SyncTeamInfo(teamInfo, teamInfo.PlayerList).Coroutine();

            if (request.Shout == 1)
            {
                int leaderLv = request.TeamPlayerInfo.PlayerLv;
                string link = $"<link=team_{teamInfo.TeamId}_{teamInfo.SceneId}_{teamInfo.FubenType}_{leaderLv}><color=#B5FF28><u>点击申请加入</u></color></link>";
                string recruitMsg = teamInfo.RecruitMsg;
                MessageHelper.SendActor(DBHelper.GetChatServerId(scene.DomainZone()), new T2Chat_WorldChatMessage()
                {
                    ChatInfo = new ChatInfo()
                    {
                        UserId = request.TeamPlayerInfo.UserID,
                        ChannelId = (int)ChannelEnum.Word,
                        ChatMsg = string.IsNullOrEmpty(recruitMsg) ? link : $"{recruitMsg} {link}",
                        ChatMsg_EN = string.IsNullOrEmpty(recruitMsg) ? link : $"{recruitMsg} {link}",
                        PlayerName = request.TeamPlayerInfo.PlayerName,
                        PlayerLevel = leaderLv,
                        Occ = request.TeamPlayerInfo.Occ,
                        Time = TimeHelper.ServerNow(),
                    }
                });
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
