using System.Collections.Generic;

namespace ET
{
    public partial class LDMount
    {
        /// <summary>按当前等级取阶段配置（图标/模型）。</summary>
        public LDMount_Stage GetStage(int lv)
        {
            LDMount_StageCategory category = LDMount_StageCategory.Instance;
            if (category == null)
            {
                return null;
            }

            return category.GetStage(this.Id, lv);
        }

        /// <summary>该坐骑全部阶段，已按 Lv_Min 排序。</summary>
        public List<LDMount_Stage> GetStageList()
        {
            LDMount_StageCategory category = LDMount_StageCategory.Instance;
            if (category == null)
            {
                return LDMount_StageCategory.EmptyList;
            }

            return category.GetStages(this.Id);
        }

        /// <summary>按当前等级取坐骑速度（Mount_Speed 全局表）。</summary>
        public int GetSpeed(int lv)
        {
            return LDMount_SpeedCategory.GetSpeed(lv);
        }
    }

    public partial class LDMountCategory
    {
        public LDMount_Stage GetStage(int mountId, int lv)
        {
            if (!this.Contain(mountId))
            {
                return null;
            }

            return this.Get(mountId).GetStage(lv);
        }

        public int GetSpeed(int lv)
        {
            return LDMount_SpeedCategory.GetSpeed(lv);
        }
    }

    public partial class LDMount_StageCategory
    {
        public static readonly List<LDMount_Stage> EmptyList = new List<LDMount_Stage>();

        Dictionary<int, List<LDMount_Stage>> stagesByMount = new Dictionary<int, List<LDMount_Stage>>();

        public override void AfterEndInit()
        {
            this.stagesByMount.Clear();
            foreach (LDMount_Stage stage in this.GetAll().Values)
            {
                if (!this.stagesByMount.TryGetValue(stage.Mount_Id, out List<LDMount_Stage> list))
                {
                    list = new List<LDMount_Stage>();
                    this.stagesByMount.Add(stage.Mount_Id, list);
                }

                list.Add(stage);
            }

            foreach (List<LDMount_Stage> list in this.stagesByMount.Values)
            {
                list.Sort((a, b) => a.Lv_Min.CompareTo(b.Lv_Min));
            }
        }

        public List<LDMount_Stage> GetStages(int mountId)
        {
            if (!this.stagesByMount.TryGetValue(mountId, out List<LDMount_Stage> list) || list == null)
            {
                return EmptyList;
            }

            return list;
        }

        public LDMount_Stage GetStage(int mountId, int lv)
        {
            return FindStageByLv(this.GetStages(mountId), lv);
        }

        public static LDMount_Stage FindStageByLv(List<LDMount_Stage> stages, int lv)
        {
            if (stages == null || stages.Count == 0)
            {
                return null;
            }

            if (lv < 1)
            {
                lv = 1;
            }

            LDMount_Stage fallback = null;
            for (int i = 0; i < stages.Count; i++)
            {
                LDMount_Stage stage = stages[i];
                fallback = stage;
                if (lv >= stage.Lv_Min && lv <= stage.Lv_Max)
                {
                    return stage;
                }
            }

            return fallback;
        }
    }

    public partial class LDMount_SpeedCategory
    {
        List<LDMount_Speed> speedList = new List<LDMount_Speed>();

        public override void AfterEndInit()
        {
            this.speedList.Clear();
            foreach (LDMount_Speed speed in this.GetAll().Values)
            {
                this.speedList.Add(speed);
            }

            this.speedList.Sort((a, b) => a.Lv_Min.CompareTo(b.Lv_Min));
        }

        public LDMount_Speed GetByLv(int lv)
        {
            if (this.speedList.Count == 0)
            {
                return null;
            }

            if (lv < 1)
            {
                lv = 1;
            }

            LDMount_Speed fallback = null;
            for (int i = 0; i < this.speedList.Count; i++)
            {
                LDMount_Speed item = this.speedList[i];
                fallback = item;
                if (lv >= item.Lv_Min && lv <= item.Lv_Max)
                {
                    return item;
                }
            }

            return fallback;
        }

        public static int GetSpeed(int lv)
        {
            LDMount_SpeedCategory category = Instance;
            if (category == null)
            {
                return 0;
            }

            LDMount_Speed config = category.GetByLv(lv);
            return config != null ? config.Speed : 0;
        }
    }
}
