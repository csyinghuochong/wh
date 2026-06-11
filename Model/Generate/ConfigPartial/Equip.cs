
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class EquipCategory
    {

        private Dictionary<int, List<HideProList>> EquipAttribute = new Dictionary<int,List<HideProList>> { };

        public override void AfterEndInit()
        {
            foreach (Equip occupation in this.GetAll().Values)
            {
                List<HideProList> equipAttribute = new List<HideProList>();
 
                equipAttribute.Add( new HideProList() { HideID = NumericType.PATK_Max, HideValue = occupation.Equip_MaxAct});
           
                EquipAttribute.Add(occupation.Id, equipAttribute);
            }
        }

        public List<HideProList> GetEquipAttribute(int equipId)
        {
            this.EquipAttribute.TryGetValue(equipId, out List<HideProList> hideProLists);
            return hideProLists;
        }
    }
}