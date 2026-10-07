using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 重要跑马灯。一个区服一份，Id = 区服号。全服公告由各服各存一份。
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DBMarqueeInfo : Entity, IAwake
    {
        public List<MarqueeInfo> ImportantList = new List<MarqueeInfo>();
    }
}
