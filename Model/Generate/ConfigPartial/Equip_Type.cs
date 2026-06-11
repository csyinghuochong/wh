using System.Collections.Generic;

namespace ET
{
    public partial class Equip_TypeCategory
    {
        private readonly Dictionary<int, int> subTypeToCaoWei = new Dictionary<int, int>();

        public override void AfterEndInit()
        {
            foreach (Equip_Type equipType in GetAll().Values)
            {
                if (equipType.Type_Sub == null || equipType.Type_Sub.Length == 0)
                {
                    continue;
                }

                foreach (int subType in equipType.Type_Sub)
                {
                    if (subTypeToCaoWei.ContainsKey(subType))
                    {
                        Log.Error($"Equip_Type 子类重复映射槽位: subType={subType}, 已有槽位={subTypeToCaoWei[subType]}, 当前槽位={equipType.Id}");
                        continue;
                    }

                    subTypeToCaoWei.Add(subType, equipType.Id);
                }
            }
        }

        public int GetSubTypeToCaoWei(int subType)
        {
            if (subTypeToCaoWei.TryGetValue(subType, out int caowei))
            {
                return caowei;
            }

            return 0;
        }
    }
}