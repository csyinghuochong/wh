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
        }

        public List<int> GetShowIdList()
        {
            return this.showIdList;
        }

        public const int ValueTypeFixed = 0;
        public const int ValueTypePerMyriad = 1;

        /// <summary>0=固定值，1=万分比。未配置时默认固定值。</summary>
        public int GetValueType(int attributeId)
        {
            if (!this.Contain(attributeId))
            {
                return ValueTypeFixed;
            }

            return this.Get(attributeId).Type;
        }

        public bool IsPerMyriad(int attributeId)
        {
            return this.GetValueType(attributeId) == ValueTypePerMyriad;
        }

        public bool IsFixed(int attributeId)
        {
            return this.GetValueType(attributeId) == ValueTypeFixed;
        }
    }
}