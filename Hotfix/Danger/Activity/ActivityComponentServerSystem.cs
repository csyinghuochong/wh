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
            if (self.ActivityInfo.DayTeHui.Count == 0)
            {
                self.ActivityInfo.DayTeHui = DayTeHuiHelper.GetDayTeHuiList(2, level);
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

        public static void OnZeroClockUpdate(this ActivityComponentServer self, int level)
        {
            self.ActivityInfo.DayTeHui = DayTeHuiHelper.GetDayTeHuiList(2, level);

            //重置每日特惠 和 新春活动
            for (int i = self.ActivityReceiveIds.Count - 1; i >= 0; i--)
            {

            }

            if (self.ActivityInfo.TotalSignNumber >= 30)
            {
                self.ActivityInfo.TotalSignNumber = 0;

            }

   
            //self.LastTimerChouKaPassTime = 0;
            //self.TimerChouKaReceiveIndex = 0
        }

    }
}
