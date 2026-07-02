using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDAttributeCategory
    {
        private List<int> showIdList = new List<int>();

        public override void AfterEndInit()
        {
            this.showIdList = this.GetAll().Keys.ToList();  
        }

        public List<int> GetShowIdList()
        {
            return this.showIdList;
        }


        /// <summary>0=固定值，1=万分比。未配置时默认固定值。</summary>
        public int GetValueType(int attributeId)
        {
            if (!this.Contain(attributeId))
            {
                return AttributeValueType.Fixed;
            }

            return this.Get(attributeId).Type;
        }

        public bool IsPerMyriad(int attributeId)
        {
            return this.GetValueType(attributeId) == AttributeValueType.PerMyriad;
        }

        public bool IsFixed(int attributeId)
        {
            return this.GetValueType(attributeId) == AttributeValueType.Fixed;
        }
    }
}