using System.Linq;
using System.Collections.Generic;

namespace ET
{

    public class RecastPathAwakeSystem : AwakeSystem<RecastPathComponent>
    {
        public override void Awake(RecastPathComponent self)
        {
            self.Awake();
        }
    }

    public  static class RecastPathComponentSystem
    {
        // 代码热重载不要调。网格在原生库里，EventSystem.Load 卸掉就没了。只给控制台 R 3。
        public static void OnLoad(this RecastPathComponent self)
        {
            List<int> maps = self.m_RecastPathProcessorDic.Keys.ToList();

            //卸载旧场景
            for (int i = 0; i < maps.Count; i++)
            {
                self.UnLoadMapNavData( maps[i] );
            }

            // 进图时的名字是 Scenes_1，字典里只剩 1。按数字去找会命中别的文件。
            for (int i = 0; i < maps.Count; i++)
            {
                self.Update("Scenes_" + maps[i]);
            }
        }

        public static void OnLoadSingle(this RecastPathComponent self, int mapId)
        {
            if (!self.m_RecastPathProcessorDic.ContainsKey(mapId))
            {
                return;
            }
            self.UnLoadMapNavData(mapId);
            self.Update("Scenes_" + mapId);
        }
    }
}
