using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 收藏：文档 Id = 收藏者 UserId。不进内存缓存，按人直接读写库。
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DBConsignCollect : Entity, IAwake
    {
        public List<long> CollectIds = new List<long>();
    }
}
