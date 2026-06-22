
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDEquipCategory
    {

        private Dictionary<int, List<HideProList>> EquipAttribute = new Dictionary<int,List<HideProList>> { };

        public override void AfterEndInit()
        {
            foreach (LDEquip ldEquip in this.GetAll().Values)
            {
                List<HideProList> equipAttribute = new List<HideProList>();

                string[] attributeList = ldEquip.Attribute.Split("|");

                for (int i = 0; i < attributeList.Length; i++)
                {
                    string[] attributeInfo = attributeList[i].Split("_");
                    
                    equipAttribute.Add( new HideProList(){ HideID = int.Parse(attributeInfo[0]), HideValue = int.Parse(attributeInfo[1])} );
                }


                EquipAttribute.Add(ldEquip.Id, equipAttribute);
            }
        }

        
        
        public List<HideProList> GetEquipAttribute(int equipId)
        {
            this.EquipAttribute.TryGetValue(equipId, out List<HideProList> hideProLists);
            return hideProLists;
        }
    }
}