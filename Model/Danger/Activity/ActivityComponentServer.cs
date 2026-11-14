using System.Collections.Generic;

namespace ET
{
    public class ActivityComponentServer : Entity , IAwake, ITransfer, IUnitCache, IDestroy
    {
        /// <summary>签到 / 登录奖励 / 每日特惠</summary>
        public ActivityInfo ActivityInfo = new ActivityInfo();

        //定时抽奖[距离上次抽奖过去的时候]
        public long LastTimerChouKaPassTime = 0;
        
        public int TimerChouKaReceiveIndex = 0;

        /// <summary>已领取的全区通用兑换码 ID，1=TS666666、2=TS888888、3=TS666888；永久保存，不随日清重置。</summary>
        public List<int> ActivityReceiveIds = new List<int>();
        /// <summary>
        /// 令牌领取
        /// </summary>
        public List<TokenRecvive> QuTokenRecvive = new List<TokenRecvive>();

        public List<int> ZhanQuReceiveIds = new List<int>();

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
