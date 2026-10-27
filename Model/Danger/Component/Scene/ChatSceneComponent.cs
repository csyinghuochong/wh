using System.Collections.Generic;

namespace ET
{

    public class ChatInfoUnit : Entity, IAwake, IDestroy
    {
        public long LastSendChat;

        public long GateSessionActorId; //player.InstanceId

        public long UnionId;

        public string Name;

        public int Level;

        public int RoleHeadId;

        public int RoleHeadFrame;

        public int VipLevel;

        public int TitleId;
    }

    public class BeReportedInfo
    {
        public long JinYanTime;
        public List<long> ReportedList = new List<long>();
    }

    public class ChatSceneComponent : Entity, IAwake, IDestroy
    {
        public long Timer;

        //全服玩家GateSessionActorId
        public Dictionary<long, ChatInfoUnit> ChatInfoUnitsDict = new Dictionary<long, ChatInfoUnit>();

        /// <summary>家族 Id -> 在线成员。家族频道只扫这一份。</summary>
        public Dictionary<long, List<ChatInfoUnit>> UnionUnits = new Dictionary<long, List<ChatInfoUnit>>();

        //世界列表记录
        public List<ChatInfo> WordChatInfos = new List<ChatInfo> ();    

        public Dictionary<long, BeReportedInfo> BeReportedNumber = new Dictionary<long, BeReportedInfo> ();

        /// <summary>本服重要跑马灯。Id = 区服号。</summary>
        public DBMarqueeInfo DBMarqueeInfo;

        public ETTask MarqueeInitTask;

        public List<MarqueePlan> MarqueePlanList = new List<MarqueePlan>();
    }

    public class MarqueePlan
    {
        public int Id;
        public long FireTime;
        public long ExpireTime;
    }
}
