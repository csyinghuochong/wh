using System.Collections.Generic;

namespace ET
{

    public partial class LDEquip_SuitCategory
    {

        public Dictionary<int, List<int>> OccSuiList = new Dictionary<int, List<int>>();

        public override void AfterEndInit()
        {
            foreach (LDEquip_Suit suitConfig in this.GetAll().Values)
            {
                
            }
        }
    }
}
