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
        
        public override void AfterEndInit()
        {
            _serverItems.Clear();
            this.WarZoneMembers = new MultiMap<int, int>();
            this.WarShareZones.Clear();
            //_serverItems.Add( GetServerItem( 1, "43.139.108.125:20325", "版号服", 1720782000000, 1 ) );
            //_serverItems.Add( GetServerItem( 2, "43.139.108.125:20335", "内测服", 1720954800000, 0 ) );
            
            _serverItems.Add( GetServerItem( 1, "127.0.0.1:20325", "版号服", 1786791600000, 1 ) );
            _serverItems.Add( GetServerItem( 2, "127.0.0.1:20335", "内测1服", 1786791600000, 1 ) );
            _serverItems.Add( GetServerItem( 3, "127.0.0.1:20345", "内测2服", 1786791600000, 1 ) );

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
        }
    }
}