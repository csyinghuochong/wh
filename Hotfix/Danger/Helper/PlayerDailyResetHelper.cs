using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 登录跨天 / 零点刷新编排：统一 Login 与 G2M 零点扇出，避免两处各写一份。
    /// </summary>
    public static class PlayerDailyResetHelper
    {
        /// <summary>
        /// 零点刷新：notice=true 为整点推送，false 为登录补刷。
        /// </summary>
        public static void RunZeroClock(Unit unit, bool notice)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;

            unit.GetComponent<RoleDailyDataComponentServer>()?.OnZeroClockUpdate(notice);
            if (notice)
            {
                roleInfoComponentServer.OnHourUpdate(0, true);
            }
            // 日清列表已在 RoleDailyData.OnZeroClockUpdate 清过；这里只做 RoleInfo 其它跨天逻辑
            roleInfoComponentServer.OnZeroClockUpdate(notice);

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            if (notice)
            {
                taskComponentServer.CheckWeeklyUpdate();
            }
            taskComponentServer.OnZeroClockUpdate(notice);

            unit.GetComponent<ActivityComponentServer>()?.OnZeroClockUpdate(roleInfo.Lv);
            unit.GetComponent<ChengJiuComponentServer>()?.OnZeroClockUpdate();
            unit.GetComponent<JiaYuanComponentServer>()?.OnZeroClockUpdate(notice);
            unit.GetComponent<DataCollationComponent>()?.OnZeroClockUpdate(notice);
        }

        /// <summary>
        /// 登录时按上次登录时间补刷跨天/同天体力与家园经验。
        /// </summary>
        public static void RunLoginCrossDay(Unit unit, long currentTime)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            long lastLoginTime = roleInfoComponentServer.LastLoginTime;
            DateTime dateTime = TimeInfo.Instance.ToDateTime(currentTime);

            if (lastLoginTime == 0)
            {
                Log.Debug($"OnZeroClockUpdate [数据初始化]: {unit.Id}");
                unit.GetComponent<TaskComponentServer>().OnZeroClockUpdate(false);
                return;
            }

            DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastLoginTime);
            if (dateTime.Day != lastdateTime.Day)
            {
                Log.Debug($"OnZeroClockUpdate [登录刷新]: {unit.Id}");
                float passhour = (currentTime - lastLoginTime) * 1f / TimeHelper.Hour;
                RecoverPiLaoAcrossDays(roleInfoComponentServer, unit, lastdateTime, dateTime, passhour, currentTime, lastLoginTime);

                unit.GetComponent<TaskComponentServer>().CheckWeeklyUpdate(lastLoginTime, currentTime);
                RunZeroClock(unit, false);
                roleInfoComponentServer.OnJiaYuanExp(Math.Min(passhour, 12f));
            }
            else
            {
                RecoverPiLaoSameDay(roleInfoComponentServer, unit, lastdateTime.Hour, dateTime.Hour);
                unit.GetComponent<JiaYuanComponentServer>().OnLoginCheck(lastdateTime.Hour, dateTime.Hour);
                float passhour = (currentTime - lastLoginTime) * 1f / TimeHelper.Hour;
                roleInfoComponentServer.OnJiaYuanExp(Math.Min(passhour, 12f));
            }
        }

        private static void RecoverPiLaoAcrossDays(
            RoleInfoComponentServer roleInfoComponentServer,
            Unit unit,
            DateTime lastdateTime,
            DateTime dateTime,
            float passhour,
            long currentTime,
            long lastLoginTime)
        {
            if (passhour >= 24f)
            {
                roleInfoComponentServer.RecoverPiLao(120, false);
                return;
            }

            List<int> indexids_1 = roleInfoComponentServer.GetTiLiIndexsNew(lastdateTime.Hour, 23);
            List<int> indexids_2 = roleInfoComponentServer.GetTiLiIndexsNew(0, dateTime.Hour);
            List<int> indexids = new List<int>();
            indexids.Add(0);
            indexids.AddRange(indexids_1);
            indexids.AddRange(indexids_2);
            if (indexids.Count <= 0)
            {
                return;
            }

            int recoverTili = roleInfoComponentServer.GetTiliRecover(indexids);
            roleInfoComponentServer.RecoverPiLao(recoverTili, false);
            AntiCheatAuditHelper.LogPiLaoRecover(unit, "two day", lastdateTime.Hour, dateTime.Hour, indexids, recoverTili);
        }

        private static void RecoverPiLaoSameDay(
            RoleInfoComponentServer roleInfoComponentServer,
            Unit unit,
            int hour_1,
            int hour_2)
        {
            List<int> indexids = roleInfoComponentServer.GetTiLiIndexsNew(hour_1, hour_2);
            if (indexids.Count <= 0)
            {
                return;
            }

            int recoverTili = roleInfoComponentServer.GetTiliRecover(indexids);
            roleInfoComponentServer.RecoverPiLao(recoverTili, false);
            AntiCheatAuditHelper.LogPiLaoRecover(unit, "one day", hour_1, hour_2, indexids, recoverTili);
        }
    }
}
