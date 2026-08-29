using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 求购分桶：文档 Id = ConsignHelper.GetWantBuyKey(ItemType, ItemId)
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DBConsignWantBuy : Entity, IAwake
    {
        public List<ConsignWantBuyInfo> WantBuyInfos = new List<ConsignWantBuyInfo>();
    }
}
