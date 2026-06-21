using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDAttributeCategory
    {
        private List<int> showIdList = new List<int>();

        public override void AfterEndInit()
        {
            this.showIdList = this.GetAll().Values
                    .Where(item => item.IsShow == 1)
                    .OrderBy(item => item.Order_SL)
                    .ThenBy(item => item.Id)
                    .Select(item => item.Id)
                    .ToList();
            
            Log.ILog.Debug($" this.showIdList:  { this.showIdList.Count}");
        }

        public List<int> GetShowIdList()
        {
            return this.showIdList;
        }
    }
}