using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;


namespace ET
{

    [BsonIgnoreExtraElements]
	public class DBDayActivityInfo : Entity, IAwake
	{
		/// <summary>上次日清触发的服务器时间（跨 Global_Reset_Time 判定）。</summary>
		public long LastDailyResetTime;
		

        /// <summary>
        /// 全服随机商店货架 Key=ShopId（LDShop.Type==9）
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, List<ShopGoodsItem>> GlobalRandomShops = new Dictionary<int, List<ShopGoodsItem>>();

    }

}
