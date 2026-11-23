using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    public class TeamDungeonComponent : Entity, IAwake, IDestroy
    {
        public long Timer;
        public int FubenType;
        public long EnterTime;
        public TeamInfo TeamInfo;
        public bool IsAbandoning;
        public bool IsCompleted;

        // 当前实例光墙配置与显隐；已击杀记录独立保留，尸体移除不改变整组击杀结果。
        public List<LDScene_Mechanism> Mechanisms = new List<LDScene_Mechanism>();
        public HashSet<int> VisibleMechanismIds = new HashSet<int>();
        public Dictionary<int, HashSet<int>> MechanismGroups = new Dictionary<int, HashSet<int>>();
        public HashSet<int> DeadSceneCreatures = new HashSet<int>();

        // 当前副本场景实例中已成功进入的玩家；离开副本时保留，实例销毁时失效。
        public List<long> EnterPlayers = new List<long>();

        public Dictionary<long, TeamPlayerInfo> TeamPlayers = new Dictionary<long, TeamPlayerInfo>();
       
        public List<int> BoxReward = new List<int>();
        public List<int> KillBossList = new List<int>();

        public Vector3 BossDeadPosition = Vector3.zero;
    }
}
