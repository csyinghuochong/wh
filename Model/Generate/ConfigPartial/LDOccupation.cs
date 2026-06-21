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
                if (!OccInitAttribute.TryGetValue(occupation.Id, out List<HideProList> occInitAttris))
                {
                    occInitAttris = new List<HideProList>();
                    OccInitAttribute.Add(occupation.Id, occInitAttris);
                }

                string[] attributeList = occupation.Attribute_Init.Split("|");
                for (int i = 0; i < attributeList.Length; i++)
                {
                    string[] attribute = attributeList[i].Split("_");
                    int key = int.Parse(attribute[0]);
                    int value = int.Parse(attribute[1]);
                    occInitAttris.Add( new HideProList(){ HideID = key, HideValue = value} );
                }
            }
        }

        public List<HideProList> GetOccInitAttribute(int occ)
        {
            this.OccInitAttribute.TryGetValue(occ, out List<HideProList> hideProLists);
            return hideProLists;
        }
    }
}