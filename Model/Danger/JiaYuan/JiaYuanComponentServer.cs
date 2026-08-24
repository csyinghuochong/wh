using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 1进入家园 2收获植物 3收获动物  4清理 
    /// </summary>
    public static class JiaYuanOperateType
    {
        public const int Visit = 1;
        public const int GatherPlant = 2;
        public const int GatherPasture = 3;
        public const int Pick = 4;
    }

    public class JiaYuanComponentServer : Entity, IAwake, IDestroy, ITransfer, IDeserialize, IUnitCache
    {

        public long RefreshMonsterTime_2 = 0;

        public long JiaYuanDaShiTime_1 = 0;

        public long JiaYuanFuJinTime_3 = 0;

        public List<int> PlanOpenList_7 = new List<int>();

        public List<int> LearnMakeIds_7 = new List<int>();

        public List<long> JiaYuanFuJins_3 = new List<long>();

        public List<JiaYuanRecord> JiaYuanRecordList_1 = new List<JiaYuanRecord>();

        /// <summary>
        /// 家园收购列表
        /// </summary>
        public List<JiaYuanPurchaseItem> PurchaseItemList_7 = new List<JiaYuanPurchaseItem>();

        /// <summary>
        /// 家园植物
        /// </summary>
        public List<JiaYuanPlant> JianYuanPlantList_7 = new List<JiaYuanPlant>();

        /// <summary>
        /// 家园动物
        /// </summary>
        public List<JiaYuanPastures> JiaYuanPastureList_7 = new List<JiaYuanPastures>();

        /// <summary>
        /// 家园大师
        /// </summary>
        public List<KeyValuePair> JiaYuanProList_7 = new List<KeyValuePair>();


        /// <summary>
        /// 家园农场商店
        /// </summary>
        public List<ShopGoodsItem> PlantGoods_7 = new List<ShopGoodsItem>();

        /// <summary>
        /// 家园牧场商店
        /// </summary>
        public List<ShopGoodsItem> PastureGoods_7 = new List<ShopGoodsItem>();

        /// <summary>
        /// 家园商店
        /// </summary>
        public List<ShopGoodsItem> JiaYuanStore = new List<ShopGoodsItem>();

        /// <summary>
        /// 家园随机怪
        /// </summary>
        //keyValuePair.KeyId    怪物id
        //keyValuePair.Value    怪物出生时间戳
        //keyValuePair.Value2   怪物坐标
        public List<JiaYuanMonster> JiaYuanMonster_2 = new List<JiaYuanMonster>();

        public int NowOpenNpcId;

        /// <summary>
        /// 家园等级。不走 RoleInfo / RoleDataUpdate，进入家园时由 Init 下发。
        /// </summary>
        public int JiaYuanLv;

        /// <summary>
        /// 家园经验。
        /// </summary>
        public long JiaYuanExp;

        /// <summary>
        /// 家园资金。
        /// </summary>
        public long JiaYuanFund;
        
    }
}
