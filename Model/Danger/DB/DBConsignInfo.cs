using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{

    [BsonIgnoreExtraElements]
    public class DBConsignInfo : Entity, IAwake
    {

        public List<ConsignItemInfo> PaiMaiItemInfos = new List<ConsignItemInfo>();

        /// <summary>
        /// 本上架分类产生的过期商品，按记录中的 UserId 区分所属角色，与货架一起保存；成交收益不再写入。
        /// 这是交易服暂存数据，当前不是个人仓库；领取确认成功后移除对应记录。
        /// 过期商品仍放在拍卖服的 DBConsignInfo.StorageItems 中：
        /// </summary>
        public List<ConsignStorageInfo> StorageItems = new List<ConsignStorageInfo>();
    }
}
