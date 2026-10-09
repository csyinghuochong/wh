using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 角色周期数据。日清走 M2C_RoleDailyDataUpdate。
    /// 本次购买数量在 BuyStoreItems，按商店 Auto_Refresh 清；终身购买数量在 RoleInfo.BuyStoreItemsForever，不清。
    /// </summary>
    public class RoleDailyDataComponentServer : Entity, IAwake, ITransfer, IUnitCache, IDeserialize
    {
        public RoleDailyData Data = new RoleDailyData();

        /// <summary>个人随机商店货架：Key=ShopId。Type 2/3，按该商店 Auto_Refresh 到点后重新生成。</summary>
        public Dictionary<int, List<ShopGoodsItem>> PersonalRandomShops = new Dictionary<int, List<ShopGoodsItem>>();

        /// <summary>各商店上次刷新时间 Key=ShopId。个人店在请求时比较，全服店只用来清玩家本次购买数量。</summary>
        public Dictionary<int, long> ShopRefreshTime = new Dictionary<int, long>();

        /// <summary>在线跨过日清点。客户端覆盖缓存后，再刷任务和活动。其它 Reason 客户端不看。</summary>
        public const int ReasonZeroClock = 3;

        public long LastResetTime = 0;
    }
}