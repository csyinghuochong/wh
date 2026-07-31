using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDMonsterCategory
    {

        private Dictionary<int, List<AttributeItem>> monsterInitAttribute = new Dictionary<int, List<AttributeItem>> { };

        private List<AttributeItem> emptyAttribute = new List<AttributeItem>();

        public override void AfterEndInit()
        {

            foreach (LDMonster monster in this.GetAll().Values)
            {
                
                if (!monsterInitAttribute.TryGetValue(monster.Id, out List<AttributeItem> monsterAttrs))
                {
                    monsterAttrs = new List<AttributeItem>();
                    monsterInitAttribute.Add(monster.Id, monsterAttrs);
                }


                string[] attributeList = monster.Attribute.Split("|");
                for (int i = 0; i < attributeList.Length; i++)
                {
                    string[] attribute = attributeList[i].Split("~");
                    int key = int.Parse(attribute[0]);
                    int value = int.Parse(attribute[1]);
                    monsterAttrs.Add(new AttributeItem() { AttributeID = key, AttributeValue = value });
                }
            }
        }

        public List<AttributeItem> GetMonsterAttri(int monsterid)
        {
            monsterInitAttribute.TryGetValue(monsterid, out List<AttributeItem> monsterAttrs);
            if (monsterAttrs == null)
            {
                return emptyAttribute;
            }

            return monsterAttrs;
        }

    }
}