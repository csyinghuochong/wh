using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ET
{
    [ObjectSystem]
    public class TaskComponentAwakeSystem : AwakeSystem<TaskComponentServer>
    {
        public override void Awake(TaskComponentServer self)
        {
            self.EnsureTaskLists();
            self.TaskEventBatchDepth = 0;
            if (self.TaskEventCoalesce == null)
            {
                self.TaskEventCoalesce = new Dictionary<(int, int), int>();
            }
            else
            {
                self.TaskEventCoalesce.Clear();
            }

            if (self.PendingTaskUpdateGroups == null)
            {
                self.PendingTaskUpdateGroups = new HashSet<int>();
            }
            else
            {
                self.PendingTaskUpdateGroups.Clear();
            }

            self.PendingTaskUpdate_1 = false;
        }
    }

    [ObjectSystem]
    public class TaskComponentDestroySystem : DestroySystem<TaskComponentServer>
    {
        public override void Destroy(TaskComponentServer self)
        {
        }
    }

    [ObjectSystem]
    public class TaskComponentDeserializeSystem : DeserializeSystem<TaskComponentServer>
    {
        public override void Deserialize(TaskComponentServer self)
        {
            self.EnsureTaskLists();
            self.TaskEventBatchDepth = 0;
            if (self.TaskEventCoalesce == null)
            {
                self.TaskEventCoalesce = new Dictionary<(int, int), int>();
            }
            else
            {
                self.TaskEventCoalesce.Clear();
            }

            if (self.PendingTaskUpdateGroups == null)
            {
                self.PendingTaskUpdateGroups = new HashSet<int>();
            }
            else
            {
                self.PendingTaskUpdateGroups.Clear();
            }

            self.PendingTaskUpdate_1 = false;
        }
    }

    public static class TaskComponentServerSystem
    {
        public static void EnsureTaskLists(this TaskComponentServer self)
        {
            if (self.RoleTaskList_2 == null)
            {
                self.RoleTaskList_2 = new List<TaskPro>();
            }

            if (self.RoleComoleteTaskList_2 == null)
            {
                self.RoleComoleteTaskList_2 = new List<int>();
            }

            if (self.RoleTaskList_1 == null)
            {
                self.RoleTaskList_1 = new List<TaskPro>();
            }

            if (self.RoleComoleteTaskList_1 == null)
            {
                self.RoleComoleteTaskList_1 = new List<int>();
            }
        }

        public static int GetMainTaskNumber(this TaskComponentServer self)
        {
            int mainTaskNumber = 0;
            for (int i = 0; i < self.RoleComoleteTaskList_2.Count; i++)
            {
                /*if (ldTask.TaskType == TaskTypeEnum.Main)
                {
                    mainTaskNumber++;
                }*/
            }
            return mainTaskNumber;
        }

        public static void Check(this TaskComponentServer self)
        {
            self.OnLineTime++;
            self.OnLineTime(1);
        }

        public static bool IsTaskComplete_2(this TaskComponentServer self, int taskid)
        {
            return self.RoleComoleteTaskList_2.IndexOf(taskid) >= 0;
        }

        public static bool IsTaskComplete(this TaskComponentServer self, int taskid)
        {
            return self.IsTaskComplete_2(taskid) || self.RoleComoleteTaskList_1.Contains(taskid);
        }

        //任务追踪
        public static int TaskTrack(this TaskComponentServer self, C2M_TaskTrackRequest request)
        {
            TaskPro task1 = self.GetTaskById_1(request.TaskId);
            if (task1 != null)
            {
                task1.TrackStatus = request.TrackStatus;
                return ErrorCode.ERR_Success;
            }

            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                if (self.RoleTaskList_2[i].taskID == request.TaskId)
                {
                    self.RoleTaskList_2[i].TrackStatus = request.TrackStatus;
                }
            }
            return ErrorCode.ERR_Success;
        }

        //对话之类的任务由客户端触发完成
        public static void OnTaskNotice(this TaskComponentServer self, C2M_TaskNoticeRequest request)
        {
            int taskid = request.TaskId;
            if (TaskHelper.IsTask1(taskid))
            {
                TaskPro taskPro = self.GetTaskById_1(taskid);
                if (taskPro == null || taskPro.taskStatus != (int)TaskStatuEnum.Accepted)
                {
                    return;
                }

                LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskid);
                self.CompleteTask1Progress(taskPro, ldTask);
                self.SendToUpdateTaskCore(new HashSet<int>(), true);
                return;
            }

            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                if (self.RoleTaskList_2[i].taskID == taskid)
                {
                    self.RoleTaskList_2[i].taskTargetNum_1 = 1;
                    self.RoleTaskList_2[i].taskStatus = (int)TaskStatuEnum.Completed;
                }
            }
        }

        /// <summary>
        /// 放弃任务
        /// </summary>
        /// <param name="self"></param>
        /// <param name="taskId"></param>
        public static void OnRecvGiveUpTask(this TaskComponentServer self, int taskId)
        {
            if (TaskHelper.IsTask1(taskId))
            {
                self.GiveUpTask_1(taskId);
                return;
            }

            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList_2[i].taskID != taskId)
                {
                    continue;
                }
                self.RoleTaskList_2.RemoveAt(i);
                break;
            }
        }

        /// <summary>
        /// 随机分支放弃后回到 UnActive，仍走服务端列表；链头/全开直接接的从列表移除，客户端可再推可接。
        /// </summary>
        public static void GiveUpTask_1(this TaskComponentServer self, int taskId)
        {
            TaskPro exist = self.GetTaskById_1(taskId);
            if (exist == null
                || exist.taskStatus == (int)TaskStatuEnum.UnActive
                || exist.taskStatus == (int)TaskStatuEnum.Commited)
            {
                return;
            }

            if (TaskHelper.IsTask1RandomBranch(taskId, self.RoleComoleteTaskList_1))
            {
                exist.taskStatus = (int)TaskStatuEnum.UnActive;
                exist.TrackStatus = 0;
                TaskHelper.ResetTask1Progress(exist);
            }
            else
            {
                self.RoleTaskList_1.Remove(exist);
            }

            self.SendToUpdateTaskCore(new HashSet<int>(), true);
        }

        /// <summary>
        /// 接取任务
        /// </summary>
        /// <param name="self"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static (TaskPro, int) OnAcceptedTask(this TaskComponentServer self, int taskId)
        {
            if (taskId == 0)
            {
                return (null, ErrorCode.ERR_TaskCanNotGet);
            }
            Unit unit = self.GetParent<Unit>();
            bool canget = true; // self.CheckTaskOn(taskId);
            if (!canget)
            {
                Log.Debug($"CanNotGetTask: {unit.DomainZone()} {unit.Id} {taskId}");
                return (null, ErrorCode.ERR_TaskCanNotGet);
            }
            if (self.IsHaveTask_2(taskId))
            {
                return(null, ErrorCode.ERR_TaskNoComplete);
            }
            TaskPro taskPro = self.CreateTask_2(taskId);
            return (taskPro, ErrorCode.ERR_Success);
        }

        public static TaskPro GetTaskById_1(this TaskComponentServer self, int taskid)
        {
            for (int i = self.RoleTaskList_1.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList_1[i].taskID == taskid)
                {
                    return self.RoleTaskList_1[i];
                }
            }
            return null;
        }

        public static bool IsHaveTask_1(this TaskComponentServer self, int taskId)
        {
            if (self.RoleComoleteTaskList_1.Contains(taskId))
            {
                return true;
            }

            TaskPro exist = self.GetTaskById_1(taskId);
            return exist != null && exist.taskStatus != TaskStatuEnum.UnActive;
        }

        public static TaskPro CreateTask_1(this TaskComponentServer self, int taskid)
        {
            TaskPro exist = self.GetTaskById_1(taskid);
            if (exist != null)
            {
                exist.taskStatus = (int)TaskStatuEnum.Accepted;
                exist.TrackStatus = 1;
                TaskHelper.ResetTask1Progress(exist);
                return exist;
            }

            TaskPro taskPro = new TaskPro();
            taskPro.taskID = taskid;
            taskPro.taskStatus = (int)TaskStatuEnum.Accepted;
            taskPro.TrackStatus = 1;
            TaskHelper.ResetTask1Progress(taskPro);
            self.RoleTaskList_1.Add(taskPro);
            return taskPro;
        }

        /// <summary>
        /// Task_1 手动接取。NpcId 必须对上表里的 NPC_Accept（表填 0 则不校验 NPC）。
        /// </summary>
        public static (TaskPro, int) OnAcceptedTask_1(this TaskComponentServer self, int taskId, int npcId)
        {
            if (taskId == 0 || LDTask_1Category.Instance == null || !LDTask_1Category.Instance.Contain(taskId))
            {
                return (null, ErrorCode.ERR_ModifyData);
            }

            LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskId);
            if (self.IsHaveTask_1(taskId))
            {
                return (null, ErrorCode.ERR_TaskNoComplete);
            }

            TaskPro exist = self.GetTaskById_1(taskId);

            if (ldTask.NPC_Accept != 0 && ldTask.NPC_Accept != npcId)
            {
                return (null, ErrorCode.ERR_TaskCanNotGet);
            }

            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (!TaskHelper.CheckTask1AcceptLevel(ldTask, roleInfo.Lv)
                || !TaskHelper.CheckTask1AcceptOccupation(ldTask.Accept, roleInfo.Occ))
            {
                return (null, ErrorCode.ERR_TaskCanNotGet);
            }

            if (!self.CanAcceptTask_1(ldTask, exist))
            {
                return (null, ErrorCode.ERR_TaskCanNotGet);
            }

            TaskPro taskPro = self.CreateTask_1(taskId);
            return (taskPro, ErrorCode.ERR_Success);
        }

        /// <summary>
        /// 列表里已是 UnActive：交付后写入的后继，可接。
        /// 否则：无前置=链头；前置已交付且 Next_Id_Type=0 全开也可接；随机分支必须先被写入 UnActive。
        /// </summary>
        public static bool CanAcceptTask_1(this TaskComponentServer self, LDTask_1 ldTask, TaskPro exist)
        {
            return TaskHelper.CanAcceptTask_1(ldTask, exist, self.RoleComoleteTaskList_1);
        }

        public static TaskPro TryAddUnActiveTask_1(this TaskComponentServer self, int taskId)
        {
            if (taskId <= 0 || LDTask_1Category.Instance == null || !LDTask_1Category.Instance.Contain(taskId))
            {
                return null;
            }

            if (self.RoleComoleteTaskList_1.Contains(taskId))
            {
                return null;
            }

            TaskPro exist = self.GetTaskById_1(taskId);
            if (exist != null)
            {
                return exist;
            }

            TaskPro taskPro = new TaskPro();
            taskPro.taskID = taskId;
            taskPro.taskStatus = (int)TaskStatuEnum.UnActive;
            TaskHelper.ResetTask1Progress(taskPro);
            self.RoleTaskList_1.Add(taskPro);
            return taskPro;
        }

        /// <summary>
        /// 交付后开后继：Next_Id_Type 0 全开，1 随机一条。已有/已交付的不重复写。
        /// </summary>
        public static void UnlockNextTasks_1(this TaskComponentServer self, LDTask_1 ldTask)
        {
            if (ldTask == null || ldTask.Next_Id == null || ldTask.Next_Id.Length == 0)
            {
                return;
            }

            List<int> nextIds = new List<int>();
            for (int i = 0; i < ldTask.Next_Id.Length; i++)
            {
                int nextId = ldTask.Next_Id[i];
                if (nextId <= 0)
                {
                    continue;
                }

                nextIds.Add(nextId);
            }

            if (nextIds.Count == 0)
            {
                return;
            }

            // 全开由客户端按表+完成列表推可接，服务端只写随机分支
            if (ldTask.Next_Id_Type != 1)
            {
                return;
            }

            int pick = nextIds[RandomHelper.RandomNumber(0, nextIds.Count)];
            self.TryAddUnActiveTask_1(pick);
        }

        public static TaskPro OnGetDailyTask(this TaskComponentServer self, int taskId)
        {
            TaskPro taskPro = self.CreateTask_2(taskId);
            return taskPro;
        }

        public static string GetMainTaskId(this TaskComponentServer self)
        {
            string maintask = string.Empty;
            List<TaskPro> taskPros = self.GetTaskList_2( TaskTypeEnum.Main );
            for (int i = 0; i < taskPros.Count; i++)
            {
                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPros[i].taskID );
                //maintask += $"{ldTask.Special_Word}_";
            }
            if (string.IsNullOrEmpty(maintask))
            {
                return "无";
            }
            else
            {
                return maintask;
            }
        }

        public static List<TaskPro> GetTaskList_2(this TaskComponentServer self, int taskType)
        { 
            List<TaskPro> taskPros = new List<TaskPro>();
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_2[i];
                if (taskPro.GetTaskGroupType() != (int)taskType)
                {
                    continue;
                }
                taskPros.Add(taskPro);
            }
            return taskPros;
        }

        public static bool IsItemTask(this TaskComponentServer self, int monsterid)
        {
            int taskId = 0;
            switch (monsterid)
            {
                case 41001008:
                    taskId = 30010013; //矿工的袋子
                    break;
                case 41001010:
                    taskId = 30010010;//解毒草
                    break;
                case 41002001:
                    taskId = 30020102;//清水
                    break;
                default:
                    break;
            }

            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                if (self.RoleTaskList_2[i].taskID == taskId)
                {
                    return self.RoleTaskList_2[i].taskStatus == (int)TaskStatuEnum.Accepted;
                }
            }
            return false;
        }

        public static bool IsHaveTask_2(this TaskComponentServer self, int taskId)
        {
            if (self.RoleComoleteTaskList_2.Contains(taskId))
            {
                return true;
            }
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                if (self.RoleTaskList_2[i].taskID == taskId)
                {
                    return true;
                }
            }
            return false;
        }

        public static TaskPro CreateTask_2(this TaskComponentServer self, int taskid)
        {
            TaskPro taskPro = new TaskPro();
            taskPro.taskID = taskid;
            taskPro.taskTargetNum_1 = 0;
            taskPro.taskStatus = self.RoleComoleteTaskList_2.Contains(taskid)
                    ? (int)TaskStatuEnum.Commited
                    : (int)TaskStatuEnum.Accepted;
            self.RoleTaskList_2.Add(taskPro);
            return taskPro;
        }

        public static void GetRandomFubenId(this TaskComponentServer self, TaskPro taskPro)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            int lv = roleInfoComponentServer.RoleInfo.Lv;

            List<int> openfubenids = new List<int>();
            HashSet<int> mysteryDungeonSet = new HashSet<int>(LDSectionCategory.Instance.MysteryDungeonList);
            Dictionary<int, LDScene> allfuben =  LDSceneCategory.Instance.GetAll();
            foreach (( int fubenid, LDScene config) in allfuben)
            {
                if (config.Id == 50007)
                {
                    continue;
                }
                if (mysteryDungeonSet.Contains(config.Id))
                {
                    continue;
                }
                if (config.GetEnterLv() <= lv && config.Id < CommonConfig.GMDungeonId)
                {
                    openfubenids.Add(fubenid);
                }
            }
            int dungeonid = openfubenids[RandomHelper.RandomNumber(0, openfubenids.Count)];
            string[] monsters =  SceneConfigHelper.GetLocalDungeonMonsters_2(dungeonid).Split('@');
            taskPro.FubenId = dungeonid;
            taskPro.WaveId = RandomHelper.RandomNumber(0, monsters.Length);
            Log.Warning($"生成藏宝图任务怪: {unit.Id} {dungeonid} {taskPro.WaveId}");
        }
        
        public static void OnGMGetTask(this TaskComponentServer self, int taskid)
        {
            HashSet<int> existingTaskIds = new HashSet<int>(self.RoleTaskList_2.Count);
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                existingTaskIds.Add(self.RoleTaskList_2[i].taskID);
            }

            if (existingTaskIds.Contains(taskid))
            {
                return;
            }

            self.CreateTask_2(taskid);
            self.SendToUpdateTask(LDTask_2Category.Instance.Get(taskid).Group);
        }

        public static List<TaskPro> GetTrackTaskList_2(this TaskComponentServer self)
        {
            List<TaskPro> taskPros = new List<TaskPro>();
            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList_2[i].TrackStatus == 1)
                {
                    taskPros.Add(self.RoleTaskList_2[i]);
                }
            }
            return taskPros;
        }

        public static TaskPro GetTaskById_2(this TaskComponentServer self, int taskid)
        {
            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList_2[i].taskID == taskid)
                {
                    return self.RoleTaskList_2[i];
                }
            }
            return null;
        }

        public static int GetPointProgressCurrent(this TaskComponentServer self, LDTask_2 ldTask)
        {
            if (ldTask == null)
            {
                return 0;
            }

            Unit unit = self.GetParent<Unit>();
            if (TaskHelper.IsActivityTask(ldTask))
            {
                RoleDailyDataComponentServer dailyData = unit?.GetComponent<RoleDailyDataComponentServer>();
                if (dailyData == null)
                {
                    return 0;
                }

                return ldTask.Condition_Type == TastConditionType.WeekActivityNumber_132
                        ? dailyData.GetWeeklyActivePoint()
                        : dailyData.GetDailyActivePoint();
            }

            if (TaskHelper.IsExtraCurrencyTask(ldTask))
            {
                RoleInfo roleInfo = unit?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
                int itemId = TaskHelper.GetConditionInspect(ldTask.Condition_Type);
                return (int)RoleCurrencyHelper.Get(roleInfo, itemId);
            }

            return 0;
        }
        
        public static int CheckGiveItemTask (this TaskComponentServer self, int TargetType, int[] Target, int[] TargetValue, long BagInfoID, TaskPro taskPro)
        {
            Unit unit = self.GetParent<Unit>();
            ////收集道具的任务
            //if (TargetType == (int)TastConditionType.ItemID_Number_2)
            //{
            //    BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            //    int needid = Target[0];
            //    int neednumber = TargetValue[0];
            //    int curnumber = (int)bagComponentServer.GetItemNumber(ItemBigType.Type_Item, needid);
            //    if (curnumber < neednumber)
            //    {
            //        return ErrorCode.ERR_ItemNotEnoughError;
            //    }

            //    bagComponentServer.OnCostItemData($"{needid};{neednumber}", ItemLocType.ItemLocBag, ItemGetWay.TaskCountry  );
            //    return ErrorCode.ERR_Success;
            //}
            return taskPro.taskStatus == (int)(TaskStatuEnum.Completed)? ErrorCode.ERR_Success : ErrorCode.Pre_Condition_Error;
            //return ErrorCode.ERR_Success; 
        }

        //领取奖励
        public static int OnCommitTask(this TaskComponentServer self, C2M_TaskCommitRequest request)
        {
            int taskid = request.TaskId;
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            TaskPro taskPro = self.GetTaskById_2(taskid);
            if (taskPro == null)
            {
                return ErrorCode.ERR_TaskCommited;
            }

            if (taskPro.taskStatus == (int)TaskStatuEnum.Commited || self.RoleComoleteTaskList_2.Contains(taskid))
            {
                return ErrorCode.ERR_TaskCommited;
            }

            LDTask_2 commitLdTask = LDTask_2Category.Instance.Get(taskid);
            if (TaskHelper.IsPointProgressTask(commitLdTask))
            {
                int curPoint = self.GetPointProgressCurrent(commitLdTask);
                if (curPoint < commitLdTask.Param1)
                {
                    return ErrorCode.ERR_Error;
                }
            }
            else if (taskPro.taskStatus != (int)TaskStatuEnum.Completed)
            {
                return ErrorCode.Pre_Condition_Error;
            }

            List<RewardItem> rewardItems = TaskHelper.GetTaskRewardItems(roleInfoComponent.RoleInfo.Occ, taskid);

            int needcell = ItemNewHelper.GetNeedCell(rewardItems);
            int bagLeftCell = bagComponentServer.GetBagLeftCell();
            if (bagLeftCell < needcell)
            {
                return ErrorCode.ERR_BagIsFull;
            }
            
            if (bagLeftCell < rewardItems.Count)
            {
                return ErrorCode.ERR_BagIsFull;
            }

            int checkError = 0;/// self.CheckGiveItemTask(0, null, 0, request.BagInfoID,taskPro);
            if (checkError != ErrorCode.ERR_Success)
            {
                return checkError;
            }
            
            //for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            //{
            //    if (self.RoleTaskList_2[i].taskID == taskid)
            //    {
            //        self.RoleTaskList_2.RemoveAt(i);
            //    }
            //}
            taskPro.taskStatus = (int)TaskStatuEnum.Commited;
            if (!self.RoleComoleteTaskList_2.Contains(taskid))
            {
                self.RoleComoleteTaskList_2.Add(taskid);
            }

            TaskRewardHelper.GrantTaskCommitRewards(unit, rewardItems);
            self.SendToUpdateTask(commitLdTask.Group);
            return ErrorCode.ERR_Success;
        }

        public static int OnCommitTask_1(this TaskComponentServer self, C2M_TaskCommitRequest request)
        {
            int taskid = request.TaskId;
            if (LDTask_1Category.Instance == null || !LDTask_1Category.Instance.Contain(taskid))
            {
                return ErrorCode.ERR_ModifyData;
            }

            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            TaskPro taskPro = self.GetTaskById_1(taskid);
            if (taskPro == null)
            {
                return ErrorCode.ERR_TaskCommited;
            }

            if (taskPro.taskStatus == (int)TaskStatuEnum.Commited || self.RoleComoleteTaskList_1.Contains(taskid))
            {
                return ErrorCode.ERR_TaskCommited;
            }

            LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskid);
            int finishNpc = TaskHelper.GetTask1FinishNpc(ldTask);
            if (finishNpc != 0 && request.NpcId != finishNpc)
            {
                return ErrorCode.ERR_TaskCanNotGet;
            }

            bool noRequirement = TaskHelper.HasNoTask1Requirement(ldTask);
            if (noRequirement)
            {
                if (taskPro.taskStatus != (int)TaskStatuEnum.Accepted && taskPro.taskStatus != (int)TaskStatuEnum.Completed)
                {
                    return ErrorCode.Pre_Condition_Error;
                }
            }
            else if (taskPro.taskStatus != (int)TaskStatuEnum.Completed)
            {
                return ErrorCode.Pre_Condition_Error;
            }

            List<RewardItem> rewardItems = TaskHelper.GetTaskRewardItems_1(roleInfoComponent.RoleInfo.Occ, taskid);
            int needcell = ItemNewHelper.GetNeedCell(rewardItems);
            int bagLeftCell = bagComponentServer.GetBagLeftCell();
            if (bagLeftCell < needcell || bagLeftCell < rewardItems.Count)
            {
                return ErrorCode.ERR_BagIsFull;
            }

            taskPro.taskStatus = (int)TaskStatuEnum.Commited;
            if (!self.RoleComoleteTaskList_1.Contains(taskid))
            {
                self.RoleComoleteTaskList_1.Add(taskid);
            }

            TaskRewardHelper.GrantTaskCommitRewards(unit, rewardItems);
            if (ldTask.Exp_Role_Param1 > 0)
            {
                roleInfoComponent.Role_AddExp(ldTask.Exp_Role_Param1, true);
            }

            self.UnlockNextTasks_1(ldTask);
            self.SendToUpdateTaskCore(new HashSet<int>(), true);
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 转职
        /// </summary>
        /// <param name="self"></param>
        public static void OnChangeOccTwo(this TaskComponentServer self)
        {
        }

        /// <summary>
        /// 制造
        /// </summary>
        public static void OnMakeItem(this TaskComponentServer self)
        {
           
        }

        /// <summary>
        /// 宠物洗练
        /// </summary>
        /// <param name="self"></param>
        public static void OnPetXiLian(this TaskComponentServer self, PetInfo rolePetInfo)
        {
           
        }

        public static void OnPetHeCheng(this TaskComponentServer self, PetInfo rolePetInfo)
        {
            using (self.TaskEventBatch())
            {
                int combat = PetHelper.PetPingFen(rolePetInfo);
            }
        }

        /// <summary>
        /// 获得宠物
        /// </summary>
        /// <param name="self"></param>
        public static void OnGetPet(this TaskComponentServer self, PetInfo rolePetInfo)
        {
            using (self.TaskEventBatch())
            {
               
            }
        }

        /// <summary>
        /// 道具洗练（次数）
        /// </summary>
        public static void OnEquipXiLian(this TaskComponentServer self, int times)
        {
        }

        /// <summary>
        /// 宠物蛋孵化
        /// </summary>
        public static void OnPetEggOpen(this TaskComponentServer self, int eggItemId)
        {
            using (self.TaskEventBatch())
            {
              
            }
        }

        /// <summary>
        /// 打造装备
        /// </summary>
        public static void OnMakeEquip(this TaskComponentServer self, int quality)
        {
            using (self.TaskEventBatch())
            {
            
            }
        }

        /// <summary>
        /// 充值天数任务（入包请走 Bag.OnAddItemData，由其内部自行 batch）
        /// </summary>
        public static void OnRechargeDay(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.RechageDayNumber_113, 1, 30);
        }

        /// <summary>
        /// 宠物天梯胜利（仅任务进度；发奖走 DungeonSettlementHelper）
        /// </summary>
        public static void OnPetTianTiWin(this TaskComponentServer self)
        {
        }

        /// <summary>
        /// 宠物天梯排名
        /// </summary>
        public static void OnPetTianTiRank(this TaskComponentServer self, int rankId)
        {
        }

        /// <summary>
        /// 宠物副本通关（仅任务进度；发奖/宠物记录走 DungeonSettlementHelper）
        /// </summary>
        public static void OnPetFubenWin(this TaskComponentServer self, int petFubenId)
        {
        }

        /// <summary>
        /// 组队副本结算任务侧
        /// </summary>
        public static void OnTeamDungeonSettle(this TaskComponentServer self, int sceneId, int hurtRate)
        {
            using (self.TaskEventBatch())
            {
                self.OnPassTeamFuben();
            }
        }

        /// <summary>
        /// 在线时长，暂时一分钟触发一次
        /// </summary>
        /// <param name="self"></param>
        public static void OnLineTime(this TaskComponentServer self, int time)
        {
        }

        /// <summary>
        /// 道具回收
        /// </summary>
        /// <param name="self"></param>
        public static void OnItemHuiShow(this TaskComponentServer self, int itemNumber)
        {
        }

        /// <summary>
        /// 消耗金币
        /// </summary>
        /// <param name="self"></param>
        public static void OnCostCoin(this TaskComponentServer self, int costCoin)
        {
            if (costCoin >= 0)
                return;
        }

        public static void OnJiaYuanLevel(this TaskComponentServer self, int jiaYuanLv)
        {
        }

        public static void OnCombatToValue(this TaskComponentServer self, int combat, int delta = 0)
        {
            self.NotifyCondition(TastConditionType.CombatRechage_121, combat, delta);
            self.NotifyCondition(TastConditionType.CombatIncrease_122, combat, delta);
        }

        /// <summary>
        /// 通关副本
        /// </summary>
        /// <param name="self"></param>
        /// <param name="difficulty"></param>
        /// <param name="chapterid"></param>
        /// <param name="star"></param>
        public static void OnPassFuben(this TaskComponentServer self, int difficulty, int chapterid, int star)
        {
            using (self.TaskEventBatch())
            {
                
            }
        }

        public static void OnWinCampBattle(this TaskComponentServer self)
        {
        }

        public static void OnPassTeamFuben(this TaskComponentServer self)
        {
        }

        public static void OnOpenBox(this TaskComponentServer self)
        {
        }

        public static void OnFuMo(this TaskComponentServer self, int quality)
        {
        }

        public static void OnQiangHua(this TaskComponentServer self, int qiangHuaLevel)
        {
        }

        public static void OnJoinUnion(this TaskComponentServer self)
        {
            
        }

        public static void OnFriendPassFuben(this TaskComponentServer self)
        {
            
        }

        public static void OnPetMineBattle(this TaskComponentServer self, int result)
        {
            using (self.TaskEventBatch())
            {
                
            }
        }

        public static void OnDuiHuanGold(this TaskComponentServer self, int diamond)
        {
           
        }

        public static void OnCombatRank(this TaskComponentServer self, int rankId)
        {
            
        }

        public static void OnBattleUseItem(this TaskComponentServer self)
        {
           
        }

        public static void OnPetUseSkillBook(this TaskComponentServer self)
        {
          
        }

        public static void OnPetXiLianCrystal(this TaskComponentServer self)
        {
           
        }

        public static async ETTask UpdateUnionRaceRank(this TaskComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            await ETTask.CompletedTask;
        }

        //击杀怪物可触发多种类型的任务
        public static void OnKillUnit(this TaskComponentServer self, Unit bekill, int sceneType)
        {
            if (bekill == null || bekill.IsDisposed)
                return;

            Unit unit = self.GetParent<Unit>();
            if (bekill.Type == UnitType.Player && sceneType == MapTypeEnum.Battle)
            {
           
            }
            if (bekill.Type == UnitType.Player && sceneType == MapTypeEnum.UnionRace)
            {
            
                self.UpdateUnionRaceRank().Coroutine();
            }

            using (self.TaskEventBatch())
            {
                if (bekill.Type == UnitType.Monster)
                {
                    int unitconfigId = bekill.ConfigId;
                    LDMonster ldMonster = LDMonsterCategory.Instance.Get(unitconfigId);
                    bool isBoss = ldMonster.Type == (int)MonsterTypeEnum.Boss;
                    Scene domainScene = unit.DomainScene();
                    MapComponent mapComponent = domainScene.GetComponent<MapComponent>();
                    int fubenDifficulty = FubenDifficulty.None;
                    if (mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
                    {
                        fubenDifficulty = domainScene.GetComponent<LocalDungeonComponent>().FubenDifficulty;
                    }

                    self.TriggerTaskEvent(TastConditionType.KillMonsterByNumber_210, 1, 0);

                }
            }
        }

        //等级更新
        public static void OnUpdateLevel(this TaskComponentServer self, int rolelv)
        {
            self.NotifyCondition(TastConditionType.PlayerLv_1, rolelv);
        }


        /// <summary>
        /// 每次登录：清无效任务，缺的 Group 补建。进度只在数值变化时刷，登录不回填。
        /// 101/102 只在 OnDailyReset。
        /// </summary>
        public static void OnLogin(this TaskComponentServer self)
        {
            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (!LDTask_2Category.Instance.Contain(self.RoleTaskList_2[i].taskID))
                {
                    self.RoleTaskList_2.RemoveAt(i);
                }
            }

            if (LDTask_1Category.Instance != null)
            {
                for (int i = self.RoleTaskList_1.Count - 1; i >= 0; i--)
                {
                    if (!LDTask_1Category.Instance.Contain(self.RoleTaskList_1[i].taskID))
                    {
                        self.RoleTaskList_1.RemoveAt(i);
                    }
                }

                for (int i = self.RoleComoleteTaskList_1.Count - 1; i >= 0; i--)
                {
                    if (!LDTask_1Category.Instance.Contain(self.RoleComoleteTaskList_1[i]))
                    {
                        self.RoleComoleteTaskList_1.RemoveAt(i);
                    }
                }
            }

            self.InitAllTaskGroups();
        }


        //OnDailyReset 每日数据重置在这处理。  每次登录走onlogin
        /// <summary>
        /// 101 累计登录天数、102 今日登录。只在日清时调用：跨天登录(1)、在线 5 点(2)、首次初始化(0)。
        /// 同天重登、C2M_TaskOnLogin 不会进 OnDailyReset，因此不会重复加。
        /// </summary>
        public static void TriggerDailyLoginTaskEvents(this TaskComponentServer self)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.LoginDayNymber_101, 1, 0);
                self.TriggerTaskEvent(TastConditionType.LoginToday_102, 1, 0);
            }
        }

        //收集道具
        public static void OnGetItemForWarehouse(this TaskComponentServer self, int itemId)
        {
        }

        //累计获得道具数量
        public static void OnGetItemNumber(this TaskComponentServer self, int getWay, int itembigType,  int itemId,int itemNumber)
        {
            if (itemId == 1 || (getWay != ItemGetWay.ReceieMail && getWay != ItemGetWay.PaiMaiSell))
            {
            }
        }

        //收集道具
        public static void OnGetItem_2(this TaskComponentServer self, int itemType, int itemId)
        {
        }

        public static void GMCompletCurrentTask(this TaskComponentServer self)
        {
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_2[i];
                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPro.taskID);

                if (taskPro.taskStatus == (int)TaskStatuEnum.Completed)
                {
                    continue;
                }

                taskPro.taskTargetNum_1 = ldTask.Param1;
                taskPro.taskStatus = (int)TaskStatuEnum.Completed;
            }

            self.SendToUpdateTask();
        }

        public static void OnPetMineLogin(this TaskComponentServer self, List<PetMingPlayerInfo> petMingPlayers, List<IntLongPair> extends)
        {
            using (self.TaskEventBatch())
            {
                for (int i = 0; i < petMingPlayers.Count; i++)
                {
                }
            }
        }

        // Begin → 合并 Trigger；End → Flush。怕漏 End 时请用 using (self.TaskEventBatch()) { ... }
        /// <summary>
        /// 任务事件批处理作用域，离开 using 自动 End/Flush。仅限 Component 内部 OnXxx 使用。
        /// </summary>
        public static TaskEventBatchScope TaskEventBatch(this TaskComponentServer self)
        {
            return new TaskEventBatchScope(self);
        }

        /// <summary>
        /// 开始任务事件批处理。优先用 TaskEventBatch() + using。
        /// </summary>
        public static void BeginTaskEventBatch(this TaskComponentServer self)
        {
            self.TaskEventBatchDepth++;
        }

        /// <summary>
        /// 结束任务事件批处理（支持嵌套）。优先用 TaskEventBatch() + using。
        /// </summary>
        public static void EndTaskEventBatch(this TaskComponentServer self)
        {
            if (self.TaskEventBatchDepth <= 0)
            {
                return;
            }
            self.TaskEventBatchDepth--;
            if (self.TaskEventBatchDepth == 0)
            {
                self.FlushTaskEventBatch();
            }
        }

        /// <summary>
        /// 数值变化时推进任务。覆盖写 current，累计加 delta，由 Task_Condition.Type 决定。
        /// </summary>
        public static void NotifyCondition(this TaskComponentServer self, int conditionType, int current, int delta = 0)
        {
            bool overrideMode = GetConditionMode(conditionType) == TaskConditionMode.Override;
            if (!overrideMode && delta <= 0)
            {
                return;
            }

            self.TriggerTaskEvent(conditionType, overrideMode ? current : delta, 0);
        }

        public static void TriggerTaskEvent(this TaskComponentServer self, int conditionType, int param1, int param2)
        {
            if (self.TaskEventBatchDepth > 0)
            {
                var key = (conditionType, param2);
                if (GetConditionMode(conditionType) == TaskConditionMode.Override)
                {
                    self.TaskEventCoalesce[key] = param1;
                }
                else
                {
                    self.TaskEventCoalesce.TryGetValue(key, out int sum);
                    self.TaskEventCoalesce[key] = sum + param1;
                }
                return;
            }

            self.ApplyTaskEvent(conditionType, param1, param2);
        }

        private static int GetConditionMode(int conditionType)
        {
            if (LDTask_ConditionCategory.Instance == null || !LDTask_ConditionCategory.Instance.Contain(conditionType))
            {
                return TaskConditionMode.Accumulate;
            }

            return LDTask_ConditionCategory.Instance.Get(conditionType).Type;
        }

        private static void ApplyConditionProgress(TaskPro taskPro, int conditionType, int targetValue, int value)
        {
            if (GetConditionMode(conditionType) == TaskConditionMode.Override)
            {
                taskPro.taskTargetNum_1 = value;
            }
            else
            {
                taskPro.taskTargetNum_1 += value;
            }

            bool completed = taskPro.taskTargetNum_1 >= targetValue;
            taskPro.taskStatus = completed ? (int)TaskStatuEnum.Completed : (int)TaskStatuEnum.Accepted;
        }

        private static void ApplyConditionProgress(TaskPro taskPro, LDTask_2 ldTask, int value)
        {
            ApplyConditionProgress(taskPro, ldTask.Condition_Type, ldTask.Param1, value);
        }

        private static void ApplyTask1SlotProgress(TaskPro taskPro, LDTask_1 ldTask, int slot, int conditionType, int value)
        {
            int cur = TaskHelper.GetTaskProTargetNum(taskPro, slot);
            if (GetConditionMode(conditionType) == TaskConditionMode.Override)
            {
                cur = value;
            }
            else
            {
                cur += value;
            }

            TaskHelper.SetTaskProTargetNum(taskPro, slot, cur);
            taskPro.taskStatus = TaskHelper.IsTask1RequirementsComplete(taskPro, ldTask)
                    ? (int)TaskStatuEnum.Completed
                    : (int)TaskStatuEnum.Accepted;
        }

        private static void CompleteTask1Progress(this TaskComponentServer self, TaskPro taskPro, LDTask_1 ldTask)
        {
            for (int slot = 1; slot <= 4; slot++)
            {
                LDTask_1_Requirement requirement = TaskHelper.GetTask1Requirement(ldTask, slot);
                if (requirement == null)
                {
                    continue;
                }

                int target = requirement.Param1 > 0 ? requirement.Param1 : 1;
                TaskHelper.SetTaskProTargetNum(taskPro, slot, target);
            }

            taskPro.taskStatus = (int)TaskStatuEnum.Completed;
        }

        /// <summary>
        /// 只给已接取的 Task_1 记进度。Requirement_1~4 分别写入 taskTargetNum_1~4。
        /// </summary>
        private static bool ApplyTask1Progress(this TaskComponentServer self, int conditionType, int param1, int param2)
        {
            bool changed = false;
            for (int i = 0; i < self.RoleTaskList_1.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_1[i];
                if (taskPro.taskStatus != (int)TaskStatuEnum.Accepted)
                {
                    continue;
                }

                if (!LDTask_1Category.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }

                LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskPro.taskID);
                for (int slot = 1; slot <= 4; slot++)
                {
                    LDTask_1_Requirement requirement = TaskHelper.GetTask1Requirement(ldTask, slot);
                    if (requirement == null || requirement.Condition_Type != conditionType || requirement.Param2 != param2)
                    {
                        continue;
                    }

                    ApplyTask1SlotProgress(taskPro, ldTask, slot, requirement.Condition_Type, param1);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool ApplyTask1ProgressFromCoalesce(this TaskComponentServer self)
        {
            bool changed = false;
            for (int i = 0; i < self.RoleTaskList_1.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_1[i];
                if (taskPro.taskStatus != (int)TaskStatuEnum.Accepted)
                {
                    continue;
                }

                if (!LDTask_1Category.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }

                LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskPro.taskID);
                for (int slot = 1; slot <= 4; slot++)
                {
                    LDTask_1_Requirement requirement = TaskHelper.GetTask1Requirement(ldTask, slot);
                    if (requirement == null)
                    {
                        continue;
                    }

                    if (!self.TaskEventCoalesce.TryGetValue((requirement.Condition_Type, requirement.Param2), out int delta))
                    {
                        continue;
                    }

                    ApplyTask1SlotProgress(taskPro, ldTask, slot, requirement.Condition_Type, delta);
                    changed = true;
                }
            }

            return changed;
        }

        private static void FlushTaskEventBatch(this TaskComponentServer self)
        {
            if (self.TaskEventCoalesce.Count == 0)
            {
                return;
            }

            self.PendingTaskUpdateGroups.Clear();
            self.PendingTaskUpdate_1 = false;
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_2[i];
                if (taskPro.taskStatus >= (int)TaskStatuEnum.Completed)
                {
                    continue;
                }

                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPro.taskID);
                if (!self.TaskEventCoalesce.TryGetValue((ldTask.Condition_Type, ldTask.Param2), out int delta))
                {
                    continue;
                }

                if (!TaskHelper.ShouldRecordTaskProgress(self.RoleTaskList_2, self.RoleComoleteTaskList_2, taskPro))
                {
                    continue;
                }

                ApplyConditionProgress(taskPro, ldTask, delta);
                self.PendingTaskUpdateGroups.Add(ldTask.Group);
            }

            self.PendingTaskUpdate_1 = self.ApplyTask1ProgressFromCoalesce();
            self.TaskEventCoalesce.Clear();
            self.FlushPendingTaskUpdates();
        }

        private static void ApplyTaskEvent(this TaskComponentServer self, int conditionType, int param1, int param2)
        {
            self.PendingTaskUpdateGroups.Clear();
            self.PendingTaskUpdate_1 = false;

            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_2[i];
                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPro.taskID);
                if (ldTask.Condition_Type != conditionType)
                {
                    continue;
                }
                if (ldTask.Param2 != param2)
                {
                    continue;
                }
                if (taskPro.taskStatus >= (int)TaskStatuEnum.Completed)
                {
                    continue;
                }
                if (!TaskHelper.ShouldRecordTaskProgress(self.RoleTaskList_2, self.RoleComoleteTaskList_2, taskPro))
                {
                    continue;
                }
                ApplyConditionProgress(taskPro, ldTask, param1);
                self.PendingTaskUpdateGroups.Add(ldTask.Group);
            }

            self.PendingTaskUpdate_1 = self.ApplyTask1Progress(conditionType, param1, param2);
            self.FlushPendingTaskUpdates();
        }

        private static void FlushPendingTaskUpdates(this TaskComponentServer self)
        {
            bool updateTask1 = self.PendingTaskUpdate_1;
            self.PendingTaskUpdate_1 = false;
            if (self.PendingTaskUpdateGroups.Count == 0 && !updateTask1)
            {
                return;
            }

            if (self.PendingTaskUpdateGroups.Count > 0)
            {
                self.SendToUpdateTaskCore(self.PendingTaskUpdateGroups, updateTask1);
                return;
            }

            self.SendToUpdateTaskCore(new HashSet<int>(), updateTask1);
        }
        

        public static void CheckDailyTask(this TaskComponentServer self)
        {
            self.InitTasksByResetType(TaskGroupResetType.Daily);
        }


        public static void CheckWeeklyTask(this TaskComponentServer self)
        {
            self.InitTasksByResetType(TaskGroupResetType.Weekly);
        }

        /// <summary>
        /// 是否已有指定 Group（日常/周常等）任务；兼容旧数据 TaskType 未写入的情况。
        /// </summary>
        public static bool HasTaskByGroup(this TaskComponentServer self, int taskGroup)
        {
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList_2[i];
                if (taskPro.GetTaskGroupType() == taskGroup)
                {
                    return true;
                }

                if (!LDTask_2Category.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }

                if (LDTask_2Category.Instance.Get(taskPro.taskID).Group == taskGroup)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按 Group 全开：同一子组每条条件可能不同，必须各自记账。有新建时推送客户端。
        /// </summary>
        public static void InitTasksByType(this TaskComponentServer self, int taskType)
        {
            List<int> taskIds = TaskHelper.GenerateTaskListByType(taskType);
            if (taskIds.Count == 0)
            {
                return;
            }

            HashSet<int> existing = new HashSet<int>();
            for (int i = 0; i < self.RoleTaskList_2.Count; i++)
            {
                existing.Add(self.RoleTaskList_2[i].taskID);
            }

            bool created = false;
            for (int i = 0; i < taskIds.Count; i++)
            {
                int taskId = taskIds[i];
                if (existing.Contains(taskId))
                {
                    continue;
                }
                self.CreateTask_2(taskId);
                created = true;
            }

            if (created)
            {
                self.SendToUpdateTask(taskType);
            }
        }

        public static void InitTasksByResetType(this TaskComponentServer self, int resetType)
        {
            List<int> groupIds = TaskHelper.GetGroupIdsByResetType(resetType);
            for (int i = 0; i < groupIds.Count; i++)
            {
                self.InitTasksByType(groupIds[i]);
            }
        }

        /// <summary>
        /// 所有 Task_Group 默认全开（日常/周常/成就 Type0）。已有的不重复建。
        /// 某个 Group 在 Task 表没有行会打 Error，方便对表。
        /// </summary>
        public static void InitAllTaskGroups(this TaskComponentServer self)
        {
            if (LDTask_GroupCategory.Instance == null)
            {
                return;
            }

            foreach (LDTask_Group group in LDTask_GroupCategory.Instance.GetAll().Values)
            {
                if (TaskHelper.GenerateTaskListByType(group.Id).Count == 0)
                {
                    Log.Error($"InitAllTaskGroups: Task_Group={group.Id} 在 Task 表没有任务");
                    continue;
                }

                self.InitTasksByType(group.Id);
            }
        }

        public static void ClearTasksByResetType(this TaskComponentServer self, int resetType)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList_2);
            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (!LDTask_2Category.Instance.Contain(self.RoleTaskList_2[i].taskID))
                {
                    self.RoleTaskList_2.RemoveAt(i);
                    continue;
                }

                LDTask_2 ldTask = LDTask_2Category.Instance.Get(self.RoleTaskList_2[i].taskID);
                if (TaskHelper.GetGroupResetType(ldTask.Group) != resetType)
                {
                    continue;
                }

                if (completedTaskIds.Contains(ldTask.Id))
                {
                    self.RoleComoleteTaskList_2.Remove(ldTask.Id);
                    completedTaskIds.Remove(ldTask.Id);
                }

                self.RoleTaskList_2.RemoveAt(i);
            }

            for (int i = self.RoleComoleteTaskList_2.Count - 1; i >= 0; i--)
            {
                int taskId = self.RoleComoleteTaskList_2[i];
                if (!LDTask_2Category.Instance.Contain(taskId))
                {
                    continue;
                }

                if (TaskHelper.GetGroupResetType(LDTask_2Category.Instance.Get(taskId).Group) == resetType)
                {
                    self.RoleComoleteTaskList_2.RemoveAt(i);
                }
            }
        }


        public static void ClearTypeTask(this TaskComponentServer self, int taskType)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList_2);
            for (int i = self.RoleTaskList_2.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList_2[i].GetTaskGroupType() != taskType)
                {
                    continue;
                }
                int taskId = self.RoleTaskList_2[i].taskID;
                if (completedTaskIds.Contains(taskId))
                {
                    self.RoleComoleteTaskList_2.Remove(taskId);
                    completedTaskIds.Remove(taskId);
                }
                self.RoleTaskList_2.RemoveAt(i);
            }
        }

        public static void UpdateDayTask(this TaskComponentServer self, bool notice)
        {
            self.ClearTasksByResetType(TaskGroupResetType.Daily);
            self.InitTasksByResetType(TaskGroupResetType.Daily);
        }


        public static void UpdateWeeklyTask(this TaskComponentServer self, bool notice)
        {
            self.ClearTasksByResetType(TaskGroupResetType.Weekly);

            RoleDailyDataComponentServer dailyData = self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>();
            if (dailyData != null)
            {
                dailyData.ClearDayLists(RoleDailyClearType.Week);
                if (notice)
                {
                    dailyData.NotifyUpdate(RoleDailyDataComponentServer.ReasonFull);
                }
            }

            self.InitTasksByResetType(TaskGroupResetType.Weekly);

            if (notice)
            {
                List<int> weeklyGroups = TaskHelper.GetGroupIdsByResetType(TaskGroupResetType.Weekly);
                self.PendingTaskUpdateGroups.Clear();
                for (int i = 0; i < weeklyGroups.Count; i++)
                {
                    self.PendingTaskUpdateGroups.Add(weeklyGroups[i]);
                }

                if (self.PendingTaskUpdateGroups.Count > 0)
                {
                    self.SendToUpdateTask(self.PendingTaskUpdateGroups);
                }
            }
        }



        public static void CheckWeeklyUpdate(this TaskComponentServer self)
        {
            System.DateTime dateTime = TimeHelper.DateTimeNow();
            if( dateTime.DayOfWeek == System.DayOfWeek.Monday)
            {
                //Log.Console($"ResetWeeklyTask: passday:{self.Id} {dateTime.DayOfWeek == System.DayOfWeek.Monday}");
                self.UpdateWeeklyTask(true);
            }
        }
       
        public static void LoginCheckWeeklyUpdate(this TaskComponentServer self, long lastTime, long curTime)
        {
            //判断条件。 超过一周或者过了周末
            float passday = ((curTime - lastTime) * 1f / TimeHelper.OneDay);
            if (passday >= 7)
            {
                //Log.Warning($"ResetWeeklyTask: passday:{self.Id} {passday}");
                self.UpdateWeeklyTask(false);
            }
            else
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastTime);
                DateTime curdateTime = TimeInfo.Instance.ToDateTime(curTime);
                if ((curdateTime.DayOfWeek < lastdateTime.DayOfWeek && curdateTime.DayOfWeek!= 0)
                 || (curdateTime.DayOfWeek > lastdateTime.DayOfWeek && lastdateTime.DayOfWeek == 0))
                {
                    Log.Warning($"ResetWeeklyTask:{self.Id} {curdateTime.DayOfWeek} {lastdateTime.DayOfWeek}");
                    self.UpdateWeeklyTask(false);
                }
                //int curday = curdateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)(curdateTime.DayOfWeek);
                //int lastday = lastdateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)(lastdateTime.DayOfWeek);
                //if(curday < lastday)
                //{
                //    Log.Console($"ResetWeeklyTask:{self.Id} {curdateTime.DayOfWeek} {lastdateTime.DayOfWeek}");
                //    self.ResetWeeklyTask();
                //}
            }
        }

        public static List<TaskPro> GetClientShowTaskList_2(this TaskComponentServer self)
        {
            return TaskHelper.GetClientShowTaskList_2(self.RoleTaskList_2, self.RoleComoleteTaskList_2);
        }

        /// <summary>
        /// GroupIds 为空：整表覆盖（日清、GM 全完成）。
        /// 有 GroupIds：只带这些组当前展示条，客户端按组替换。
        /// </summary>
        public static void SendToUpdateTask(this TaskComponentServer self)
        {
            self.SendToUpdateTaskCore(null);
        }

        public static void SendToUpdateTask(this TaskComponentServer self, int groupId)
        {
            if (groupId <= 0)
            {
                self.SendToUpdateTaskCore(null);
                return;
            }

            HashSet<int> groups = new HashSet<int>();
            groups.Add(groupId);
            self.SendToUpdateTaskCore(groups);
        }

        public static void SendToUpdateTask(this TaskComponentServer self, HashSet<int> groupIds)
        {
            self.SendToUpdateTaskCore(groupIds);
        }

        public static void OnNpcTalkComplete(this TaskComponentServer self, int npcId)
        {
            for (int k = 0; k < self.RoleTaskList_2.Count; k++)
            {
                TaskPro taskPro = self.RoleTaskList_2[k];
                if (!LDTask_2Category.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }

                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPro.taskID);
                if (taskPro.taskStatus < (int)TaskStatuEnum.Completed
                    && ldTask.Condition_Type == TastConditionType.TalkToNpc_200
                    && ldTask.Param2 == npcId)
                {
                    taskPro.taskStatus = TaskStatuEnum.Completed;
                }
            }

            bool task1Changed = false;
            for (int k = 0; k < self.RoleTaskList_1.Count; k++)
            {
                TaskPro taskPro = self.RoleTaskList_1[k];
                if (taskPro.taskStatus != (int)TaskStatuEnum.Accepted || !TaskHelper.IsTask1(taskPro.taskID))
                {
                    continue;
                }

                LDTask_1 ldTask = LDTask_1Category.Instance.Get(taskPro.taskID);
                for (int slot = 1; slot <= 4; slot++)
                {
                    LDTask_1_Requirement requirement = TaskHelper.GetTask1Requirement(ldTask, slot);
                    if (requirement == null
                        || requirement.Condition_Type != TastConditionType.TalkToNpc_200
                        || requirement.Param2 != npcId)
                    {
                        continue;
                    }

                    int target = requirement.Param1 > 0 ? requirement.Param1 : 1;
                    TaskHelper.SetTaskProTargetNum(taskPro, slot, target);
                    taskPro.taskStatus = TaskHelper.IsTask1RequirementsComplete(taskPro, ldTask)
                            ? (int)TaskStatuEnum.Completed
                            : (int)TaskStatuEnum.Accepted;
                    task1Changed = true;
                }
            }

            if (task1Changed)
            {
                self.SendToUpdateTaskCore(new HashSet<int>(), true);
            }
        }

        private static void SendToUpdateTaskCore(this TaskComponentServer self, HashSet<int> groupIds, bool updateTask1 = false)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_TaskUpdate m2C_TaskUpdate = self.M2C_TaskUpdate;
            m2C_TaskUpdate.GroupIds.Clear();
            m2C_TaskUpdate.UpdateTask_1 = updateTask1 ? 1 : 0;
            if (updateTask1)
            {
                List<TaskPro> task1List = new List<TaskPro>(self.RoleTaskList_1.Count);
                for (int i = 0; i < self.RoleTaskList_1.Count; i++)
                {
                    task1List.Add(self.RoleTaskList_1[i]);
                }

                m2C_TaskUpdate.RoleTaskList_1 = task1List;
            }
            else
            {
                m2C_TaskUpdate.RoleTaskList_1 = new List<TaskPro>();
            }

            if (groupIds != null && groupIds.Count > 0)
            {
                foreach (int groupId in groupIds)
                {
                    m2C_TaskUpdate.GroupIds.Add(groupId);
                }

                m2C_TaskUpdate.RoleTaskList_2 = TaskHelper.GetClientShowTaskList_2(self.RoleTaskList_2, self.RoleComoleteTaskList_2, groupIds);
            }
            else if (groupIds == null)
            {
                m2C_TaskUpdate.RoleTaskList_2 = self.GetClientShowTaskList_2();
            }
            else
            {
                m2C_TaskUpdate.RoleTaskList_2 = new List<TaskPro>();
            }

            MessageHelper.SendToClient(unit, m2C_TaskUpdate);
        }

        /// <summary>
        /// 日清。resetType：0 首次初始化 / 1 跨天登录 / 2 在线 5 点。
        /// 1、2（以及 0）刷新 101/102 登录次数；同天重登不会进这里。
        /// </summary>
        public static void OnDailyReset(this TaskComponentServer self, int resetType)
        {
            bool notice = resetType == 2;
            self.OnLineTime = 0;
            self.UpdateDayTask(notice);
            self.InitAllTaskGroups();
            self.TriggerDailyLoginTaskEvents();

            if (notice)
            {
                self.SendToUpdateTask();
            }
        }
    }
}
