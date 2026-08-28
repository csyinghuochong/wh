using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace ET
{
    public partial class StartSceneConfigCategory
    {
        public const int GameZoneIdMax = 1000;

        static readonly object ExpandLock = new object();

        bool gameZonesCloned;
        bool indexesBuilt;

        public MultiMap<int, StartSceneConfig> Gates = new MultiMap<int, StartSceneConfig>();
        
        public Dictionary<int, StartSceneConfig> Queues = new Dictionary<int, StartSceneConfig>();

        public Dictionary<int, StartSceneConfig> YeWai = new Dictionary<int, StartSceneConfig>();

        public MultiMap<int, StartSceneConfig> FuBenWorkScens = new MultiMap<int, StartSceneConfig>();

        public MultiMap<int, StartSceneConfig> ProcessScenes = new MultiMap<int, StartSceneConfig>();
        
        public Dictionary<long, Dictionary<string, StartSceneConfig>> ZoneScenesByName = new Dictionary<long, Dictionary<string, StartSceneConfig>>();

        public StartSceneConfig RealmConfig;

        public StartSceneConfig LocationConfig;

        public StartSceneConfig LoginCenterConfig;

        public StartSceneConfig RechargeConfig;

        public StartSceneConfig RobotConfig;
        
        public List<StartSceneConfig> Robots = new List<StartSceneConfig>();
        
        public List<StartSceneConfig> GetByProcess(int process)
        {
            this.EnsureGameZonesExpanded();
            return this.ProcessScenes[process];
        }
        
        public StartSceneConfig GetBySceneName(int zone, string name)
        {
            this.EnsureGameZonesExpanded();
            return this.ZoneScenesByName[zone][name];
        }

        public bool TryGetBySceneName(int zone, string name, out StartSceneConfig config)
        {
            this.EnsureGameZonesExpanded();
            config = null;
            if (!this.ZoneScenesByName.TryGetValue(zone, out Dictionary<string, StartSceneConfig> dict))
            {
                return false;
            }
            return dict.TryGetValue(name, out config);
        }

        public StartSceneConfig GetRandomFubenWork(int zone)
        {
            this.EnsureGameZonesExpanded();
            List<StartSceneConfig> zonelocaldungeons = StartSceneConfigCategory.Instance.FuBenWorkScens[zone];
            int n = RandomHelper.RandomNumber(0, zonelocaldungeons.Count);
            StartSceneConfig startSceneConfig = zonelocaldungeons[n];
            return startSceneConfig;
        }

        public override void AfterEndInit()
        {
            this.EnsureGameZonesExpanded();
        }

        /// <summary>
        /// 以 Zone&lt;1000 中最小区为模板，按 StartZoneConfig 为其余游戏区克隆 Scene。
        /// 进程号拷贝模板；有 OuterPort 的按 (目标区-模板区)*10 递增。
        /// </summary>
        public void EnsureGameZonesExpanded()
        {
            lock (ExpandLock)
            {
                bool cloned = this.TryCloneMissingGameZones();
                if (!this.indexesBuilt || cloned)
                {
                    this.RebuildIndexes();
                    this.indexesBuilt = true;
                }

                StartZoneConfigCategory.Instance?.RebuildServerItems();
            }
        }

        bool TryCloneMissingGameZones()
        {
            if (this.gameZonesCloned)
            {
                return false;
            }

            StartZoneConfigCategory zoneCategory = StartZoneConfigCategory.Instance;
            if (zoneCategory == null || zoneCategory.GetAll().Count == 0)
            {
                return false;
            }

            int templateZone = int.MaxValue;
            foreach (StartSceneConfig scene in this.GetAll().Values)
            {
                if (scene.Zone <= 0 || scene.Zone >= GameZoneIdMax)
                {
                    continue;
                }

                if (scene.Zone < templateZone)
                {
                    templateZone = scene.Zone;
                }
            }

            if (templateZone == int.MaxValue)
            {
                return false;
            }

            List<StartSceneConfig> templates = new List<StartSceneConfig>();
            HashSet<int> existingGameZones = new HashSet<int>();
            foreach (StartSceneConfig scene in this.GetAll().Values)
            {
                if (scene.Zone <= 0 || scene.Zone >= GameZoneIdMax)
                {
                    continue;
                }

                existingGameZones.Add(scene.Zone);
                if (scene.Zone == templateZone)
                {
                    templates.Add(scene);
                }
            }

            if (templates.Count == 0)
            {
                return false;
            }

            templates.Sort((a, b) => a.Id.CompareTo(b.Id));

            int cloned = 0;
            foreach (StartZoneConfig zoneConfig in zoneCategory.GetAll().Values)
            {
                int zone = zoneConfig.Id;
                if (zone <= 0 || zone >= GameZoneIdMax || zone == templateZone || existingGameZones.Contains(zone))
                {
                    continue;
                }

                for (int i = 0; i < templates.Count; i++)
                {
                    StartSceneConfig template = templates[i];
                    int offset = template.Id - templateZone * 100;
                    if (offset <= 0)
                    {
                        offset = template.Id % 100;
                        if (offset <= 0)
                        {
                            offset = i + 1;
                        }
                    }

                    int newId = zone * 100 + offset;
                    if (this.dict.ContainsKey(newId))
                    {
                        Log.Error($"StartSceneConfig 克隆 Id 冲突: Zone={zone} Id={newId} Name={template.Name}");
                        continue;
                    }

                    StartSceneConfig clone = new StartSceneConfig
                    {
                        Id = newId,
                        Zone = zone,
                        Process = template.Process,
                        SceneType = template.SceneType,
                        Name = template.Name,
                        OuterPort = template.OuterPort <= 0
                            ? 0
                            : template.OuterPort + (zone - templateZone) * 10
                    };
                    clone.EndInit();
                    this.dict.Add(newId, clone);
                    this.list.Add(clone);
                    cloned++;
                }

                existingGameZones.Add(zone);
            }

            this.gameZonesCloned = true;
            if (cloned > 0)
            {
                Log.Console($"StartSceneConfig 模板区={templateZone} 动态生成 Scene {cloned} 条");
            }

            return cloned > 0;
        }

        void RebuildIndexes()
        {
            this.Gates.Clear();
            this.Queues.Clear();
            this.FuBenWorkScens.Clear();
            this.ProcessScenes.Clear();
            this.ZoneScenesByName.Clear();
            this.Robots.Clear();
            this.RealmConfig = null;
            this.LocationConfig = null;
            this.LoginCenterConfig = null;
            this.RechargeConfig = null;
            this.RobotConfig = null;

            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values)
            {
                this.ProcessScenes.Add(startSceneConfig.Process, startSceneConfig);
                if (!this.ZoneScenesByName.ContainsKey(startSceneConfig.Zone))
                {
                    this.ZoneScenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.ZoneScenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);

                switch (startSceneConfig.Type)
                {
                    case SceneType.Gate:
                        this.Gates.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.FubenWork:
                        this.FuBenWorkScens.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Queue:
                        this.Queues.Add(startSceneConfig.Zone, startSceneConfig);
                        break;
                    case SceneType.Location:
                        this.LocationConfig = startSceneConfig;
                        break;
                    case SceneType.Robot:
                        this.Robots.Add(startSceneConfig);
                        break;
                    case SceneType.Realm:
                        this.RealmConfig  = startSceneConfig;
                        break;
                    case SceneType.LoginCenter:
                        this.LoginCenterConfig = startSceneConfig;
                        break;
                    case SceneType.ReCharge:
                        this.RechargeConfig = startSceneConfig;
                        break;
                }
            }
        }
        
    }
    
    public partial class StartSceneConfig: ISupportInitialize
    {
        public long InstanceId;
        
        public SceneType Type;

        public StartProcessConfig StartProcessConfig
        {
            get
            {
                return StartProcessConfigCategory.Instance.Get(this.Process);
            }
        }
        
        public StartZoneConfig StartZoneConfig
        {
            get
            {
                return StartZoneConfigCategory.Instance.Get(this.Zone);
            }
        }

        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint innerIPOutPort;

        public IPEndPoint InnerIPOutPort
        {
            get
            {
                if (innerIPOutPort == null)
                {
                    this.innerIPOutPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.InnerIP}:{this.OuterPort}");
                }

                return this.innerIPOutPort;
            }
        }

        /// <summary>
        /// WebSocket 外网端口 = OuterPort + <see cref="OuterNetDefine.WsPortOffset"/>（与客户端 CommonConfig.WebGlOuterPortOffset 同源）。
        /// HttpListener 前缀绑 InnerIP（不要用 *），本机 127.0.0.1 无需 netsh；
        /// 客户端连 ws://OuterIP:OuterWsPort。
        /// </summary>
        public const int OuterWsPortOffset = OuterNetDefine.WsPortOffset;

        public int OuterWsPort => this.OuterPort + OuterWsPortOffset;

        public string OuterWsPrefix => $"http://{this.StartProcessConfig.InnerIP}:{this.OuterWsPort}/";

        private IPEndPoint outerIPPort;

        // 外网地址外网端口
        public IPEndPoint OuterIPPort
        {
            get
            {
                if (this.outerIPPort == null)
                {
                    this.outerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.OuterPort}");
                }

                return this.outerIPPort;
            }
        }

        public override void BeginInit()
        {
        }

        public override void EndInit()
        {
            this.Type = EnumHelper.FromString<SceneType>(this.SceneType);
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(this.Process, (uint) this.Id);
            this.InstanceId = instanceIdStruct.ToLong();
        }
    }
}