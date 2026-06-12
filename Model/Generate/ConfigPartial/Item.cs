using System.Collections.Generic;


namespace ET
{
    public partial class LDItemCategory
    {

        public List<int> FoodList = new List<int> { };
        public Dictionary<int, List<int>> FoodLevelList = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> EquipTypeList = new Dictionary<int, List<int>>(); 

        public override void AfterEndInit()
        {
            foreach (LDItem Item in this.GetAll().Values)
            {
                if (Item.ItemType== 1 && Item.ItemSubType== 131)
                {
                    List<int> foodlist = null;
                    FoodLevelList.TryGetValue(Item.UseLv, out foodlist);
                    if (foodlist == null)
                    {
                        foodlist = new List<int>();
                        FoodLevelList.Add(Item.UseLv, foodlist);
                    }
                    foodlist.Add(Item.Id);
                }
               
            }

            foreach (LDItem Item in this.GetAll().Values)
            {
                if (Item.ItemType == 1 && Item.ItemSubType == 131 && Item.Quality > 2)
                {
                    FoodList.Add(Item.Id);
                }
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
                LDItem ldItem = LDItemCategory.Instance.Get(equiplist[i]);
                if (ldItem.ItemSubType != subType ||  ldItem.UseLv > lv)
                {
                    continue;
                }
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
                else
                {
                    canequiplist.Add(equiplist[i]);
                }
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
