using System.Collections.Generic;

namespace ET
{
    public class MountComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        public long UseMountId = 0;
        public long RideMountId = 0;
        public List<MountInfo> MountInfos = new List<MountInfo>();
    }
}
