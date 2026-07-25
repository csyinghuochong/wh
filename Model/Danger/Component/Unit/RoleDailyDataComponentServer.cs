using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 角色日清：零点重置 Numeric + <see cref="RoleDailyData"/>。
    /// 个人随机商店货架（Type 2/3）也挂这里，每日清；终身限购在 RoleInfo.BuyStoreItemsForever。
    /// </summary>
    public class RoleDailyDataComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        public RoleDailyData Data = new RoleDailyData();

        /// <summary>个人随机商店货架：Key=ShopId，日清重置后按需重新生成。</summary>
        public Dictionary<int, List<MysteryItemInfo>> PersonalRandomShops = new Dictionary<int, List<MysteryItemInfo>>();

        public const int ReasonFull = 1;
        public const int ReasonShopLimit = 2;
        public const int ReasonZeroClock = 3;

        public long LastResetTime = 0;
    }
}
