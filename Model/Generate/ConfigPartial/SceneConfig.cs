using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    public partial class LDSceneCategory
    {

        public List<int> NpcIdList = new List<int>();

        public override void AfterEndInit()
        {
            foreach (LDScene sceneConfig in this.GetAll().Values)
            {
                if (sceneConfig.Id == 101)
                {
                    InitMainNpc(sceneConfig);
                }
            }
        }

        public void InitMainNpc(LDScene ldScene)
        {
            int[] npcids = ldScene.NpcList;
            for (int i = 0; i < npcids.Length; i++)
            {
                NpcIdList.Add(npcids[i]);
            }
        }
    }
}