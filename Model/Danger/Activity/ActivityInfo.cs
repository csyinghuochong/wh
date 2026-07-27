using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 活动通用进度（签到 / 登录奖励 / 每日特惠），挂在 ActivityComponentServer 上。
    /// </summary>
    public class ActivityInfo
    {
        /// <summary>上次签到时间</summary>
        public long LastSignTime;

        /// <summary>已经签到次数</summary>
        public int TotalSignNumber;

        /// <summary>每日签到：当前已领取的 Activity_Sign_In.Id</summary>
        public int SignInReceiveId;

        /// <summary>上次领取登录奖励时间</summary>
        public long LastLoginTime;

        /// <summary>每日特惠活动 Id 列表</summary>
        public List<int> DayTeHui = new List<int>();
    }
}
