using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDSectionCategory
    {

        /// <summary>
        /// 神秘副本
        /// </summary>
        public List<int> MysteryDungeonList = new List<int>() { };

        /// <summary>
        /// 神秘副本权重
        /// </summary>
        public Dictionary<int, List<int>> MysteryWeights = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> MysteryDungeon = new Dictionary<int, List<int>>();


        public override void AfterEndInit()
        {

            foreach (LDSection functionConfig in this.GetAll().Values)
            {
                MysteryWeights.Add(functionConfig.Id, new List<int>());
                MysteryDungeon.Add(functionConfig.Id, new List<int>());
                /*string[] shenminds = functionConfig.ShenMiEnterID.Split('|');
                for (int i = 0; i < shenminds.Length; i++)
                {
                    string[] shenminfuben = shenminds[i].Split('&');
                    if (shenminfuben.Length < 2)
                    {
                        Log.Error($"shenminfuben.Length < 2: {shenminds[i]}");
                        continue;
                    }

                    if (!int.TryParse(shenminfuben[0], out int weight))
                    {
                        Log.Error($"int.TryParse error: {shenminfuben[0]}");
                        continue;
                    }

                    if (!int.TryParse(shenminfuben[1], out int mysteryDungeonId))
                    {
                        Log.Error($"int.TryParse error: {shenminfuben[1]}");
                        continue;
                    }

                    MysteryWeights[functionConfig.Id].Add(weight);
                    MysteryDungeon[functionConfig.Id].Add(mysteryDungeonId);
                    MysteryDungeonList.Add(mysteryDungeonId);
                }*/
            }
        }

        public int GetMysteryDungeon(int chapterId)
        {
            List<int> weights = MysteryWeights[chapterId];
            int index = RandomHelper.RandomByWeight(weights);

            return MysteryDungeon[chapterId][index];
        }
    }
}
