using System.Collections.Generic;

namespace ET
{
    public static class RoleDeleteHelper
    {
        /// <summary>正式删角前清理公会、队伍和好友关系；全部成功后才标记删除，冷静期内不调用。</summary>
        public static async ETTask<int> RemoveRelations(CreateRoleInfo roleInfo)
        {
            Union2R_DeleteRoleData response = (Union2R_DeleteRoleData)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(roleInfo.ServerId), new R2Union_DeleteRoleData { UnitId = roleInfo.UserID });
            if (response.Error != ErrorCode.ERR_Success)
            {
                return response.Error;
            }

            // 跨服组队可能保存在其他区的队伍服，逐服退队并等待成员刷新通知发出。
            List<int> zones = ServerMessageHelper.GetAllZone();
            if (!zones.Contains(roleInfo.ServerId))
            {
                zones.Add(roleInfo.ServerId);
            }
            HashSet<long> teamServers = new HashSet<long>();
            foreach (int zone in zones)
            {
                long teamServerId = DBHelper.GetTeamServerId(zone);
                if (!teamServers.Add(teamServerId))
                {
                    continue;
                }
                Other2A_ServerMessageRResponse teamResponse = (Other2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call(
                    teamServerId, new A2Other_ServerMessageRequest
                    {
                        MessageType = ServerNoticeType.PlayerExit,
                        MessageValue = roleInfo.UserID.ToString(),
                    });
                if (teamResponse.Error != ErrorCode.ERR_Success)
                {
                    return teamResponse.Error;
                }
            }

            await FriendHelper.RemoveRoleRelations(roleInfo.ServerId, roleInfo.UserID, zones);
            return ErrorCode.ERR_Success;
        }
    }
}
