using System.Collections.Generic;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif


namespace ET
{
#if SERVER
     public  class TaskComponentServer: Entity, IAwake, ITransfer, IDestroy, IUnitCache, IDeserialize
#else
    public class TaskComponent : Entity, IAwake
#endif
    {
        public int OnLineTime = 0;
        public List<int> ReceiveHuoYueIds = new List<int>();
        public List<TaskPro> RoleTaskList = new List<TaskPro>();
        public List<int> RoleComoleteTaskList = new List<int>();

#if SERVER
        [BsonIgnore]
        public M2C_TaskUpdate M2C_TaskUpdate = new M2C_TaskUpdate();
        
        /*public const int WeeklyTaskNumber = 3172;
        public const int WeeklyTaskId = 3173;
           public const int DailyTaskNumber = 3063;                                 //赏金任务完成数量   
        public const int RingTaskNumber = 3161;
        public const int RingTaskId = 3162;
         public const int SeasonTask = 3152;
           public const int UnionTaskId = 3110;      
                   public const int DailyTaskID = 3084;                                         //赏金任务ID                                  //家族任务
        public const int UnionTaskNumber = 3111;
                public const int SystemTask = 3182;                    
                                     //系统任务*/
#endif
    }
}
