using System.Collections.Generic;


namespace ET
{
    public partial class LDScene_CreatureCategory
    {

        private Dictionary<int, List<int>> CreatureList = new Dictionary<int,  List<int>>{ };
     
        public override void AfterEndInit()
        {
            foreach (LDScene_Creature Item in this.GetAll().Values)
            {
                if (!this.CreatureList.TryGetValue(Item.Scene_Id , out List<int> creatureList))
                {
                    creatureList = new List<int>();
                    this.CreatureList.Add(Item.Scene_Id , creatureList);
                }

                creatureList.Add(Item.Id);
            }
        }
        
        public List<int> GetSceneCreatureList(int sceneid)
        {
            this.CreatureList.TryGetValue(sceneid, out List<int> creatureList);
            return creatureList;
        }
    }
}
