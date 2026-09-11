using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;


namespace ET
{

    [BsonIgnoreExtraElements]
	public class DBDayActivityInfo : Entity, IAwake
	{
		public int LastHour;
		

        /// <summary>
        /// 全服随机商店货架 Key=ShopId（LDShop.Type==9）
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, List<ShopGoodsItem>> GlobalRandomShops = new Dictionary<int, List<ShopGoodsItem>>();

		//首胜记录
		public List<FirstWinInfo> FirstWinInfos = new List<FirstWinInfo>();

    }

}
