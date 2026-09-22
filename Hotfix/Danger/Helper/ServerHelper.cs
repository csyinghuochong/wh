using System;
using System.Collections.Generic;

namespace ET
{
    public static class ServerHelper
    {

        private static int Platform = -1;   //平台
        private static int VersionMode = 0;

        private static List<ServerItem> ServerItems = new List<ServerItem>();

        //public static string LocalIp = "192.168.1.16"; 
        public static string LocalIp = "127.0.0.1";


        //版号专区
        public static bool IsBanHaoZone(int zone)
        {
            return Game.Options.StartConfig.Contains("BanHao");
        }


        public static bool IsGoogleServer(int zone)
        {
            return Platform == 7; 
        }

        //Alpha = 0,              //仅内部人员使用。一般不向外部发布
        //Beta = 1,               //公开测试版
        //BanHao = 2,
        public static string GetServerIpList(bool innerNet, int zone)
        {
            ServerItem serverItem = GetGetServerItem(innerNet, zone);
            return serverItem.ServerIp;
        }

        public static ServerItem GetGetServerItem(bool innerNet, int zone)
        {
            ServerItem serverItem = null;
            List<ServerItem> serverItems = GetServerList();
            for (int i = 0; i < serverItems.Count; i++)
            {
                if (serverItems[i].ServerId == zone)
                {
                    serverItem = serverItems[i];
                }
            }
            return serverItem;
        }


        public static long GetOpenServerTime(bool innerNet, int zone)
        {
            ServerItem serverItem = GetGetServerItem(innerNet, zone);
            if (serverItem == null)
            {
                Log.Error($"serverItem == null {zone}");
                return 0;
            }
            return serverItem.ServerOpenTime;
        }

        public static int GetOpenServerDay(bool innerNet, int zone)
        {
            long serverNow = TimeHelper.ServerNow();
            long openSerTime = GetOpenServerTime(innerNet, zone);
            if (openSerTime == 0 || serverNow < openSerTime)
            {
                return 0;
            }

            int openserverDay = DateDiff_Time(serverNow, openSerTime);
            return openserverDay;
        }

        public static int DateDiff_Time(long time1, long time2)
        {
            DateTime d1 = TimeInfo.Instance.ToDateTime(time1);
            DateTime d2 = TimeInfo.Instance.ToDateTime(time2);
            DateTime d3 = Convert.ToDateTime(string.Format("{0}-{1}-{2}", d1.Year, d1.Month, d1.Day));

            DateTime d4 = Convert.ToDateTime(string.Format("{0}-{1}-{2}", d2.Year, d2.Month, d2.Day));
            int days = (d3 - d4).Days + 1;
            return days;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerNet"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static int GetOldServerId(int zone)
        {
            List<ServerItem> serverItems_1 = GetServerList();

            string serverip = string.Empty;
            for (int i = 0; i < serverItems_1.Count; i++)
            {
                if (serverItems_1[i].ServerId == zone)
                {
                    serverip = serverItems_1[i].ServerIp;
                    break;
                }
            }
            for (int i = 0; i < serverItems_1.Count; i++)
            {
                if (serverItems_1[i].ServerIp == serverip)
                {
                    zone = serverItems_1[i].ServerId;
                }
            }
            return zone;
        }

        public static bool IsOldServer(int zone)
        {
            List<ServerItem> serverItems_1 = GetServerList();
            string serverip = string.Empty;
            for (int i = 0; i < serverItems_1.Count; i++)
            {
                if (serverItems_1[i].ServerId == zone)
                {
                    serverip = serverItems_1[i].ServerIp;
                    break;
                }
            }

            int servernumber = 0;
            for (int i = 0; i < serverItems_1.Count; i++)
            {
                if (serverItems_1[i].ServerIp == serverip)
                {
                    servernumber++;
                }
            }
            return servernumber > 1;
        }
        
        public static void UpdateServerList()
        {

        }


        public static bool IsBanHaoServer(int zone)
        {
            return VersionMode == 2;
        }

        public static List<ServerItem> GetServerList()
        {
            return StartZoneConfigCategory.Instance.ServerItems;
        }


        /// <summary>
        /// 获取合区后的新区id.  todo
        /// </summary>
        /// <param name="zoneid"></param>
        /// <returns></returns>
        public static int GetNewServerId(int zoneid)
        {
            return zoneid;
        }

        public static void SetServerList(List<ServerItem> serverItems)
        {
            ServerItems = serverItems;
        }
    }
}