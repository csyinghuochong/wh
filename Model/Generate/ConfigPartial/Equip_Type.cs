using System.Collections.Generic;

namespace ET
{
    public partial class LDEquip_TypeCategory
    {
        public override void AfterEndInit()
        {
            foreach (LDEquip_Type equipType in GetAll().Values)
            {

            }
        }
    }
}