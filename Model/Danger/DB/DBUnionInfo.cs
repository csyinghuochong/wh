using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;


namespace ET
{

    [BsonIgnoreExtraElements]
    public class DBUnionInfo : Entity, IAwake
    {
        public UnionInfo UnionInfo = new UnionInfo();

        /// <summary>公会仓库密码，不下发客户端。</summary>
        public string WarehousePassword = string.Empty;

        public List<ShopGoodsItem> MysteryItemInfos = new List<ShopGoodsItem>();

        public long MysteryFreshTime = 0;
    }
}
