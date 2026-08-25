using System.Collections.Generic;


namespace ET
{
    public partial class LDItem
    {
        public int GetTypeParam1()
        {
            return ParseTypeParam(this.ItemTypeParam1);
        }

        public int GetTypeParam2()
        {
            return ParseTypeParam(this.ItemTypeParam2);
        }

        static int ParseTypeParam(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return 0;
            }

            return int.TryParse(raw, out int value) ? value : 0;
        }
    }

    public partial class LDItemCategory
    {

        public List<int> FoodList = new List<int> { };
        public Dictionary<int, List<int>> FoodLevelList = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> EquipTypeList = new Dictionary<int, List<int>>(); 

        public override void AfterEndInit()
        {
            LDSkill_BattleCategory skillCategory = LDSkill_BattleCategory.Instance;
            if (skillCategory == null)
            {
                return;
            }

            List<int> ids = skillCategory.PetBookSkillIds;
            ids.Clear();
            HashSet<int> added = new HashSet<int>();
            foreach (LDItem item in this.GetAll().Values)
            {
                if (item.ItemType != ItemTypeEnum.SubType_PetSkillBook_39)
                {
                    continue;
                }

                int skillId = item.GetTypeParam1();
                if (skillId <= 0 || !added.Add(skillId) || !skillCategory.Contain(skillId))
                {
                    continue;
                }

                ids.Add(skillId);
            }
        }

        public int GetRandomEquip(int occ, int subType, int lv)
        {
            List<int> equiplist = null;
            EquipTypeList.TryGetValue(subType, out equiplist);
            if (equiplist == null)
            {
                return 0;
            }
            List<int> canequiplist = new List<int>();
            for (int i = 0; i < equiplist.Count; i++)
            {
                /*if ((Item.EquipType == 1|| Item.EquipType == 2))
                {
                    if (occ == 1)
                    {
                        canequiplist.Add(equiplist[i]);
                    }
                }
                else  if ((Item.EquipType == 3 || Item.EquipType == 4))
                {
                    if (occ == 2)
                    {
                        canequiplist.Add(equiplist[i]);
                    }
                }*/
            }
            if (canequiplist.Count == 0)
            {
                return 0;
            }
            return canequiplist[ RandomHelper.RandomNumber(0, canequiplist.Count) ];
        }

        public int[] GetRandomEquipList(int occ, int lv)
        {
            int[] equipList = new int[13];
            for (int i = 0; i < 13; i++)
            {
                equipList[i] = GetRandomEquip(occ, i, lv); 
            }
            return equipList;
        }

        public int GetFoodId(int lv)
        {
            int templv = 0;

            List<int> foodlist = null;
            FoodLevelList.TryGetValue( lv, out foodlist);

            if (foodlist == null)
            {
                foreach ((int level, List<int> ids) in FoodLevelList)
                {
                    templv = level;
                    if (level >= lv)
                    {
                        foodlist = ids;
                    }
                }
            }
            if (foodlist == null)
            {
                foodlist = FoodLevelList[templv];
            }

            return foodlist[RandomHelper.RandomNumber(0, foodlist.Count)];
        }
    }
}
