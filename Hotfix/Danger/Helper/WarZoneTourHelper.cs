namespace ET
{
    /// <summary>
    /// 战区跨服旅游：同战区内进入目标游戏服主城地图。
    /// GM：21#目标区号，例 21#2；21#本服区号 则回本服主城。
    /// Zone 请一律走 <see cref="UnitZoneHelper"/>。
    /// </summary>
    public static class WarZoneTourHelper
    {
        /// <summary>传送到目标服主城。同战区校验；目标=归属服即回本服。</summary>
        public static async ETTask<int> TourToZone(Unit unit, int targetZone)
        {
            int homeZone = UnitZoneHelper.GetHomeZone(unit);

            if (!StartZoneConfigCategory.Instance.Contain(targetZone))
            {
                Log.Warning($"[WarZoneTour] 目标区不存在 unit={unit.Id} home={homeZone} target={targetZone}");
                return ErrorCode.ERR_RequestRepeatedly;
            }

            if (StartZoneConfigCategory.Instance.IsWarShareZone(targetZone))
            {
                Log.Warning($"[WarZoneTour] 不能旅游到战区共享区 unit={unit.Id} target={targetZone}");
                return ErrorCode.ERR_RequestRepeatedly;
            }

            if (!StartZoneConfigCategory.Instance.IsSameWarZone(homeZone, targetZone))
            {
                Log.Warning($"[WarZoneTour] 非同战区 unit={unit.Id} home={homeZone}({StartZoneConfigCategory.Instance.GetWarZone(homeZone)}) target={targetZone}({StartZoneConfigCategory.Instance.GetWarZone(targetZone)})");
                return ErrorCode.ERR_RequestRepeatedly;
            }

            if (!StartSceneConfigCategory.Instance.TryGetBySceneName(targetZone, $"Map{CommonHelper.MainCityID()}", out StartSceneConfig mapConfig))
            {
                Log.Error($"[WarZoneTour] 目标服无主城 Map unit={unit.Id} target={targetZone}");
                return ErrorCode.ERR_RequestRepeatedly;
            }

            int currentZone = UnitZoneHelper.GetCurrentZone(unit);
            if (currentZone == targetZone
                && unit.DomainScene().GetComponent<MapComponent>()?.MapTypeEnum == MapTypeEnum.MainCityScene)
            {
                Log.Debug($"[WarZoneTour] 已在目标主城 unit={unit.Id} zone={targetZone}");
                return ErrorCode.ERR_Success;
            }

            Log.Console($"[WarZoneTour] {unit.Id} home={homeZone} {currentZone} → {targetZone} map={mapConfig.InstanceId}");

            TransferHelper.BeforeTransfer(unit);
            await TransferHelper.Transfer(unit, mapConfig.InstanceId, (int)MapTypeEnum.MainCityScene, CommonHelper.MainCityID(), 0, "0");
            return ErrorCode.ERR_Success;
        }
    }
}
