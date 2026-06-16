using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDMonsterCategory
    {

        private Dictionary<int, List<HideProList>> monsterInitAttribute = new Dictionary<int, List<HideProList>> { };

        private List<HideProList> emptyAttribute = new List<HideProList>();

        public override void AfterEndInit()
        {

            foreach (LDMonster monster in this.GetAll().Values)
            {
                
                if (!monsterInitAttribute.TryGetValue(monster.Id, out List<HideProList> monsterAttrs))
                {
                    monsterAttrs = new List<HideProList>();
                    monsterInitAttribute.Add(monster.Id, monsterAttrs);
                }
                
                monsterAttrs.Add( new HideProList() { HideID = NumericType.HP_Fixed, HideValue = 10});
                monsterAttrs.Add( new HideProList() { HideID = NumericType.Speed_Current, HideValue = 5 * 10000});
                monsterAttrs.Add( new HideProList() { HideID = NumericType.PATK_Max, HideValue = 1});
            }
        }

        public List<HideProList> GetMonsterAttri(int monsterid)
        {
            monsterInitAttribute.TryGetValue(monsterid, out List<HideProList> monsterAttrs);
            if (monsterAttrs == null)
            {
                return emptyAttribute;
            }

            return monsterAttrs;
        }

    }
}