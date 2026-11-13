using System.Collections.Generic;

namespace ET
{
    public class UnionSceneComponent : Entity, IAwake, IDestroy
    {
        public long Timer;

        public long WinUnionId;

        public long UnionRaceSceneId;
        public long UnionRaceSceneInstanceId;

        public DBUnionManager DBUnionManager;

        public Dictionary<long, List<long>> UnionRaceUnits = new Dictionary<long, List<long>>();


        public Dictionary<long, long> UnionFubens = new Dictionary<long, long>();   //fubenid->fubeninstanceid


        public Dictionary<long, DBUnionInfo> DBUnionInfos = new Dictionary<long, DBUnionInfo>();

        public bool AllUnionsLoaded;

        public ETTask LoadAllUnionsTask;

        // 角色 -> 当前会话验密凭证；成员对象用于阻止退会重入后复用旧凭证。
        public Dictionary<long, (long UnionId, string Token, UnionPlayerInfo Member)> WarehouseAccess
            = new Dictionary<long, (long UnionId, string Token, UnionPlayerInfo Member)>();
    }
}
