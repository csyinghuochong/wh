using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{

    [BsonIgnoreExtraElements]
    public class DBConsignInfo : Entity
    {
        public List<ConsignItemInfo> PaiMaiItemInfos = new List<ConsignItemInfo>();                       

      
        public List<ConsignShopItemInfo> PaiMaiShopItemInfos = new List<ConsignShopItemInfo>();         //商店，

    }
}
