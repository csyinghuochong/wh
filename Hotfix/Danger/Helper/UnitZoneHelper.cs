namespace ET
{
    /// <summary>
    /// 角色 Zone 统一入口（创角须 GenerateUnitId）：
    /// GetHomeZone = 归属服；GetCurrentZone = 当前地图所在区。
    /// </summary>
    public static class UnitZoneHelper
    {
        public static int GetHomeZone(long unitId)
        {
            return ServerHelper.GetNewServerId(UnitIdStruct.GetUnitZone(unitId));
        }

        public static int GetHomeZone(Unit unit)
        {
            return GetHomeZone(unit.Id);
        }

        public static int GetCurrentZone(Unit unit)
        {
            return unit.DomainZone();
        }

        public static bool IsTouring(Unit unit)
        {
            return GetCurrentZone(unit) != GetHomeZone(unit);
        }
    }
}
