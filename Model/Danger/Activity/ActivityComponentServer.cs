using System.Collections.Generic;

namespace ET
{

    
    public class ActivityComponentServer : Entity , IAwake, ITransfer, IUnitCache, IDestroy
    {
        /// <summary>
        /// 上次签到时间
        /// </summary>
        public long LastSignTime = 0;
        /// <summary>
        /// 已经签到次数
        /// </summary>
        public int TotalSignNumber = 0;

        public long LastLoginTime = 0;

        //每日特惠
        public List<int> DayTeHui = new List<int>();

        //定时抽奖[距离上次抽奖过去的时候]
        public long LastTimerChouKaPassTime = 0;
        
        public int TimerChouKaReceiveIndex = 0;

        public List<int> ActivityReceiveIds = new List<int>();
        /// <summary>
        /// 令牌领取
        /// </summary>
        public List<TokenRecvive> QuTokenRecvive = new List<TokenRecvive>();

        public List<int> ZhanQuReceiveIds = new List<int>();

        public ActivityV1Info ActivityV1Info = new ActivityV1Info();
        
        
        /*
        public const int V1TotalPoints = 3198;                               //活动周期累计积分
        public const int V1PointsChouKaIndex = 3199;
        public const int GoldWeeklyCard = 3200;                                 //黄金周卡开始时间
        public const int DiamondWeeklyCard = 3201;                              //钻石周卡ComHelp.GetDayByTime
        public const int RechargeType = 3202;                                    //0充值钻石   1购买周卡
        public const int WeChatOABind = 3197;
        */
    }
}
