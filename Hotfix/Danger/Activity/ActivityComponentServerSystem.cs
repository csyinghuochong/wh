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

         /// <summary>
        /// 每周一零点刷新
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notice"></param>
        public static void ActivityV1Reset(this ActivityComponentServer self, bool notice)
        {
            //累计消耗钻石奖励
            //self.ActivityV1Info.ConsumeDiamondReward.Clear();
            //限时活动积分兑换
            self.ActivityV1Info.PointsReward.Clear();
            self.ActivityV1Info.PointsShuxuReward = 0;
            self.ActivityV1Info.GrowthTreeValue = 0;
        }

        public static int GetMaxActivityId(this ActivityComponentServer self, int rechargeNumb)
        {
            int activityId = 0;
           
            return activityId;
        }

        public static void OnLogin(this ActivityComponentServer self, int level)
        {
            if (self.ActivityInfo.DayTeHui.Count == 0)
            {
                self.ActivityInfo.DayTeHui = DayTeHuiHelper.GetDayTeHuiList(2, level);
            }
            if (self.ActivityV1Info.LiBaoAllIds.Count == 0)
            {
                self.ActivityV1Info.LiBaoAllIds = ActivityV1Config.GetLiBaoList( );
            }
            if (string.IsNullOrEmpty(self.ActivityV1Info.ChouKa2ItemList))
            {
                self.ActivityV1Info.ChouKa2ItemList = ActivityV1Config.GetChouKa2RewardList();
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

            self.ActivityV1Info.LiBaoAllIds = ActivityV1Config.GetLiBaoList();
            self.ActivityV1Info.LiBaoBuyIds.Clear();
            self.ActivityV1Info.LastGuessReward.Clear();
            self.ActivityV1Info.ChouKaNumberReward.Clear();

            //self.LastTimerChouKaPassTime = 0;
            //self.TimerChouKaReceiveIndex = 0
        }

    }
}
