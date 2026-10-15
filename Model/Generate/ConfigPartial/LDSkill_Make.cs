using System.Collections.Generic;

namespace ET
{
    public partial class LDSkill_Make
    {
        /// <summary>Consume 拆好的材料，配置加载后填一次。</summary>
        public List<RewardItem> ConsumeItems;

        public void ParseConsume()
        {
            if (this.ConsumeItems == null)
            {
                this.ConsumeItems = new List<RewardItem>();
            }
            else
            {
                this.ConsumeItems.Clear();
            }

            if (string.IsNullOrEmpty(this.Consume) || this.Consume == "0")
            {
                return;
            }

            string[] parts = this.Consume.Replace('^', '~').Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                string[] itemInfo = parts[i].Split('~');
                if (itemInfo.Length < 3
                    || !int.TryParse(itemInfo[0], out int itemType)
                    || !int.TryParse(itemInfo[1], out int itemId)
                    || !int.TryParse(itemInfo[2], out int itemNum)
                    || itemType <= 0
                    || itemId <= 0
                    || itemNum <= 0)
                {
                    continue;
                }

                int itemFlags = 0;
                if (itemInfo.Length >= 4)
                {
                    int.TryParse(itemInfo[3], out itemFlags);
                }

                this.ConsumeItems.Add(new RewardItem
                {
                    ItemType = itemType,
                    ItemID = itemId,
                    ItemNum = itemNum,
                    ItemFlags = itemFlags,
                });
            }
        }

        /// <summary>该行某个产物（Make_Type/Make_Id）单次制造的数量。</summary>
        public int GetOutputNum(int itemType, int itemId)
        {
            if (this.Make_Type_1 == itemType && this.Make_Id_1 == itemId)
            {
                return this.Make_Num_1;
            }

            if (this.Make_Type_2 == itemType && this.Make_Id_2 == itemId)
            {
                return this.Make_Num_2;
            }

            if (this.Make_Type_3 == itemType && this.Make_Id_3 == itemId)
            {
                return this.Make_Num_3;
            }

            if (this.Make_Type_4 == itemType && this.Make_Id_4 == itemId)
            {
                return this.Make_Num_4;
            }

            if (this.Make_Type_5 == itemType && this.Make_Id_5 == itemId)
            {
                return this.Make_Num_5;
            }

            return 0;
        }
    }

    public partial class LDSkill_MakeCategory
    {
        /// <summary>产物 Type+Id → 能造出它的 Skill_Make.Id，Id 升序。</summary>
        /// Make_Id_1  1001100 对应的是 id = 1
        private Dictionary<long, List<int>> productMakeIds;

        public override void AfterEndInit()
        {
            if (this.productMakeIds == null)
            {
                this.productMakeIds = new Dictionary<long, List<int>>();
            }
            else
            {
                this.productMakeIds.Clear();
            }

            List<LDSkill_Make> makes = new List<LDSkill_Make>(this.GetAll().Values);
            makes.Sort((a, b) => a.Id.CompareTo(b.Id));
            for (int i = 0; i < makes.Count; i++)
            {
                LDSkill_Make make = makes[i];
                make.ParseConsume();
                this.AddProduct(make.Make_Type_1, make.Make_Id_1, make.Make_Num_1, make.Id);
                this.AddProduct(make.Make_Type_2, make.Make_Id_2, make.Make_Num_2, make.Id);
                this.AddProduct(make.Make_Type_3, make.Make_Id_3, make.Make_Num_3, make.Id);
                this.AddProduct(make.Make_Type_4, make.Make_Id_4, make.Make_Num_4, make.Id);
                this.AddProduct(make.Make_Type_5, make.Make_Id_5, make.Make_Num_5, make.Id);
            }
        }

        /// <summary>
        /// 材料是否是某条配方的产物（Make_Type/Make_Id），不看是否已学。
        /// 对不上返回 0，对上取 Id 最小的配方。
        /// </summary>
        public int GetProductMakeId(int itemType, int itemId)
        {
            if (this.productMakeIds == null)
            {
                return 0;
            }

            long key = MakeProductKey(itemType, itemId);
            if (!this.productMakeIds.TryGetValue(key, out List<int> makeIds) || makeIds.Count == 0)
            {
                return 0;
            }

            return makeIds[0];
        }

        /// <summary>
        /// 材料类型和 Id 对上 Make_Type/Make_Id，且玩家已学会时，返回那条 Skill_Make.Id。
        /// 对不上或没学会返回 0。同一产物有多行时取 Id 最小的已学行。
        /// </summary>
        public int GetSecondLayerMakeId(int itemType, int itemId, List<int> learnedIds)
        {
            if (learnedIds == null || this.productMakeIds == null)
            {
                return 0;
            }

            long key = MakeProductKey(itemType, itemId);
            if (!this.productMakeIds.TryGetValue(key, out List<int> makeIds))
            {
                return 0;
            }

            for (int i = 0; i < makeIds.Count; i++)
            {
                if (learnedIds.Contains(makeIds[i]))
                {
                    return makeIds[i];
                }
            }

            return 0;
        }

        private void AddProduct(int itemType, int itemId, int outputNum, int makeId)
        {
            if (itemType <= 0 || itemId <= 0 || outputNum <= 0)
            {
                return;
            }

            long key = MakeProductKey(itemType, itemId);
            if (!this.productMakeIds.TryGetValue(key, out List<int> makeIds))
            {
                makeIds = new List<int>();
                this.productMakeIds.Add(key, makeIds);
            }

            if (makeIds.Contains(makeId))
            {
                return;
            }

            makeIds.Add(makeId);
        }

        private static long MakeProductKey(int itemType, int itemId)
        {
            return ((long)itemType << 32) | (uint)itemId;
        }
    }
}
