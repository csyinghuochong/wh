
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDEquipCategory
    {

        private Dictionary<int, List<AttributeRandom>> EquipAttribute = new Dictionary<int,List<AttributeRandom>> { };

        public override void AfterEndInit()
        {
            foreach (LDEquip ldEquip in this.GetAll().Values)
            {
                List<AttributeRandom> equipAttribute = new List<AttributeRandom>();

                string[] attributeList = ldEquip.Attribute.Split("|");

                for (int i = 0; i < attributeList.Length; i++)
                { 
                    string[] attributeInfo = attributeList[i].Split("_");
                    if (attributeInfo.Length == 2)
                    {
                        equipAttribute.Add(new AttributeRandom()
                        {
                            AttributeID = int.Parse(attributeInfo[0]),
                            AttributeValueMin = int.Parse(attributeInfo[1]),
                            AttributeValueMax = int.Parse(attributeInfo[1]),
                        });
                    }
                    if (attributeInfo.Length == 3)
                    {
                        equipAttribute.Add(new AttributeRandom()
                        {
                            AttributeID = int.Parse(attributeInfo[0]),
                            AttributeValueMin = int.Parse(attributeInfo[1]),
                            AttributeValueMax = int.Parse(attributeInfo[2]),
                        });
                    }
                }


                EquipAttribute.Add(ldEquip.Id, equipAttribute);
            }
        }

        
        public List<AttributeItem> GetEquipAttribute(int equipId)
        {
            this.EquipAttribute.TryGetValue(equipId, out List<AttributeRandom> hideRandom);

            if (hideRandom == null)
            {
                return null;
             }

            List<AttributeItem>  hideProLists1 = new List<AttributeItem>();
            for (int i = 0; i < hideRandom.Count; i++)
            {
                int getValue = RandomHelper.RandomNumber((int)hideRandom[i].AttributeValueMin, (int)hideRandom[i].AttributeValueMax + 1 );
                hideProLists1.Add ( new AttributeItem()
                {
                    AttributeID = hideRandom[i].AttributeID,
                    AttributeValue = getValue,   
                }); 
            }


            return hideProLists1;
        }
    }
}