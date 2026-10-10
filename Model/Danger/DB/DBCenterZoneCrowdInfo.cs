using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    /// <summary>
    /// 中心库。一个区一条，Id 为区服 Id。只保留拥挤窗口内的去重登录。
    /// </summary>
    [BsonIgnoreExtraElements]
    public class DBCenterZoneCrowdInfo : Entity, IAwake
    {
        /// <summary>角色 Id → 最近一次进入游戏的时间。</summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, long> RoleLoginTime = new Dictionary<long, long>();

        /// <summary>十分钟结算后的人数。登录当时不改这个值。</summary>
        public int Count;
    }
}
