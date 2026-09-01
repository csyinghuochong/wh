using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    
    public class RecastPathComponent : Entity, IAwake, ILoad
    {
        public const string Map1NavDataPath = "../Config/RecastNavData/1.bin";

        /// <summary>
        /// 寻路处理者（可用于拓展多线程，参考A*插件）
        /// key 为 RecastDll 地图 id（Scenes_300 -> 300）
        /// </summary>
        public Dictionary<int, RecastPathProcessor> m_RecastPathProcessorDic = new Dictionary<int, RecastPathProcessor>();

        /// <summary>
        /// 初始化寻路引擎
        /// </summary>
        public void Awake( )
        {
            if (!ConfigData.OldNavMesh)
            {
                return;
            }

            RecastInterface.Init();

            Dictionary<int, LDScene> sceneConfigs = LDSceneCategory.Instance.GetAll();
            foreach (var sceneConfig in sceneConfigs)
            {
                //Update(sceneConfig.Key);
            }
        }

        /// <summary>
        /// 寻路
        /// </summary>
        public void SearchPath(string navName, Vector3 from, Vector3 to, List<Vector3> result, int unitType)
        {
            RecastPathProcessor processor = GetRecastPathProcessor(navName);
            if (processor == null)
            {
                return;
            }

            processor.CalculatePath(from, to, result, unitType);
        }

        public RecastPathProcessor GetRecastPathProcessor(string navName)
        {
            return GetRecastPathProcessor(RecastFileReader.ToNavId(navName));
        }

        public RecastPathProcessor GetRecastPathProcessor(int mapId)
        {
            if (this.m_RecastPathProcessorDic.TryGetValue(mapId, out var recastPathProcessor))
            {
                return recastPathProcessor;
            }

            Log.Error($"未找到地图id为{mapId}的recastPathProcessor");
            return null;
        }

        /// <summary>
        /// 加载一个Map的数据
        /// </summary>
        public void LoadMapNavData(int mapId, char[] navDataPath)
        {
            if (mapId <= 0)
            {
                Log.Error($"LoadMapNavData mapId invalid");
                return;
            }

            if (m_RecastPathProcessorDic.ContainsKey(mapId))
            {
                return;
            }

            if (RecastInterface.LoadMap(mapId, navDataPath))
            {
                RecastPathProcessor recastPathProcessor = this.domain.AddChild<RecastPathProcessor>();
                recastPathProcessor.MapId = mapId;
                m_RecastPathProcessorDic[mapId] = recastPathProcessor;
                Log.Info($"加载Id为{mapId}的地图Nav数据成功！");
            }
            else
            {
                Log.Error($"加载Id为{mapId}的地图Nav数据失败 path={new string(navDataPath)}");
            }
        }

        /// <summary>
        /// 卸载地图数据
        /// </summary>
        /// <param name="mapId">地图Id</param>
        public void UnLoadMapNavData(int mapId)
        {
            if (!m_RecastPathProcessorDic.ContainsKey(mapId))
            {
                return;
            }

            m_RecastPathProcessorDic[mapId].Dispose();
            m_RecastPathProcessorDic.Remove(mapId);
            if (RecastInterface.FreeMap(mapId))
            {
                Log.Info($"地图： {mapId}  释放成功");
            }
            else
            {
                Log.Info($"地图： {mapId}  释放失败");
            }
        }

        public override void Dispose()
        {
            m_RecastPathProcessorDic = new Dictionary<int, RecastPathProcessor>();
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
            //RecastInterface.Fini();
        }

        public void Update(string navName)
        {
            if (!ConfigData.OldNavMesh)
            {
                return;
            }

            int mapId = RecastFileReader.ToNavId(navName);
            string path = RecastFileReader.ResolveNavPath(navName);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Log.Error($"未找到寻路文件 nav={navName} id={mapId} path={path}");
                return;
            }

            LoadMapNavData(mapId, path.ToCharArray());
        }
    }
}
