namespace ET
{
    /// <summary>
    /// 在线判定：Location 通知用本类；推客户端 Session 仍走 Gate（T2G_GateUnitInfo）。
    /// </summary>
    public static class PlayerOnlineHelper
    {
        /// <summary>
        /// Unit 是否已在 Location 注册（在地图进程）。
        /// </summary>
        public static async ETTask<bool> IsInLocation(long unitId)
        {
            if (unitId == 0)
            {
                return false;
            }
            long actorId = await LocationProxyComponent.Instance.Get(unitId);
            return actorId != 0;
        }
    }
}
