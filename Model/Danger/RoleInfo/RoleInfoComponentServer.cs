#if SERVER
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
#endif


namespace ET
{

    
    /// <summary>
    /// 角色相关数据
    /// </summary>
    public class RoleInfoComponentServer : Entity, IAwake, IDestroy, ITransfer, IUnitCache
    {
        public RoleInfo RoleInfo = new RoleInfo();

         public string Account;
         public string Password;
          /// <summary>
          /// 登录或者零点刷新的时候会改变.主要用来体力恢复、刷新数据、签到登录天数。
          /// </summary>
        public long LastLoginTime;


        /// <summary>
        /// 今日在线时长
        /// </summary>
        public long TodayOnLine;

        public string RemoteAddress;

        public string UserName;


        public long UpdateCombatTime;

        [BsonIgnore]
        public readonly M2C_RoleDataBroadcast m2C_RoleDataBroadcast  = new M2C_RoleDataBroadcast();
        [BsonIgnore]
        public readonly M2C_RoleDataUpdate m2C_RoleDataUpdate = new M2C_RoleDataUpdate();


        public List<KeyValuePair> Buffs = new List<KeyValuePair>();

    }
}
