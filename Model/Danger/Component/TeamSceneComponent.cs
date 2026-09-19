using System.Collections.Generic;

namespace ET
{
    public class TeamSceneComponent : Entity, IAwake
    {

        public List<TeamInfo> TeamList = new List<TeamInfo>();

        public M2C_TeamDungeonQuitMessage M2C_TeamDungeonQuitMessage = new M2C_TeamDungeonQuitMessage();
    }
}
