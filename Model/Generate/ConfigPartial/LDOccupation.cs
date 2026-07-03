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
                if (!OccInitAttribute.TryGetValue(occupation.Id, out List<AttributeItem> occInitAttris))
                {
                    occInitAttris = new List<AttributeItem>();
                    OccInitAttribute.Add(occupation.Id, occInitAttris);
                }

                string[] attributeList = occupation.Attribute_Init.Split("|");
                for (int i = 0; i < attributeList.Length; i++)
                {
                    string[] attribute = attributeList[i].Split("_");
                    int key = int.Parse(attribute[0]);
                    int value = int.Parse(attribute[1]);
                    occInitAttris.Add( new AttributeItem(){ AttributeID = key, AttributeValue = value} );
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