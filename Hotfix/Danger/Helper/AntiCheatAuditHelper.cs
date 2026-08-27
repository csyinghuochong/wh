using System;
using System.Collections.Generic;
using System.Text;

namespace ET
{
    /// <summary>
    /// 审计 / 反作弊 / 大额货币日志：从热路径主逻辑剥离，不改变业务规则。
    /// </summary>
    public static class AntiCheatAuditHelper
    {
        public static void OnHourTick(Unit unit)
        {
            if (unit == null || unit.IsDisposed || unit.IsRobot())
            {
                return;
            }
            LogHelper.CheckZuoBi(unit);
        }

        public static void LogMoneyAdd(Unit unit, int type, long gold, int getWay, string roleName, string paramsifo)
        {
            if (unit == null)
            {
                return;
            }

            if (gold < 0)
            {
                Log.Warning($"增加货币出错:{type}  {unit.Id} {getWay} {roleName}  {gold}", true);
            }
            else if (getWay != ItemGetWay.PickItem || gold > 1000)
            {
                LogHelper.LogWarning($"增加货币:{type} {unit.Id} {getWay} {roleName}  {gold}", true);
            }

            if (gold > 1000000 || gold < -1000000)
            {
                Log.Warning($"增加货币[超额]:{type} {unit.Id} {getWay} {roleName} {gold}", true);
            }
            else if (gold > 100000 || gold < -100000)
            {
                Log.Warning($"增加货币[大额]:{type} {unit.Id} {getWay} {roleName} {gold}  {paramsifo}", true);
            }

            if (type == UserDataType.Diamond)
            {
                Log.Warning($"增加钻石: {type} {unit.Id} {getWay} {roleName} {gold}");
            }
        }

        public static void LogMoneySub(Unit unit, int type, long gold, int getWay, string roleName)
        {
            if (unit == null)
            {
                return;
            }

            if (gold > 0)
            {
                LogHelper.LogWarning($"扣除货币出错:{type} {unit.Id} {getWay} {roleName}  {gold}", true);
            }
            else
            {
                LogHelper.LogWarning($"扣除货币:{type} {unit.Id} {getWay} {roleName} {gold}", true);
            }

            if (gold > 100000 || gold < -100000)
            {
                Log.Warning($"扣除货币[大额]:{type} {unit.Id} {getWay} {roleName} {gold}");
            }

            if (type == UserDataType.Diamond)
            {
                Log.Warning($"扣除钻石: {type} {unit.Id} {getWay} {roleName} {gold}");
            }
        }

        public static void LogPiLaoRecover(Unit unit, string tag, int hour1, int hour2, List<int> indexids, int recoverTili)
        {
            if (unit == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(unit.Id).Append(' ').Append(tag)
                .Append(" : hour_1: ").Append(hour1)
                .Append("  hour_2:").Append(hour2)
                .Append("   indexs: ");
            for (int i = 0; i < indexids.Count; i++)
            {
                sb.Append(indexids[i]).Append("   ");
            }
            sb.Append("recover: ").Append(recoverTili);
            Log.Debug(sb.ToString());
        }

        public static void LogShenShouSuspect(Unit unit, RoleInfo roleInfo, int rechargeNumber)
        {
            if (unit == null || roleInfo == null)
            {
                return;
            }
            LogHelper.GongZuoShi($"神兽作弊: {unit.DomainZone()}   \t名称:{roleInfo.Name}  " +
                $"\t等级:{roleInfo.Lv}" + $"\t钻石:{RoleCurrencyHelper.Get(roleInfo, UserDataType.Diamond)}" +
                $"\t充值:{rechargeNumber}");
        }

        public static void LogSession(Unit unit, RoleInfoComponentServer roleInfo, string action)
        {
            if (unit == null || roleInfo == null || unit.IsRobot())
            {
                return;
            }

            string info = $"{unit.DomainZone()}区： " +
               $"unit.id: {roleInfo.Id} : " +
               $" {roleInfo.RoleInfo.Name} : " +
               $"{TimeHelper.DateTimeNow().ToString()}   {action}";
            LogHelper.LoginInfo(info);
            if (action == "离线" || action == "登录")
            {
                Log.Warning(info);
            }
            else if (action == "移除")
            {
                LogHelper.LogDebug(info);
            }
            else
            {
                Log.Debug(info);
            }
        }
    }
}
