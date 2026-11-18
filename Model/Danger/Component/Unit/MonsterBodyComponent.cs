using System.Collections.Generic;

namespace ET
{
    /// <summary>死亡怪物的尸体寿命及正在查看掉落的玩家，仅保留在当前地图。</summary>
    public class MonsterBodyComponent : Entity, IAwake, IDestroy
    {
        public long Timer;
        public long ExpireTime;
        // 玩家 UnitId -> 打开时的 Gate 会话，断线重登后旧界面不能继续暂停尸体。
        public Dictionary<long, long> Viewers = new Dictionary<long, long>();
    }
}
