using System;
using System.Collections.Generic;


namespace ET
{
    public static class FriendHelper
    {

        public static async ETTask<List<FriendInfo>> GetFriendInfos(long dbCacheId, long gateServerId,  List<long> friends)
        {
            List<FriendInfo> friendInfos = new List < FriendInfo >();
            for (int i = 0; i < friends.Count; i++)
            {
                long friendId = friends[i];
                D2G_GetComponent d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = friendId, Component = DBHelper.RoleInfoComponent });
                RoleInfoComponentServer roleInfoComponentServer = d2GGetUnit.Component as RoleInfoComponentServer;
                if (roleInfoComponentServer == null)
                {
                    continue;
                }
                RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;

                G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                   (gateServerId, new T2G_GateUnitInfoRequest()
                   {
                       UserID = friendId
                   });

                friendInfos.Add(new FriendInfo()
                {
                    UserId = friendId,
                    PlayerLevel = roleInfo.Lv,
                    OnLineTime = g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0  ? 1 : 0,
                    PlayerName = roleInfo.Name,
                    Occ = roleInfo.Occ
                });
            }

            return friendInfos;
        }
    }
}
