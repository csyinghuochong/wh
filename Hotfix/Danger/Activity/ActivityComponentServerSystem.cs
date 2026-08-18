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
            ActivityHelper.EnsureSignInLoginDay(self.ActivityInfo,  role.LastLoginTime, role.RoleInfo.CreateTime);

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
