using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 1进入家园 2收获植物 3收获动物  4清理 
    /// </summary>
    public static class HomeOperateType
    {
        public const int Visit = 1;
        public const int GatherPlant = 2;
        public const int GatherPasture = 3;
        public const int Pick = 4;
    }

    public class HomeComponentServer : Entity, IAwake, IDestroy, ITransfer, IDeserialize, IUnitCache
    {

        public long RefreshMonsterTime_2 = 0;

        public long HomeDaShiTime_1 = 0;

        public long HomeFuJinTime_3 = 0;

        public List<int> PlanOpenList_7 = new List<int>();

        public List<int> LearnMakeIds_7 = new List<int>();

        /// <summary>
        /// 家园植物
        /// </summary>
        public List<HomePlant> HomePlantList_7 = new List<HomePlant>();

        /// <summary>
        /// 家园动物
        /// </summary>
        public List<HomePastures> HomePastureList_7 = new List<HomePastures>();

        public int NowOpenNpcId;

        /// <summary>
        /// 家园等级。不走 RoleInfo / RoleDataUpdate，进入家园时由 Init 下发。
        /// </summary>
        public int HomeLv;

        /// <summary>
        /// 家园经验。
        /// </summary>
        public long HomeExp;

        /// <summary>
        /// 家园资金。
        /// </summary>
        public long HomeFund;
        
    }
}
