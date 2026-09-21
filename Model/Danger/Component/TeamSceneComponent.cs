using System.Collections.Generic;

namespace ET
{
    public class TeamSceneComponent : Entity, IAwake
    {

        public List<TeamInfo> TeamList = new List<TeamInfo>();

        public Dictionary<long, List<TeamPlayerInfo>> ApplyDict = new Dictionary<long, List<TeamPlayerInfo>>();
    }
}
