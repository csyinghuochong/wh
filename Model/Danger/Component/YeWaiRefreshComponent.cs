using System.Collections.Generic;

namespace ET
{
    public struct RefreshMonster
    {
        /// <summary>Scene_Creature刷新点ID；0沿用原有按怪物ID/坐标的刷新方式。</summary>
        public int SceneCreateId;
        public int MonsterId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public long NextTime;
        public float Range;
        public int Number;
        public long Interval;    //-1只刷新一次
        public int Rotation;
    }

    public class YeWaiRefreshComponent : Entity, IAwake, IDestroy
    {         
        public long Timer;
        public int SceneId;
        public int RandomTime;
        public List<RefreshMonster> RefreshMonsters = new List<RefreshMonster>();

        public bool LogTest;
    }
}
