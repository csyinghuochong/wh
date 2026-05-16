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


        public const string LogicServer = "weijinggameserver.weijinggame.com";//"weijinggameserver.weijinggame.com"

        //ec2-52-35-43-8.us-west-2.compute.amazonaws.com    52.35.43.8      172.31.44.172  亚马逊 美国 -俄勒冈州
        //ec2-23-20-18-54.compute-1.amazonaws.com           23.20.18.54     172.31.21.19   亚马逊 美国 -弗吉尼亚州
        //othercountry2.weijinggame.com                     8.221.119.18    172.31.183.49  阿里云 美国 -弗吉尼亚州
        //                                                  47.251.252.96   172.20.250.253 阿里云  美国 -加利福尼亚州  距离俄勒冈州 近一点  玩家ip也在俄勒冈州
        //                                                  47.86.59.101    172.20.227.82 阿里云  香港
        public const string LogicServerGoogle = "47.86.59.101"; //"8.221.110.80"; ///"47.77.221.152"; //"47.86.59.101";香港    //"othercountry.weijinggame.com";  
        public const string LogicServerBanHao = "43.139.108.125";

        public static string GetLogicServer(bool innerNet)
        {
            if (Platform == -1)
            {
                Console.WriteLine("Platform == -1");
            }

            if (innerNet)
            {
                return LocalIp;
            }

            if (Platform == 7)
            {
                return LogicServerGoogle;
            }

            return VersionMode == 1 ? LogicServer : LogicServerBanHao;

            //switch (versionMode)
            //{
            //    case VersionMode.BanHao:
            //        return innerNet ? ComHelp.LocalIp : LogicServerBanHao;
            //    default:
            //        return innerNet ? ComHelp.LocalIp : LogicServer;

            //}
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

        public static bool IsGoogleServer(int zone)
        {
            return Platform == 7;
        }

        public static bool IsBanHaoServer(int zone)
        {
            return VersionMode == 2;
        }

        public static List<ServerItem> GetServerList()
        {
            return StartZoneConfigCategory.Instance.ServerItems;
        }


        public static void SetServerList(List<ServerItem> serverItems)
        {
            ServerItems = serverItems;
        }
    }
}