using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionMyInfoHandler : AMActorRpcHandler<Scene, C2U_UnionMyInfoRequest, U2C_UnionMyInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionMyInfoRequest request, U2C_UnionMyInfoResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo?.UnionInfo == null || dBUnionInfo.UnionInfo.LeaderId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            UnionInfo src = dBUnionInfo.UnionInfo;
            response.UnionMyInfo = CopyUnionInfoWithoutKeJiJingXuan(src);

            HashSet<long> onlineIds = await ServerMessageHelper.GetChatOnlineUnitIds(scene.DomainZone());
            List<UnionPlayerInfo> playerList = src.UnionPlayerList;
            if (playerList != null && onlineIds.Count > 0)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    UnionPlayerInfo player = playerList[i];
                    if (player == null || player.UserID == 0)
                    {
                        continue;
                    }

                    if (onlineIds.Contains(player.UserID))
                    {
                        response.OnLinePlayer.Add(player.UserID);
                    }
                }
            }

            reply();
        }

        /// <summary>回包用新对象，不带回科技/竞选，也不改内存里的公会文档。</summary>
        private static UnionInfo CopyUnionInfoWithoutKeJiJingXuan(UnionInfo src)
        {
            UnionInfo info = new UnionInfo();
            info.UnionName = src.UnionName;
            info.LeaderId = src.LeaderId;
            info.LeaderName = src.LeaderName;
            info.LevelLimit = src.LevelLimit;
            info.UnionPurpose = src.UnionPurpose;
            info.ApplyList = src.ApplyList;
            info.UnionId = src.UnionId;
            info.Level = src.Level < 1 ? 1 : src.Level;
            info.Exp = src.Exp;
            info.UnionPlayerList = src.UnionPlayerList;
            info.DonationRecords = src.DonationRecords;
            info.UnionGold = src.UnionGold;
            info.ActiveRecord = src.ActiveRecord;
            info.UnionBanner = src.UnionBanner;
            info.UnionPattern = src.UnionPattern;
            return info;
        }
    }
}
