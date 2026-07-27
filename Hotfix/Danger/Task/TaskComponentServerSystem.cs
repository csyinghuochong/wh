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
            self.TaskEventBatchDepth = 0;
            self.TaskEventCoalesce?.Clear();
            if (self.RoleTaskList.Count == 0)
            {
                
            }
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
            self.TaskEventBatchDepth = 0;
            if (self.TaskEventCoalesce == null)
            {
                self.TaskEventCoalesce = new Dictionary<(int, int), int>();
            }
            else
            {
                self.TaskEventCoalesce.Clear();
            }
        }
    }

    public static class TaskComponentServerSystem
    {

        public static bool ShowPaiMai(this TaskComponentServer self, int lv, int simulator)
        {
            if (simulator == 0)
            {
                return true;
            }
            //int mainTaskNumber = 0;
            //for (int i = 0; i < self.RoleComoleteTaskList.Count; i++)
            //{
            //    TaskConfig taskConfig = TaskConfigCategory.Instance.Get(self.RoleComoleteTaskList[i]);
            //    if (taskConfig.TaskType == TaskTypeEnum.Main)
            //    {
            //        mainTaskNumber ++;  
            //    }
            //}
            return lv >= 5 && self.RoleComoleteTaskList.Count > lv;
        }

        public static int GetMainTaskNumber(this TaskComponentServer self)
        {
            int mainTaskNumber = 0;
            for (int i = 0; i < self.RoleComoleteTaskList.Count; i++)
            {
                /*if (ldTask.TaskType == TaskTypeEnum.Main)
                {
                    mainTaskNumber++;
                }*/
            }
            return mainTaskNumber;
        }

        public static int GetHuoYueDu(this TaskComponentServer self)
        {
            const int pointsPerTask = 10;
            int huoYueDu = 0;
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                if (taskPro.taskStatus < (int)TaskStatuEnum.Completed)
                {
                    continue;
                }
                if (taskPro.TaskType == TaskTypeEnum.Daily)
                {
                    huoYueDu += pointsPerTask;
                    continue;
                }
                if (!LDTaskCategory.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }
                LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);
                if (ldTask.Condition_Type == TastConditionType.EveryDayTask_1019
                    || ldTask.Condition_Type == TastConditionType.DailyTask_1014)
                {
                    huoYueDu += pointsPerTask;
                }
            }
            return huoYueDu;
        }

        public static void Check(this TaskComponentServer self)
        {
            self.OnLineTime++;
            self.OnLineTime(1);
        }

        public static bool IsTaskComplete(this TaskComponentServer self, int taskid)
        {
            return self.RoleComoleteTaskList.IndexOf(taskid) >= 0;
        }

        //任务追踪
        public static int TaskTrack(this TaskComponentServer self, C2M_TaskTrackRequest request)
        {
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                if (self.RoleTaskList[i].taskID == request.TaskId)
                {
                    self.RoleTaskList[i].TrackStatus = request.TrackStatus;
                }
            }
            return ErrorCode.ERR_Success;
        }

        //对话之类的任务由客户端触发完成
        public static void OnTaskNotice(this TaskComponentServer self, C2M_TaskNoticeRequest request)
        {
            int taskid = request.TaskId;
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                if (self.RoleTaskList[i].taskID == taskid)
                {
                    self.RoleTaskList[i].taskTargetNum_1 = 1;
                    self.RoleTaskList[i].taskStatus = (int)TaskStatuEnum.Completed;
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
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].taskID != taskId)
                {
                    continue;
                }
                self.RoleTaskList.RemoveAt(i);
                break;
            }
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
            if (self.IsHaveTask(taskId))
            {
                return(null, ErrorCode.ERR_TaskNoComplete);
            }
            TaskPro taskPro = self.CreateTask(taskId);
            return (taskPro, ErrorCode.ERR_Success);
        }

        public static TaskPro OnGetDailyTask(this TaskComponentServer self, int taskId)
        {
            TaskPro taskPro = self.CreateTask(taskId);
            return taskPro;
        }

        public static string GetMainTaskId(this TaskComponentServer self)
        {
            string maintask = string.Empty;
            List<TaskPro> taskPros = self.GetTaskList( TaskTypeEnum.Main );
            for (int i = 0; i < taskPros.Count; i++)
            {
                LDTask ldTask = LDTaskCategory.Instance.Get(taskPros[i].taskID );
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

        public static List<TaskPro> GetTaskList(this TaskComponentServer self, int taskType)
        { 
            List<TaskPro> taskPros = new List<TaskPro>();
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                if (taskPro.TaskType!= (int)taskType)
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

            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                if (self.RoleTaskList[i].taskID == taskId)
                {
                    return self.RoleTaskList[i].taskStatus == (int)TaskStatuEnum.Accepted;
                }
            }
            return false;
        }

        public static bool IsHaveTask(this TaskComponentServer self, int taskId)
        {
            if (self.RoleComoleteTaskList.Contains(taskId))
            {
                return true;
            }
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                if (self.RoleTaskList[i].taskID == taskId)
                {
                    return true;
                }
            }
            return false;
        }

        public static TaskPro CreateTask(this TaskComponentServer self, int taskid)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            LDTask ldTask = LDTaskCategory.Instance.Get(taskid);
            TaskPro taskPro = new TaskPro();
            taskPro.taskID = taskid;
            taskPro.TaskType = ldTask.Group;

            switch (ldTask.Condition_Type)
            {
                case (int)(int)TastConditionType.PlayerLv_1:
                    taskPro.taskTargetNum_1 = roleInfo.Lv;
                    break;
                /*case (int)TastConditionType.KillMonsterID_1:
                    taskPro.taskTargetNum_1 = unit.GetComponent<RoleInfoComponentServer>().GetReviveTime(ldTask.Param1) > 0?1 : 0;
                    break;*/
                case (int)TastConditionType.ItemID_Number_2:
                    taskPro.taskTargetNum_1 = (int)bagComponentServer.GetItemNumber(ItemBigType.Type_Item, ldTask.Param1);
                    break;
                case(int)TastConditionType.LookingFor_3:
                    taskPro.taskTargetNum_1 = 1;
                    break;
              
                case (int)TastConditionType.ChangeOcc_8:
                    taskPro.taskTargetNum_1 = roleInfo.OccTwo > 0 ? 1 : 0;
                    break;
                case (int)TastConditionType.JoinUnion_9:
                    taskPro.taskTargetNum_1 = numericComponent.GetAsLong(NumericType.UnionId_0) > 0? 1 : 0;
                    break;
                case (int)TastConditionType.PetNumber1_11:
                    taskPro.taskTargetNum_1 = petComponentServer.GetAllPets().Count;
                    break;
                case (int)TastConditionType.QiangHuaLevel_17:
                    taskPro.taskTargetNum_1 = bagComponentServer.GetMaxQiangHuaLevel();
                    break;
                case (int)TastConditionType.PetNSkill_18:
                    taskPro.taskTargetNum_1 = petComponentServer.GetMaxSkillNumber();
                    break;
                case (int)TastConditionType.PetFubenId_19:
                    taskPro.taskTargetNum_1 = petComponentServer.GetPassMaxFubenId();
                    break;
                case (int)TastConditionType.JiaYuanLevel_22:
                    taskPro.taskTargetNum_1 = roleInfo.JiaYuanLv;
                    break;
                case (int)TastConditionType.CombatToValue_133:
                    taskPro.taskTargetNum_1 = roleInfo.Combat;
                    break;
                case (int)TastConditionType.TrialTowerCeng_134:
                    int curtrialid = numericComponent.GetAsInt(NumericType.TrialDungeonId);
                    if (curtrialid > ldTask.Param1)
                    {
                        taskPro.taskTargetNum_1 = 1;
                    }
                    break;
                default:
                    taskPro.taskTargetNum_1 = 0;
                    break;
            }

            bool completed = false;
            
           // self.IsCompleted(taskPro, ldTask.TargetType, ldTask.Target, ldTask.TargetValue);
            taskPro.taskStatus = completed ? (int)TaskStatuEnum.Completed : (int)TaskStatuEnum.Accepted;
            
            self.RoleTaskList.Add(taskPro);
            /*if (ldTask.TaskType == TaskTypeEnum.Treasure)
            {
                self.GetRandomFubenId(taskPro);
            }
            if (ldTask.TaskType != TaskTypeEnum.Season
                && ldTask.TaskType != TaskTypeEnum.Welfare
                && ldTask.TaskType != TaskTypeEnum.System
                && self.GetTrackTaskList().Count < 3)
            {
                taskPro.TrackStatus = 1;
            }*/
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
            HashSet<int> existingTaskIds = new HashSet<int>(self.RoleTaskList.Count);
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                existingTaskIds.Add(self.RoleTaskList[i].taskID);
            }

            if (existingTaskIds.Contains(taskid))
            {
                return;
            }

            self.CreateTask(taskid);
          
            self.SendToUpdateTask();
        }

        public static List<TaskPro> GetTrackTaskList(this TaskComponentServer self)
        {
            List<TaskPro> taskPros = new List<TaskPro>();
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].TrackStatus == 1)
                {
                    taskPros.Add(self.RoleTaskList[i]);
                }
            }
            return taskPros;
        }

        public static TaskPro GetTaskById(this TaskComponentServer self, int taskid)
        {
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].taskID == taskid)
                {
                    return self.RoleTaskList[i];
                }
            }
            return null;
        }
        
        public static int CheckGiveItemTask (this TaskComponentServer self, int TargetType, int[] Target, int[] TargetValue, long BagInfoID, TaskPro taskPro)
        {
            Unit unit = self.GetParent<Unit>();
            //收集道具的任务
            if (TargetType == (int)TastConditionType.ItemID_Number_2)
            {
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                int needid = Target[0];
                int neednumber = TargetValue[0];
                int curnumber = (int)bagComponentServer.GetItemNumber(ItemBigType.Type_Item, needid);
                if (curnumber < neednumber)
                {
                    self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, needid, 0);
                 
                    return ErrorCode.ERR_ItemNotEnoughError;
                }

                bagComponentServer.OnCostItemData($"{needid};{neednumber}", ItemLocType.ItemLocBag, ItemGetWay.TaskCountry  );
                return ErrorCode.ERR_Success;
            }
            //给予任务
            if (TargetType == (int)TastConditionType.GiveItem_10)
            {
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, BagInfoID);
                if (bagInfo == null)
                {
                    return ErrorCode.ERR_ItemNotExist;
                }
                if (!TaskHelper.IsTaskGiveItem(TargetType, Target, TargetValue, bagInfo))
                {
                    return ErrorCode.ERR_ItemNotEnoughError;
                }
                bagComponentServer.OnCostItemData(BagInfoID, 1);
                return ErrorCode.ERR_Success;
            }
            //给予宠物
            if (TargetType == (int)TastConditionType.GivePet_25)
            {
                PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
                RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(BagInfoID);
                if (rolePetInfo == null)
                {
                    return ErrorCode.ERR_ItemNotExist;
                }
                if (!TaskHelper.IsTaskGivePet(TargetType, Target, TargetValue, rolePetInfo))
                {
                    return ErrorCode.ERR_ItemNotEnoughError;
                }

                petComponentServer.OnRolePetFenjie(BagInfoID);
                return ErrorCode.ERR_Success;
            }
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
            TaskPro taskPro = self.GetTaskById(taskid);
            if (taskPro == null)
            {
                return ErrorCode.ERR_TaskCommited;
            }
            List<RewardItem> rewardItems = TaskHelper.GetTaskRewardItems(roleInfoComponent.RoleInfo.Occ, taskid);

            if (rewardItems.Count == 0)
            {
                rewardItems.Add( new RewardItem()
                {
                    ItemType = ItemBigType.Type_Item, 
                    ItemID = 1,
                    ItemNum = 1
                });
            }
            
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
            
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].taskID == taskid)
                {
                    self.RoleTaskList.RemoveAt(i);
                }
            }
            self.RoleComoleteTaskList.Add(taskid);
            TaskRewardHelper.GrantTaskCommitRewards(unit, rewardItems);
            
            self.OnTeskAddTask(taskid);
            
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 转职
        /// </summary>
        /// <param name="self"></param>
        public static void OnChangeOccTwo(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.ChangeOcc_8, 0, 1);
          
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
        public static void OnPetXiLian(this TaskComponentServer self, RolePetInfo rolePetInfo)
        {
            self.TriggerTaskEvent(TastConditionType.PetNSkill_18, 0, rolePetInfo.PetSkill.Count);
           
        }

        public static void OnPetHeCheng(this TaskComponentServer self, RolePetInfo rolePetInfo)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.PetNumber1_11, 0, 1);
                self.TriggerTaskEvent(TastConditionType.PetHeCheng_23, 0, 1);
                self.TriggerTaskEvent(TastConditionType.PetNumber2_24, 0, 1);
                self.TriggerTaskEvent(TastConditionType.PetNSkill_18, 0, rolePetInfo.PetSkill.Count);
                int combat = PetHelper.PetPingJia(rolePetInfo);
                self.TriggerTaskEvent(TastConditionType.PetHeChengCombat_32, combat, 1);
            }
        }

        /// <summary>
        /// 获得宠物
        /// </summary>
        /// <param name="self"></param>
        public static void OnGetPet(this TaskComponentServer self, RolePetInfo rolePetInfo)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent( TastConditionType.PetNumber1_11, 0, 1 );
                self.TriggerTaskEvent(TastConditionType.PetNumber2_24, 0, 1);
                self.TriggerTaskEvent( TastConditionType.PetNSkill_18,  0, rolePetInfo.PetSkill.Count);
                self.TriggerTaskEvent(TastConditionType.PetNumber_31, 0, 1);
            }
        }

        /// <summary>
        /// 道具洗练（次数）
        /// </summary>
        public static void OnEquipXiLian(this TaskComponentServer self, int times)
        {
            self.TriggerTaskEvent(TastConditionType.EquipXiLian_13, 0, times);
        }

        /// <summary>
        /// 宠物蛋孵化
        /// </summary>
        public static void OnPetEggOpen(this TaskComponentServer self, int eggItemId)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.PetFuHuaNumber_34, 0, 1);
                self.TriggerTaskEvent(TastConditionType.PetFuHuaId_35, eggItemId, 1);
            }
        }

        /// <summary>
        /// 打造装备
        /// </summary>
        public static void OnMakeEquip(this TaskComponentServer self, int quality)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.MakeNumber_12, 0, 1);
                self.TriggerTaskEvent(TastConditionType.MakeQulityNumber_29, quality, 1);
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
            self.TriggerTaskEvent(TastConditionType.PetTianDiWin_37, 0, 1);
        }

        /// <summary>
        /// 宠物天梯排名
        /// </summary>
        public static void OnPetTianTiRank(this TaskComponentServer self, int rankId)
        {
            self.TriggerTaskEvent(TastConditionType.PetTianTiRank_82, rankId, 1);
        }

        /// <summary>
        /// 宠物副本通关（仅任务进度；发奖/宠物记录走 DungeonSettlementHelper）
        /// </summary>
        public static void OnPetFubenWin(this TaskComponentServer self, int petFubenId)
        {
            self.TriggerTaskEvent(TastConditionType.PetFubenId_19, 0, petFubenId - 10000);
        }

        /// <summary>
        /// 组队副本结算任务侧
        /// </summary>
        public static void OnTeamDungeonSettle(this TaskComponentServer self, int sceneId, int hurtRate)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.TeamDungeonHurt_136, sceneId, hurtRate);
                self.OnPassTeamFuben();
            }
        }

        /// <summary>
        /// 在线时长，暂时一分钟触发一次
        /// </summary>
        /// <param name="self"></param>
        public static void OnLineTime(this TaskComponentServer self, int time)
        {
            self.TriggerTaskEvent(TastConditionType.OnLineTime_1010, 0, time);
        }

        /// <summary>
        /// 道具回收
        /// </summary>
        /// <param name="self"></param>
        public static void OnItemHuiShow(this TaskComponentServer self, int itemNumber)
        {
            self.TriggerTaskEvent(TastConditionType.EquipHuiShou_16, 0, itemNumber);
         
        }

        /// <summary>
        /// 消耗金币
        /// </summary>
        /// <param name="self"></param>
        public static void OnCostCoin(this TaskComponentServer self, int costCoin)
        {
            if (costCoin >= 0)
                return;
            self.TriggerTaskEvent(TastConditionType.TotalCostGold_20, 0, costCoin * -1);
        }

        public static void OnJiaYuanLevel(this TaskComponentServer self, int jiaYuanLv)
        {
            self.TriggerTaskEvent(TastConditionType.JiaYuanLevel_22, 0, jiaYuanLv);
        }

        public static void OnCombatToValue(this TaskComponentServer self, int combat)
        {
            self.TriggerTaskEvent(TastConditionType.CombatToValue_133, 0, combat);
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
                self.TriggerTaskEvent(TastConditionType.PassFubenID_7, chapterid, 1);

                if ((int)difficulty >= (int)FubenDifficulty.TiaoZhan)  //挑战
                {
                    self.TriggerTaskEvent(TastConditionType.PassTianZhanFubenID_111, chapterid, 1);
                }
                if ((int)difficulty >= (int)FubenDifficulty.DiYu)  //地狱
                {
                    self.TriggerTaskEvent(TastConditionType.PassDiYuFubenID_112, chapterid, 1);
                }
            }
        }

        public static void OnWinCampBattle(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.BattleWin_1101, 0, 1);
        }

        public static void OnPassTeamFuben(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.PassTeamFuben_1004, 0, 1);
        }

        public static void OnOpenBox(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.OpenBox_137, 0, 1);
        }

        public static void OnFuMo(this TaskComponentServer self, int quality)
        {
            self.TriggerTaskEvent(TastConditionType.FuMoQulity_41, quality, 1);
        }

        public static void OnQiangHua(this TaskComponentServer self, int qiangHuaLevel)
        {
            self.TriggerTaskEvent(TastConditionType.QiangHuaLevel_17, 0, qiangHuaLevel);
        }

        public static void OnJoinUnion(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.JoinUnion_9, 0, 1);
        }

        public static void OnFriendPassFuben(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.FriendPassFuben_138, 0, 1);
        }

        public static void OnPetMineBattle(this TaskComponentServer self, int result)
        {
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.MineBattleNumber_402, 0, 1);
                if (result == CombatResultEnum.Win)
                {
                    self.TriggerTaskEvent(TastConditionType.MineWinNumber_403, 0, 1);
                }
            }
        }

        public static void OnDuiHuanGold(this TaskComponentServer self, int diamond)
        {
            self.TriggerTaskEvent(TastConditionType.DuiHuanGold_15, 0, diamond / 100);
        }

        public static void OnCombatRank(this TaskComponentServer self, int rankId)
        {
            self.TriggerTaskEvent(TastConditionType.CombatRank_83, rankId, 1);
        }

        public static void OnBattleUseItem(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.BattleUseItem_30, 0, 1);
        }

        public static void OnPetUseSkillBook(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.PetUseSkillBook_36, 0, 1);
        }

        public static void OnPetXiLianCrystal(this TaskComponentServer self)
        {
            self.TriggerTaskEvent(TastConditionType.PetXiLian10010086_33, 0, 1);
        }

        public static async ETTask UpdateUnionRaceRank(this TaskComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            RankShouLieInfo rankingInfo = new RankShouLieInfo()
            {
                UnitID = unit.Id,
                KillNumber = 1,
                Occ = roleInfo.RoleInfo.Occ,
                PlayerName = roleInfo.RoleInfo.Name
            };
            M2R_RankUnionRaceRequest request = new M2R_RankUnionRaceRequest()
            {
                 RankingInfo = rankingInfo
            };
            long mapInstanceId = DBHelper.GetRankServerId(unit);
            R2M_RankUnionRaceResponse Response = (R2M_RankUnionRaceResponse)await ActorMessageSenderComponent.Instance.Call
                     (mapInstanceId, request);
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
                if (bekill.Type == UnitType.Player)
                {
                    self.TriggerTaskEvent( TastConditionType.KillPlayer_21,0, 1 );
                }
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
                  
                    if (isBoss)
                    {
                        self.TriggerTaskEvent(TastConditionType.KillBOSS_6, 0, 1);
                    }

                    if ((int)fubenDifficulty >= (int)FubenDifficulty.TiaoZhan) //挑战
                    {
                        self.TriggerTaskEvent(TastConditionType.KillTiaoZhanMonsterID_101, unitconfigId, 1);
                        self.TriggerTaskEvent(TastConditionType.KillTianZhanMonsterNumber_121, 0, 1);
                        if (isBoss)
                        {
                            self.TriggerTaskEvent(TastConditionType.KillTianZhanBossNumber_131, 0, 1);
                        }
                    }

                    if ((int)fubenDifficulty == (int)FubenDifficulty.DiYu)  //地狱
                    {
                        self.TriggerTaskEvent(TastConditionType.KillDiYuMonsterID_102, unitconfigId, 1);
                        self.TriggerTaskEvent(TastConditionType.KillDiYuMonsterNumber_122, 0, 1);
                        if (isBoss)
                        {
                            self.TriggerTaskEvent(TastConditionType.KillDiYuBossNumber_132, 0, 1);
                            self.TriggerTaskEvent(TastConditionType.KillDiYuBoss_141, ldMonster.Lv, 1);
                        }
                    }
                }
            }
        }

        //等级更新
        public static void OnUpdateLevel(this TaskComponentServer self, int rolelv)
        {
            self.TriggerTaskEvent(TastConditionType.PlayerLv_1, 0, rolelv);

            if (rolelv == 10)
            {
                self.CheckDailyTask(true);
            }
            self.CheckWeeklyTask(true);
        }

        private static void OnTeskGetTask(this TaskComponentServer self)
        {
               
            if (self.RoleTaskList.Count == 0 && self.RoleComoleteTaskList.Count == 0)
            {
                self.OnAcceptedTask(1);
            }
                    
            if (self.RoleTaskList.Count == 1)
            {
                self.RoleTaskList[0].TrackStatus = 1;
            }

        }
        
        
        private static void OnTeskAddTask(this TaskComponentServer self, int taskid)
        {
            self.OnAcceptedTask(taskid + 1);

            if (self.RoleTaskList.Count == 1)
            {
                self.RoleTaskList[0].TrackStatus = 1;
            }

            self.SendToUpdateTask();
        }

        //登录
        public static void OnLogin(this TaskComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            for (int i = self.RoleTaskList.Count - 1; i >=0; i--)
            {
                if (!LDTaskCategory.Instance.Contain(self.RoleTaskList[i].taskID))
                {
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }
            }

            long unionid = numericComponent.GetAsLong(NumericType.UnionId_0);
            int trialid = numericComponent.GetAsInt(NumericType.TrialDungeonId);
            //触发一下搜集道具类型的任务
            using (self.TaskEventBatch())
            {
                for (int i = 0; i < self.RoleTaskList.Count; i++)
                {
                    TaskPro taskPro = self.RoleTaskList[i];
                    LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);
                    if (ldTask.Condition_Type == TastConditionType.ItemID_Number_2)
                    {
                        self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, ldTask.Param1, 0);
                        continue;
                    }
                    if (ldTask.Condition_Type == TastConditionType.PlayerLv_1)
                    {
                        int roleLv = roleInfoComponentServer.RoleInfo.Lv;
                        self.TriggerTaskEvent(TastConditionType.PlayerLv_1, ldTask.Param1, roleLv);
                        continue;
                    }
                  
                }
            }
            
            using (self.TaskEventBatch())
            {
                self.TriggerTaskEvent(TastConditionType.LoginDayNymber_101, 1, 0);
                self.TriggerTaskEvent(TastConditionType.LoginToday_102, 1, 0);
                self.TriggerTaskEvent( TastConditionType.TrialRank_81, numericComponent.GetAsInt(NumericType.TrialRankId),1 );
                self.TriggerTaskEvent(TastConditionType.PetTianTiRank_82, numericComponent.GetAsInt(NumericType.PetTianTiRankID), 1);
                self.TriggerTaskEvent(TastConditionType.CombatRank_83, numericComponent.GetAsInt(NumericType.CombatRankID), 1);
            }

            self.CheckWeeklyTask(false);
            self.CheckDailyTask(false);
        }

        //收集道具
        public static void OnGetItemForWarehouse(this TaskComponentServer self, int itemId)
        {
            self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, itemId, 0);
        }

        //累计获得道具数量
        public static void OnGetItemNumber(this TaskComponentServer self, int getWay, int itembigType,  int itemId,int itemNumber)
        {
            if (itemId == 1 || (getWay != ItemGetWay.ReceieMail && getWay != ItemGetWay.PaiMaiSell))
            {
                self.TriggerTaskEvent(TastConditionType.GetItemNumber_142, itemId, itemNumber);
            }
        }

        //收集道具
        public static void OnGetItem_2(this TaskComponentServer self, int itemType, int itemId)
        {
            self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, itemId, 0);
        }

        public static void GMCompletCurrentTask(this TaskComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);

                if (taskPro.taskStatus == (int)TaskStatuEnum.Completed)
                {
                    continue;
                }

                taskPro.taskTargetNum_1 = ldTask.Param1;
                taskPro.taskStatus = (int)TaskStatuEnum.Completed;
            }

            M2C_TaskUpdate m2C_TaskUpdate = self.M2C_TaskUpdate;
            m2C_TaskUpdate.RoleTaskList = self.RoleTaskList;
            MessageHelper.SendToClient(unit, m2C_TaskUpdate);
        }

        public static void OnPetMineLogin(this TaskComponentServer self, List<PetMingPlayerInfo> petMingPlayers, List<KeyValuePairInt> extends)
        {
            using (self.TaskEventBatch())
            {
                for (int i = 0; i < petMingPlayers.Count; i++)
                {
                    for (int mineid = petMingPlayers[i].MineType; mineid <= 10003; mineid++)
                    {
                        self.TriggerTaskEvent(TastConditionType.MineHaveNumber_401, mineid, 1);
                    }

                    bool hexin = CommonHelper.IsHexinMine(petMingPlayers[i].MineType, petMingPlayers[i].Postion, extends);
                    if (hexin)
                    {
                        self.TriggerTaskEvent(TastConditionType.MineHaveNumber_401, 0, 1);
                    }
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

        public static void TriggerTaskEvent(this TaskComponentServer self, int conditionType, int param1, int param2)
        {
            if (self.TaskEventBatchDepth > 0)
            {
                var key = (conditionType, param2);
                self.TaskEventCoalesce.TryGetValue(key, out int sum);
                self.TaskEventCoalesce[key] = sum + param1;
                return;
            }

            self.ApplyTaskEvent(conditionType, param1, param2);
        }

        private static void FlushTaskEventBatch(this TaskComponentServer self)
        {
            if (self.TaskEventCoalesce.Count == 0)
            {
                return;
            }

            bool updateTask = false;
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                if (taskPro.taskStatus == (int)TaskStatuEnum.Completed)
                {
                    continue;
                }

                LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);
                if (!self.TaskEventCoalesce.TryGetValue((ldTask.Condition_Type, ldTask.Param2), out int delta))
                {
                    continue;
                }

                updateTask = true;
                taskPro.taskTargetNum_1 += delta;
                bool completed = taskPro.taskTargetNum_1 >= ldTask.Param1;
                taskPro.taskStatus = completed ? (int)TaskStatuEnum.Completed : (int)TaskStatuEnum.Accepted;
            }

            self.TaskEventCoalesce.Clear();
            if (updateTask)
            {
                self.SendToUpdateTask();
            }
        }

        private static void ApplyTaskEvent(this TaskComponentServer self, int conditionType, int param1, int param2)
        {
            bool updateTask = false;

            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);
                if (ldTask.Condition_Type != conditionType)
                {
                    continue;
                }
                if (ldTask.Param2 != param2)
                {
                    continue;
                }
                if (taskPro.taskStatus == (int)TaskStatuEnum.Completed)
                {
                    continue;
                }
                updateTask = true;
                taskPro.taskTargetNum_1 += param1;
                bool completed = taskPro.taskTargetNum_1 >= ldTask.Param1;
                taskPro.taskStatus = completed ? (int)TaskStatuEnum.Completed : (int)TaskStatuEnum.Accepted;
            }

            if (!updateTask)
            {
                return;
            }

            self.SendToUpdateTask();
        }
        

        public static void CheckDailyTask(this TaskComponentServer self, bool notice)
        {
            if (self.HasTaskByGroup(TaskTypeEnum.Daily))
            {
                return;
            }

            self.InitTasksByType(TaskTypeEnum.Daily);
        }

        public static void CheckRingTask(this TaskComponentServer self)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            /*if (numericComponent.GetAsInt(NumericType.RingTaskId) == 0 && numericComponent.GetAsInt(NumericType.RingTaskNumber) < 1)
            {
                //self.ClearTypeTask(TaskTypeEnum.Ring);

                int roleLv = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
                int ringTaskId = TaskHelper.GetTaskIdByType(TaskTypeEnum.Ring, roleLv);
                numericComponent.ApplyValue(NumericType.RingTaskId, ringTaskId, false);
            }*/
        }

        public static void CheckWeeklyTask(this TaskComponentServer self, bool notice)
        {
            if (self.HasTaskByGroup(TaskTypeEnum.Weekly))
            {
                return;
            }

            self.InitTasksByType(TaskTypeEnum.Weekly);
        }

        /// <summary>
        /// 是否已有指定 Group（日常/周常等）任务；兼容旧数据 TaskType 未写入的情况。
        /// </summary>
        public static bool HasTaskByGroup(this TaskComponentServer self, int taskGroup)
        {
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                TaskPro taskPro = self.RoleTaskList[i];
                if (taskPro.TaskType == taskGroup)
                {
                    return true;
                }

                if (!LDTaskCategory.Instance.Contain(taskPro.taskID))
                {
                    continue;
                }

                if (LDTaskCategory.Instance.Get(taskPro.taskID).Group == taskGroup)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按类型从配置初始化任务列表。有新建任务时推送客户端。
        /// </summary>
        public static void InitTasksByType(this TaskComponentServer self, int taskType)
        {
            List<int> taskIds = TaskHelper.GenerateTaskListByType(taskType);
            if (taskIds.Count == 0)
            {
                return;
            }

            HashSet<int> existing = new HashSet<int>();
            for (int i = 0; i < self.RoleTaskList.Count; i++)
            {
                existing.Add(self.RoleTaskList[i].taskID);
            }

            bool created = false;
            for (int i = 0; i < taskIds.Count; i++)
            {
                int taskId = taskIds[i];
                if (existing.Contains(taskId))
                {
                    continue;
                }
                self.CreateTask(taskId);
                created = true;
            }

            if (created)
            {
                self.SendToUpdateTask();
            }
        }


        public static void ClearTypeTask(this TaskComponentServer self, int taskType)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].TaskType != taskType)
                {
                    continue;
                }
                int taskId = self.RoleTaskList[i].taskID;
                if (completedTaskIds.Contains(taskId))
                {
                    self.RoleComoleteTaskList.Remove(taskId);
                    completedTaskIds.Remove(taskId);
                }
                self.RoleTaskList.RemoveAt(i);
            }
        }

        public static void UpdateDayTask(this TaskComponentServer self, bool notice)
        {
            //清空每日任务
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            int roleLv = roleInfoComponent.RoleInfo.Lv;
            System.DateTime dateTime = TimeHelper.DateTimeNow();
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if(!LDTaskCategory.Instance.Contain(self.RoleTaskList[i].taskID))
                {
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }

                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID);
                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Daily || ldTask.Group == TaskTypeEnum.Daily)
                {
                    if (completedTaskIds.Contains(ldTask.Id))
                    {
                        self.RoleComoleteTaskList.Remove(ldTask.Id);
                        completedTaskIds.Remove(ldTask.Id);
                    }
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }
            }

            List<int> taskCountryList = TaskHelper.GenerateTaskListByType(TaskTypeEnum.Daily);
            for (int i = 0; i < taskCountryList.Count; i++)
            {
                self.CreateTask(taskCountryList[i]);
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
       
        }


        public static void UpdateWeeklyTask(this TaskComponentServer self, bool notice)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (!LDTaskCategory.Instance.Contain(self.RoleTaskList[i].taskID))
                {
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }

                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Weekly
                    || LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID).Group == TaskTypeEnum.Weekly)
                {
                    int taskId = self.RoleTaskList[i].taskID;
                    if (completedTaskIds.Contains(taskId))
                    {
                        self.RoleComoleteTaskList.Remove(taskId);
                        completedTaskIds.Remove(taskId);
                    }
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }
            }
            for (int i = self.RoleComoleteTaskList.Count - 1; i >= 0; i--)
            {
                if (!LDTaskCategory.Instance.Contain(self.RoleComoleteTaskList[i]))
                {
                    continue;
                }

                /*if (ldTask.TaskType == TaskTypeEnum.Weekly)
                {
                    self.RoleComoleteTaskList.RemoveAt(i);
                    continue;
                }*/
            }

            List<int> weeklyTaskList = TaskHelper.GenerateTaskListByType(TaskTypeEnum.Weekly);
            for (int i = 0; i < weeklyTaskList.Count; i++)
            {
                self.CreateTask(weeklyTaskList[i]);
            }

            if (notice)
            {
                self.SendToUpdateTask();
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
       
        public static void CheckWeeklyUpdate(this TaskComponentServer self, long lastTime, long curTime)
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

        public static void SendToUpdateTask(this TaskComponentServer self)
        {  
            Unit unit = self.GetParent<Unit>();
            M2C_TaskUpdate m2C_TaskUpdate = self.M2C_TaskUpdate;
            m2C_TaskUpdate.RoleTaskList = self.RoleTaskList;
            MessageHelper.SendToClient(unit, m2C_TaskUpdate);
        }

        /// <summary>
        /// 重置每日活跃
        /// </summary> 
        /// <param name="self"></param>
        public static void OnZeroClockUpdate(this TaskComponentServer self, bool notice)
        {
            self.OnLineTime = 0;
         
            self.UpdateDayTask(notice);
           
            if (notice)
            {
                self.SendToUpdateTask();
            }
        }
    }
}
