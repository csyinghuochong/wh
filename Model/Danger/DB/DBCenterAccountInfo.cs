using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    public enum AccountType
    {
        General = 0,

        NoPaiMai = 1, //禁止拍卖

        BlackList = 2, //黑名单

        Delete = 3, //删号
    }

    
    public enum RoleInfoState
    {
        Normal = 0,
        Freeze = 1,    //冻结
    }

    [BsonIgnoreExtraElements]
    public class DBCenterAccountInfo : Entity, IAwake
    {
        

        //禁封角色列表
        public List<long> BanUserList = new List<long>();
        
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<long, long> BanUserTime = new Dictionary<long, long>();

        //用户名
        public string Account { get; set; }

        public string EnPhone{ get; set; }

        //密码
        public string Password { get; set; }

        public PlayerInfo PlayerInfo { get; set; }

        public int TotalRecharge { get; set; }   //总充值

        public int AccountType; //账号类型

        public long CreateTime; //创建时间
        
        public long BanTime;    //封号时间

        public string TaprepRequest{ get; set; }  
        
        public string BanAccount;  //关联封禁帐号

        public string DeviceID;

        public bool addRecharge = false;
        
        public List<CreateRoleInfo> RoleList = new List<CreateRoleInfo>();

        public List<int> IsUpperList = new List<int>();

        public string IP;    //上次登陆ip

        public int GetTotalRecharge()
        {
            int total = 0;
            for (int i = 0; i < PlayerInfo.RechargeInfos.Count; i++)
            {
                total += PlayerInfo.RechargeInfos[i].Amount;
            }

            return total;
        }
        
        public int TodayCreateRole()
        {
            int total = 0;
           
            return total;
        }
        
        public CreateRoleInfo GetRoleInfo( int zone, long unitid)
        {
            for (int i = 0; i < RoleList.Count; i++)
            {
                if (  zone ==RoleList[i].ServerId && RoleList[i].UserID == unitid )
                {
                    return RoleList[i];
                }
            }

            return null;
        }
    }
}
