using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

namespace ET
{

	[BsonIgnoreExtraElements]
	public class DBAccountBagInfo : Entity, IAwake
	{
        
        public List<BagInfo> BagInfoList = new List<BagInfo>();

        public int HaveItemById(long bagInfoId)
        {
            for (int i = 0; i < BagInfoList.Count; i++)
            {
                if (BagInfoList[i].BagInfoID == bagInfoId)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
