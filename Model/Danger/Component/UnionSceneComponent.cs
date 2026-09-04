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

        public Dictionary<long, long> UnionBossList = new Dictionary<long, long>(); 

        public Dictionary<long, long> UnionFubens = new Dictionary<long, long>();   //fubenid->fubeninstanceid


        public Dictionary<long, DBUnionInfo> DBUnionInfos = new Dictionary<long, DBUnionInfo>();

        public bool AllUnionsLoaded;

        public ETTask LoadAllUnionsTask;
    }
}
