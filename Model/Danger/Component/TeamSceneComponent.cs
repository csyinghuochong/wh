using System.Collections.Generic;

namespace ET
{
    public class TeamSceneComponent : Entity, IAwake
    {

        public List<TeamInfo> TeamList = new List<TeamInfo>();

        public const int ApplyMaxCount = 99;

        public Dictionary<long, List<TeamPlayerInfo>> ApplyDict = new Dictionary<long, List<TeamPlayerInfo>>();

        // 队伍 Id -> 本次申请 Id，取消后重新申请不会被旧等待任务提前创建。
        public Dictionary<long, long> OpeningTeams = new Dictionary<long, long>();
    }
}
