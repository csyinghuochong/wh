using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDOccupationCategory
    {

        private Dictionary<int, List<HideProList>> OccInitAttribute = new Dictionary<int,List<HideProList>> { };

        public override void AfterEndInit()
        {
            foreach (LDOccupation occupation in this.GetAll().Values)
            {
                if (!OccInitAttribute.ContainsKey(occupation.Id))
                {
                    OccInitAttribute.Add(occupation.Id, new List<HideProList>());
                }

                OccInitAttribute[occupation.Id].Add( new HideProList() { HideID = NumericType.HP_Fixed, HideValue = 1000* 10000});
                OccInitAttribute[occupation.Id].Add( new HideProList() { HideID = NumericType.Speed_Fixed, HideValue =5 * 10000});
                OccInitAttribute[occupation.Id].Add( new HideProList() { HideID = NumericType.PATK_Max, HideValue = 50 * 10000 });
            }
        }

        public List<HideProList> GetOccInitAttribute(int occ)
        {
            this.OccInitAttribute.TryGetValue(occ, out List<HideProList> hideProLists);
            return hideProLists;
        }
    }
}