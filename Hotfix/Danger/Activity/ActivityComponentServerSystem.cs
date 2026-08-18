using System;
using System.Linq;
using System.Collections.Generic;

namespace ET
{
    public static class ActivityComponentServerSystem
    {

        /// <summary>
        /// 取到当前可以领取的最小等级
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetCurActivityId(this ActivityComponentServer self, int rechargeNumb)
        {
            int activityId = 0;
            List<LDActivity> activityConfigs = LDActivityCategory.Instance.GetAll().Values.ToList();
           
            return activityId;
        }


        public static void OnLogin(this ActivityComponentServer self, int level)
        {
        }

        public static void OnDailyReset(this ActivityComponentServer self, int level)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();

            // 每个游戏日首次进入时登录天数 +1；跨组（进度组 != 当前组）时重置
            long createTime = role.RoleInfo.CreateTime;
            long lastLoginTime = role.LastLoginTime;
            long now = TimeHelper.ServerNow();
            int groupNow = ActivityHelper.GetCurrentSignInGroup(createTime, ActivityHelper.DailySignActivityId, now);
            int groupOld = ActivityHelper.GetSignInProgressGroup(self.ActivityInfo);
            if (groupOld <= 0 && lastLoginTime > 0 && self.ActivityInfo.SignInLoginDays > 0)
            {
                groupOld = ActivityHelper.GetCurrentSignInGroup(createTime, ActivityHelper.DailySignActivityId, lastLoginTime);
            }

            if (self.ActivityInfo.SignInLoginDays <= 0 || (groupOld > 0 && groupOld != groupNow))
            {
                self.ActivityInfo.SignInLoginDays = 1;
                self.ActivityInfo.SignInReceivedId = 0;
            }
            else
            {
                self.ActivityInfo.SignInLoginDays += 1;
            }

            Console.WriteLine($"SignInLoginDays: {self.ActivityInfo.SignInLoginDays}");

            //重置每日特惠 和 新春活动
            for (int i = self.ActivityReceiveIds.Count - 1; i >= 0; i--)
            {
            }
        }

        public static void ClearJieRiActivty(this ActivityComponentServer self)
        {
            for (int i = self.ActivityReceiveIds.Count - 1; i >= 0; i--)
            {
              
            }
        }

        public static void Check(this ActivityComponentServer self)
        {
            self.LastTimerChouKaPassTime += TimeHelper.Second;
        }

    }
}
