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
    }
}