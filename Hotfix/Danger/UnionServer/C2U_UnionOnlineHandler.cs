using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionOnlineHandler : AMActorRpcHandler<Scene, C2U_UnionOnlineRequest, U2C_UnionOnlineResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionOnlineRequest request, U2C_UnionOnlineResponse response, Action reply)
        {
            response.PlayerList = new List<UnionPlayerInfo>();
            if (request.UnionId == 0)
            {
                reply();
                return;
            }

            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo?.UnionInfo?.UnionPlayerList == null)
            {
                reply();
                return;
            }

            HashSet<long> onlineIds = await ServerMessageHelper.GetChatOnlineUnitIds(scene.DomainZone());
            List<UnionPlayerInfo> playerList = dBUnionInfo.UnionInfo.UnionPlayerList;
            for (int i = 0; i < playerList.Count; i++)
            {
                UnionPlayerInfo player = playerList[i];
                if (player == null || player.UserID == 0 || player.UserID == request.UserId)
                {
                    continue;
                }
                if (onlineIds.Contains(player.UserID))
                {
                    response.PlayerList.Add(player);
                }
            }

            reply();
        }
    }
}
