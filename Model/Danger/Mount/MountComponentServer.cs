using System.Collections.Generic;

namespace ET
{
    public class MountComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        public List<MountInfo> MountInfos = new List<MountInfo>();

        /// <summary>
        /// C2M_MountRide 记下的骑乘偏好。切图/登录按此恢复上马；战斗下马不清。
        /// </summary>
        public bool WantRide;
    }
}
