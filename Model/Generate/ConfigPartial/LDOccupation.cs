using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDOccupationCategory
    {

        private Dictionary<int, List<AttributeItem>> OccInitAttribute = new Dictionary<int,List<AttributeItem>> { };

        public override void AfterEndInit()
        {
            foreach (LDOccupation occupation in this.GetAll().Values)
            {
                if (!OccInitAttribute.ContainsKey(occupation.Id))
                {
                    OccInitAttribute.Add(occupation.Id, new List<AttributeItem>());
                    OccInitAttribute[occupation.Id].Add(new AttributeItem() { AttributeID = NumericType.Speed_Fixed_16, AttributeValue = occupation.Speed });
                }
            }
        }

        public List<AttributeItem> GetOccInitAttribute(int occ)
        {
            this.OccInitAttribute.TryGetValue(occ, out List<AttributeItem> hideProLists);
            return hideProLists;
        }
    }
}