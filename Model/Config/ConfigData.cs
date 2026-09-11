using System.Collections.Generic;

namespace ET
{
    public static class ConfigData
    {
        public static int PackageLimit = 500;

        public static bool AccountOldLogic = true;

        public static bool CleanSkill = true;

        public static bool LogRechargeNumber = false;

        public static bool ShowLieOpen = false;


        public static Dictionary<int , ServerInfo> ServerInfoList = new Dictionary<int , ServerInfo>();


        //公众平台上开发者设置的token, appID, EncodingAESKey
        //public const string sToken = "yinhuochongweijing666";
        //public const string sAppID = "wx8fe94511cb4701c1";
        //public const string sAppSecret = "f54fcbc601209c2ba3500cd5f56e448c";
        //public const string sEncodingAESKey = "yinhuochongweijing666yinhuochongweijing666a";

        public const string sToken = "yinhuochongweijing666";
        public const string sAppID = "wx7854ea0905d50363";
        public const string sAppSecret = "ef45491463da00149880f07f47fb74da";
        public const string sEncodingAESKey = "yinhuochongweijing666yinhuochongweijing666a";

        public static bool OldNavMesh = true;


        public static char DataCollationSpit = '&';

        public static string sNewStr = string.Empty;
    }
}
