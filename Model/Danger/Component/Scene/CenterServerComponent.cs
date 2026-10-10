using System.Collections.Generic;

namespace ET
{
    
    /// <summary>
    /// 挂载realmscene上
    /// </summary>
    public class CenterServerComponent : Entity, IAwake, IDestroy
    {
        public long Timer;

        public int TianQITime = 0;
        public int TianQiValue= 0;
        
        public bool IsHoliday;
        public bool StopServer;
        
        public int CheckIndex = 0;
        public DBCenterSerialInfo DBCenterSerialInfo;

        public Dictionary<string, KeyValuePair<long, string>> PhoneVerification = new Dictionary<string, KeyValuePair<long, string>>();

        /// <summary>区服 Id → 窗口内去重登录。人数在 DBCenterZoneCrowdInfo.Count。</summary>
        public Dictionary<int, DBCenterZoneCrowdInfo> ZoneCrowdInfos = new Dictionary<int, DBCenterZoneCrowdInfo>();

        public int CrowdTick;
    }
}