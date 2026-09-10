using System.Collections.Generic;

namespace ET
{

    public class HomeSceneComponent : Entity,IAwake,IDestroy
    {
        public Dictionary<long, long> HomeFubens = new Dictionary<long, long>();
    }
}
