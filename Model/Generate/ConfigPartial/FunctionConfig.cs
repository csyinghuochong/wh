using System.Collections.Generic;
namespace ET
{
    public partial class FunctionConfigCategory
    {

        public override void AfterEndInit()
        {
            foreach (FunctionConfig activityConfig in this.GetAll().Values)
            {
               
            }
        }
    }
}
