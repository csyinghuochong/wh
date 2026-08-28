using System.Collections.Generic;

namespace ET
{
    public class MountComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        public List<MountInfo> MountInfos = new List<MountInfo>();
    }
}
