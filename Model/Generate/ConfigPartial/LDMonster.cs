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
                
                monsterAttrs.Add( new AttributeItem() { AttributeID = NumericType.HP_Fixed_11, AttributeValue = 10});
                monsterAttrs.Add( new AttributeItem() { AttributeID = NumericType.Speed_Current_15, AttributeValue = 5 * 10000});
                monsterAttrs.Add( new AttributeItem() { AttributeID = NumericType.PATK_Max_22, AttributeValue = 1});
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