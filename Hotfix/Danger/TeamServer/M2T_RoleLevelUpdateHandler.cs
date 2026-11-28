namespace ET
{
    [ActorMessageHandler]
    public class M2T_RoleLevelUpdateHandler : AMActorHandler<Scene, M2T_RoleLevelUpdate>
    {
        /// <summary>网关转发到实际队伍服；队伍服只更新仍在队中的成员，再用现有推送刷新全队。</summary>
        protected override async ETTask Run(Scene scene, M2T_RoleLevelUpdate message)
        {
            if (message.UnitId <= 0 || message.Level <= 0)
            {
                return;
            }

            // 跨服组队时，队伍可能由其他区的队伍服管理，直接发到玩家本区队伍服会找不到成员。
            // 地图服先把等级通知发给归属 Gate，由 Gate 按玩家已保存的队伍服地址转发。
            if (scene.SceneType == SceneType.Gate)
            {
                Player player = scene.GetComponent<PlayerComponent>().GetByUserId(message.UnitId);
                if (player == null)
                {
                    return;
                }
                // 优先使用实际加入的队伍服；未绑定时使用本区默认队伍服，成员查找和等级更新由队伍服处理。
                long teamServerId = player.JoinedTeamServerID != 0 ? player.JoinedTeamServerID : player.TeamServerID;
                MessageHelper.SendActor(teamServerId, message);
                return;
            }

            if (scene.SceneType != SceneType.Team)
            {
                return;
            }
            // 同一角色连续升级时按收到顺序发送，防止异步查询网关后旧等级晚到。
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, message.UnitId))
            {
                TeamInfo teamInfo = scene.GetComponent<TeamSceneComponent>().GetTeamInfo(message.UnitId);
                if (teamInfo == null)
                {
                    return;
                }
                TeamPlayerInfo member = teamInfo.PlayerList.Find(player => player.UserID == message.UnitId);
                if (member == null || member.PlayerLv == message.Level)
                {
                    return;
                }
                member.PlayerLv = message.Level;

                // 只推等级增量，不重发成员关系，避免离队期间的等级通知把已离队角色重新加回客户端。
                M2C_TeamPlayerLevelUpdate update = new M2C_TeamPlayerLevelUpdate
                {
                    UnitId = message.UnitId,
                    Level = message.Level,
                };
                foreach (TeamPlayerInfo player in teamInfo.PlayerList.ToArray())
                {
                    if (player.RobotId != 0)
                    {
                        continue;
                    }
                    await ServerMessageHelper.SendToClient(UnitZoneHelper.GetHomeZone(player.UserID), player.UserID, update);
                }
            }
        }
    }
}
