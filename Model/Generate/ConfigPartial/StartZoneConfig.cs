using System.Collections.Generic;

namespace ET
{
    public partial class StartZoneConfigCategory
    {

        private List<ServerItem> _serverItems = new List<ServerItem>();
        public List<ServerItem> ServerItems => _serverItems;
        
        
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
        
        public override void AfterEndInit()
        {
            _serverItems.Clear();
            //_serverItems.Add( GetServerItem( 1, "43.139.108.125:20325", "版号服", 1720782000000, 1 ) );
            //_serverItems.Add( GetServerItem( 2, "43.139.108.125:20335", "内测服", 1720954800000, 0 ) );
            
            _serverItems.Add( GetServerItem( 1, "127.0.0.1:20325", "版号服", 1779102000000, 1 ) );
            _serverItems.Add( GetServerItem( 2, "127.0.0.1:20335", "内测服", 1779102000000, 1 ) );
        }
    }
}