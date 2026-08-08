using System;
using System.Collections.Generic;


namespace ET
{
    public static class FriendHelper
    {

        public static async ETTask<List<FriendInfo>> GetFriendInfos(long gateServerId,  List<long> friends)
        {
            List<FriendInfo> friendInfos = new List < FriendInfo >();
            for (int i = 0; i < friends.Count; i++)
            {
                long friendId = friends[i];
                int homeZone = UnitZoneHelper.GetHomeZone(friendId);
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, friendId);
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
