using System;
using System.Collections.Generic;

namespace ET
{
    public partial class StartZoneConfigCategory
    {

        private List<ServerItem> _serverItems = new List<ServerItem>();
        public List<ServerItem> ServerItems => _serverItems;

        /// <summary>战区共享 Zone → 成员游戏服 Zone 列表（不含共享区自身）</summary>
        public MultiMap<int, int> WarZoneMembers = new MultiMap<int, int>();

        /// <summary>已配置的战区共享 ZoneId 集合（如 2001、2002）</summary>
        public HashSet<int> WarShareZones = new HashSet<int>();
        
        
        public ServerItem GetServerItem(int id, string ip, string name, long openTime, int show)
        {
            ServerItem serverItem = new ServerItem();
            serverItem.ServerId = id;
            serverItem.ServerIp = ip;
            serverItem.ServerName = name;
            serverItem.ServerOpenTime = openTime;
            serverItem.New = 0;
            serverItem.Show = show;
            return serverItem;
        }

        /// <summary>游戏服 Zone → 战区共享 Zone；0 表示不入战区</summary>
        public int GetWarZone(int zone)
        {
            if (!this.Contain(zone))
            {
                return 0;
            }
            return this.Get(zone).WarZone;
        }

        /// <summary>是否为战区共享区（Id == WarZone 且 WarZone != 0）</summary>
        public bool IsWarShareZone(int zone)
        {
            if (!this.Contain(zone))
            {
                return false;
            }
            StartZoneConfig config = this.Get(zone);
            return config.WarZone != 0 && config.Id == config.WarZone;
        }

        /// <summary>同战区成员游戏服（不含共享区）</summary>
        public List<int> GetWarZoneMembers(int warZone)
        {
            return this.WarZoneMembers[warZone];
        }

        /// <summary>两 Zone 是否同属一个战区（均入战区且 WarZone 相同）</summary>
        public bool IsSameWarZone(int zoneA, int zoneB)
        {
            int warZone = this.GetWarZone(zoneA);
            return warZone != 0 && warZone == this.GetWarZone(zoneB);
        }
        
        public const long DefaultServerOpenTime = 1786791600000;
        public const int GameZoneListPortStart = 20325;

        public override void AfterEndInit()
        {
            this.WarZoneMembers = new MultiMap<int, int>();
            this.WarShareZones.Clear();

            foreach (StartZoneConfig config in this.GetAll().Values)
            {
                if (config.WarZone == 0)
                {
                    continue;
                }

                if (config.Id == config.WarZone)
                {
                    this.WarShareZones.Add(config.Id);
                    continue;
                }

                this.WarZoneMembers.Add(config.WarZone, config.Id);
            }

            this.RebuildServerItems();
            StartSceneConfigCategory.Instance?.EnsureGameZonesExpanded();
        }

        public void RebuildServerItems()
        {
            _serverItems.Clear();

            List<StartZoneConfig> gameZones = new List<StartZoneConfig>();
            foreach (StartZoneConfig config in this.GetAll().Values)
            {
                if (config.Id <= 0 || config.Id >= StartSceneConfigCategory.GameZoneIdMax)
                {
                    continue;
                }

                gameZones.Add(config);
            }

            gameZones.Sort((a, b) => a.Id.CompareTo(b.Id));
            string outerIp = GetListOuterIp();
            for (int i = 0; i < gameZones.Count; i++)
            {
                StartZoneConfig config = gameZones[i];
                int port = GameZoneListPortStart + (config.Id - 1) * 10;
                long openTime = config.OpenTime > 0 ? config.OpenTime : DefaultServerOpenTime;
                _serverItems.Add(GetServerItem(config.Id, $"{outerIp}:{port}", GetZoneDisplayName(config), openTime, 1));
            }
        }

        static string GetListOuterIp()
        {
            StartMachineConfigCategory machines = StartMachineConfigCategory.Instance;
            if (machines == null || machines.GetAll().Count == 0)
            {
                return "127.0.0.1";
            }

            if (machines.Contain(1))
            {
                string ip = machines.Get(1).OuterIP;
                if (!string.IsNullOrEmpty(ip))
                {
                    return ip;
                }
            }

            foreach (StartMachineConfig machine in machines.GetAll().Values)
            {
                if (!string.IsNullOrEmpty(machine.OuterIP))
                {
                    return machine.OuterIP;
                }
            }

            return "127.0.0.1";
        }

        static string GetZoneDisplayName(StartZoneConfig config)
        {
            if (string.IsNullOrEmpty(config.Name))
            {
                return config.Id.ToString();
            }

            return config.Name;
        }
    }
}