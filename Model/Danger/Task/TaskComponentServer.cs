using System.Collections.Generic;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif


namespace ET
{

     public  class TaskComponentServer: Entity, IAwake, ITransfer, IDestroy, IUnitCache, IDeserialize
    {
        public int OnLineTime = 0;

        /// <summary>Task_2 日活/成就进行中。Bson 仍写 RoleTaskList，兼容旧存档。</summary>
#if SERVER
        [BsonElement("RoleTaskList")]
#endif
        public List<TaskPro> RoleTaskList_2 = new List<TaskPro>();

        /// <summary>Task_2 已领取。Bson 仍写 RoleComoleteTaskList，兼容旧存档。</summary>
#if SERVER
        [BsonElement("RoleComoleteTaskList")]
#endif
        public List<int> RoleComoleteTaskList_2 = new List<int>();

        /// <summary>Task_1 NPC 对话任务进行中（可接/已接/待交）。</summary>
        public List<TaskPro> RoleTaskList_1 = new List<TaskPro>();

        /// <summary>Task_1 已交付。</summary>
        public List<int> RoleComoleteTaskList_1 = new List<int>();

        [BsonIgnore]
        public M2C_TaskUpdate M2C_TaskUpdate = new M2C_TaskUpdate();

        /// <summary>任务事件批处理深度；&gt;0 时 TriggerTaskEvent 只合并不推送</summary>
        [BsonIgnore]
        public int TaskEventBatchDepth;

        /// <summary>批处理合并表：key=(conditionType, param2)，value=累加的 param1</summary>
        [BsonIgnore]
        public Dictionary<(int, int), int> TaskEventCoalesce = new Dictionary<(int, int), int>();

        /// <summary>本次进度变更涉及到的 Group，Flush 时按组推送</summary>
        [BsonIgnore]
        public HashSet<int> PendingTaskUpdateGroups = new HashSet<int>();

        /// <summary>本次是否有 Task_1 进度变更，推送时整表覆盖 RoleTaskList_1</summary>
        [BsonIgnore]
        public bool PendingTaskUpdate_1;
    }
}
