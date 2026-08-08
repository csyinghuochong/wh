using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Options;

namespace ET
{

    [BsonIgnoreExtraElements]
    public class DBCenterSerialInfo : Entity, IAwake
    {
        public int SerialIndex = 0;
        public int LastHour = 0;
        public List<KeyValuePair> SerialList = new List<KeyValuePair>();
    }
}
