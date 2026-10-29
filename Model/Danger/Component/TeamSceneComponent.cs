using System.Collections.Generic;

namespace ET
{
    public class TeamSceneComponent : Entity, IAwake
    {

        public List<TeamInfo> TeamList = new List<TeamInfo>();

        public const int ApplyMaxCount = 99;

        public Dictionary<long, List<TeamPlayerInfo>> ApplyDict = new Dictionary<long, List<TeamPlayerInfo>>();
    }
}
