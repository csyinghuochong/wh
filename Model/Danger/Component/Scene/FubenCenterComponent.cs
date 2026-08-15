using System.Collections.Generic;

namespace ET
{

    public class BattleInfo : Entity, IAwake
    {
        public int SceneId = 0;
        public long FubenId = 0;
        public int PlayerNumber = 0;
        public long ProgressId = 0;
        public long FubenInstanceId = 0;

        public List<long> Camp1Player = new List<long>();
        public List<long> Camp2Player = new List<long>();
    }


    public class FubenCenterComponent : Entity, IAwake
    {

        public List<long> FubenInstanceList = new List<long>();
        public Dictionary<int, long> YeWaiFubenList = new Dictionary<int, long>();
        public ServerInfo ServerInfo;

        /// <summary>
        /// 战场活动（？？？）。玩家↔实例↔阵营只记在 FubenCenter，中途退出按这份名单回原副本。
        /// </summary>
        public bool BattleOpen = false;
        public List<BattleInfo> BattleInfos = new List<BattleInfo>();

        
    }
}
