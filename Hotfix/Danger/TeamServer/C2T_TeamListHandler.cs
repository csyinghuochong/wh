using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamListHandler : AMActorRpcHandler<Scene, C2T_TeamListRequest, T2C_TeamListResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamListRequest request, T2C_TeamListResponse response, Action reply)
        {
            List<TeamInfo> teamInfos = new List<TeamInfo>();
            List<TeamInfo> teamList = scene.GetComponent<TeamSceneComponent>().TeamList;
            for (int i = teamList.Count - 1; i >= 0; i--)
            {
                TeamInfo teamInfo = teamList[i];
                if (teamInfo.PlayerList.Count == 0)
                {
                    teamList.RemoveAt(i);
                    continue;
                }
                if (teamInfo.SceneId > 0)
                {
                    teamInfos.Add(teamInfo);
                }
            }

            response.TeamList = teamInfos;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
