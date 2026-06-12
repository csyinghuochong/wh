
using UnityEngine;

namespace ET
{

    [ObjectSystem]
    public class NpcComponentAwakeSystem : AwakeSystem<NpcComponent>
    {
        public override void Awake(NpcComponent self)
        {
            Scene scene = self.GetParent<Scene>();
            //初始化主场景npc
            if (!scene.Name.Contains("Map"))
            {
                return;
            }
            string map = scene.Name.Substring(3, scene.Name.Length - 3);
            self.InitNpc(int.Parse(map));
        }
    }

    public static class NpcComponentSystem
    {

        public static void InitNpc(this NpcComponent self, int sceneId)
        {
            LDScene ldScene = LDSceneCategory.Instance.Get(sceneId);
            int[] npcs =ldScene.NpcList;
            if (npcs == null)
            { 
                return ;
            }
            
            Vector3 vector3 =  new Vector3(-3f, 0f, 3f);
            for (int i = 0; i < npcs.Length; i++)
            {
                if (npcs[i] == 0)
                {
                    continue;
                }
                UnitFactory.CreateNpc(self.DomainScene(), npcs[i],  RandomHelper.GetRandomPointInCircle(vector3, 10f));									
            }
        }

    }
}
