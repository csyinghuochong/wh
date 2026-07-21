using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ObjectSystem]
    public class TaskComponentAwakeSystem : AwakeSystem<TaskComponentServer>
    {
        public override void Awake(TaskComponentServer self)
        {
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
                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleComoleteTaskList[i]);
                /*if (ldTask.TaskType == TaskTypeEnum.Main)
                {
                    mainTaskNumber++;
                }*/
            }
            return mainTaskNumber;
        }

        public static int GetHuoYueDu(this TaskComponentServer self)
        {
            int huoYueDu = 0;
           
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
                LDTask ldTask = LDTaskCategory.Instance.Get(taskPro.taskID);
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
            if (bagComponentServer.GetBagLeftCell() < needcell)
            {
                return ErrorCode.ERR_BagIsFull;
            }
            
            if (bagComponentServer.GetBagLeftCell()  < rewardItems.Count)
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
            bagComponentServer.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.TaskReward}_{TimeHelper.ServerNow()}");
            
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
            self.TriggerTaskEvent(TastConditionType.PetNumber1_11, 0, 1);
          
            self.TriggerTaskEvent(TastConditionType.PetHeCheng_23, 0, 1);
           
            self.TriggerTaskEvent(TastConditionType.PetNumber2_24, 0, 1);
           
            self.TriggerTaskEvent(TastConditionType.PetNSkill_18, 0, rolePetInfo.PetSkill.Count);
           
            int combat = PetHelper.PetPingJia(rolePetInfo);
            self.TriggerTaskEvent(TastConditionType.PetHeChengCombat_32, combat, 1);
        
        }

        /// <summary>
        /// 获得宠物
        /// </summary>
        /// <param name="self"></param>
        public static void OnGetPet(this TaskComponentServer self, RolePetInfo rolePetInfo)
        {
            self.TriggerTaskEvent( TastConditionType.PetNumber1_11, 0, 1 );
          
            self.TriggerTaskEvent(TastConditionType.PetNumber2_24, 0, 1);
          
            self.TriggerTaskEvent( TastConditionType.PetNSkill_18,  0, rolePetInfo.PetSkill.Count);
          
            self.TriggerTaskEvent(TastConditionType.PetNumber_31, 0, 1);
           
        }

        /// <summary>
        /// 道具洗练
        /// </summary>
        /// <param name="self"></param>
        public static void OnEquipXiLian(this TaskComponentServer self, int times)
        {
            self.TriggerTaskEvent( TastConditionType.EquipXiLian_13, 0, times);
          
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

        /// <summary>
        /// 通关副本
        /// </summary>
        /// <param name="self"></param>
        /// <param name="difficulty"></param>
        /// <param name="chapterid"></param>
        /// <param name="star"></param>
        public static void OnPassFuben(this TaskComponentServer self, int difficulty, int chapterid, int star)
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

        public static void OnWinCampBattle(this TaskComponentServer self)
        {
           
        }

        public static void OnPassTeamFuben(this TaskComponentServer self)
        {
            
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

            if (bekill.Type == UnitType.Player && sceneType == MapTypeEnum.Battle)
            {
           
            }
            if (bekill.Type == UnitType.Player && sceneType == MapTypeEnum.UnionRace)
            {
            
                self.UpdateUnionRaceRank().Coroutine();
            }
            if (bekill.Type == UnitType.Player)
            {
                self.TriggerTaskEvent( TastConditionType.KillPlayer_21,0, 1 );
             
            }
            if (bekill.Type == UnitType.Monster)
            {
                int unitconfigId = bekill.ConfigId;
                LDMonster ldMonster = LDMonsterCategory.Instance.Get(unitconfigId);
                bool isBoss = ldMonster.Type == (int)MonsterTypeEnum.Boss;
                Scene domainScene = self.GetParent<Unit>().DomainScene();
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

        //等级更新
        public static void OnUpdateLevel(this TaskComponentServer self, int rolelv)
        {
            self.TriggerTaskEvent(TastConditionType.PlayerLv_1, 0, rolelv);

            if (rolelv == 10)
            {
                self.CheckDailyTask(true);
            }
            self.CheckWeeklyTask();
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

                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.System)
                {
                    self.RoleTaskList[i].TrackStatus = 0;
                }
            }

            self.OnTeskGetTask();
    

            long unionid = numericComponent.GetAsLong(NumericType.UnionId_0);
            int trialid = numericComponent.GetAsInt(NumericType.TrialDungeonId);
            //触发一下搜集道具类型的任务
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
                if (ldTask.Condition_Type == TastConditionType.JoinUnion_9)
                {
                    self.TriggerTaskEvent(TastConditionType.JoinUnion_9, ldTask.Param1, unionid > 0 ? 1 : 0);
                    continue;
                }
                if (ldTask.Condition_Type == TastConditionType.CombatToValue_133)
                {
                    int combat = roleInfoComponentServer.RoleInfo.Combat;
                    self.TriggerTaskEvent(TastConditionType.CombatToValue_133, 0, combat);
                    continue;
                }
                if (ldTask.Condition_Type == TastConditionType.TrialTowerCeng_134)
                {
                    if (trialid >= ldTask.Param1)
                    {
                        self.TriggerTaskEvent(TastConditionType.TrialTowerCeng_134, ldTask.Param1, 1);
                    }
                } 
            }
            
            /*if (numericComponent.GetAsInt(NumericType.DailyTaskID) == 0)
            {
                //self.UpdateDayTask(false);
                self.CheckDailyTask(false);
            }*/
            /*if (numericComponent.GetAsInt(NumericType.RingTaskId) == 0 )
            {
                self.CheckRingTask();
            }
            if (numericComponent.GetAsInt(NumericType.UnionTaskId) == 0 )
            {
                self.CheckUnionTask();
            }
            if (numericComponent.GetAsInt(NumericType.WeeklyTaskId) == 0)
            {
                self.CheckWeeklyTask();
            }
            if (numericComponent.GetAsInt(NumericType.SystemTask) == 0)
            {
                self.CheckSystemTask();
            }*/

            self.UpdateTargetTask(false);
            self.InitActivityV1Task();
            self.InitActivityWeekTask(false);

            //numericComponent.ApplyValue(NumericType.Numeric_Error, chat2G_EnterChat.RankId, false, false);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, chat2G_EnterChat.PetRankId, false, false);
            //numericComponent.ApplyValue(NumericType.SoloRankId, chat2G_EnterChat.SoloRankId, false, false);
            //numericComponent.ApplyValue(NumericType.TrialRankId, chat2G_EnterChat.TrialRankId, false, false);
            self.TriggerTaskEvent( TastConditionType.TrialRank_81, numericComponent.GetAsInt(NumericType.TrialRankId),1 );

            self.TriggerTaskEvent(TastConditionType.PetTianTiRank_82, numericComponent.GetAsInt(NumericType.PetTianTiRankID), 1);

            self.TriggerTaskEvent(TastConditionType.CombatRank_83, numericComponent.GetAsInt(NumericType.CombatRankID), 1);
        }

        //收集道具
        public static void OnGetItemForWarehouse(this TaskComponentServer self, int itemId)
        {
            self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, itemId, 0);
        }

        //累计获得道具数量
        public static void OnGetItemNumber(this TaskComponentServer self, int getWay, int itemId,int itemNumber)
        {
            if (itemId == 1 || (getWay != ItemGetWay.ReceieMail && getWay != ItemGetWay.PaiMaiSell))
            {
                self.TriggerTaskEvent(TastConditionType.GetItemNumber_142, itemId, itemNumber);
            }

            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            if (ldItem.ItemType == ItemTypeEnum.Equipment && ldItem.Quality >= 5)
            {
                self.TriggerTaskEvent(TastConditionType.GetOrangeEquip_139, ldItem.UseLv, 1);
            }
        }

        //收集道具
        public static void OnGetItem_2(this TaskComponentServer self, int itemId)
        {
            self.TriggerTaskEvent(TastConditionType.ItemID_Number_2, itemId, 0);
        }

        public static void GMCompletCurrentTask(this TaskComponentServer self)
        {
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
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2C_TaskUpdate);
        }

        public static void OnPetMineLogin(this TaskComponentServer self, List<PetMingPlayerInfo> petMingPlayers, List<KeyValuePairInt> extends)
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

        
        
        public static void TriggerTaskEvent(this TaskComponentServer self, int conditionType, int param1, int param2)
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
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            /*if (numericComponent.GetAsInt(NumericType.DailyTaskID) != 0)
            {
                return;
            }*/
            /*
            if (numericComponent.GetAsInt(NumericType.DailyTaskNumber) >= 1)
            {
                return;
            }
            */

            int roleLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
           // numericComponent.ApplyValue(NumericType.DailyTaskID, TaskHelper.GetTaskIdByType(TaskTypeEnum.Daily, roleLv), notice);
        }

        public static void CheckRingTask(this TaskComponentServer self)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            /*if (numericComponent.GetAsInt(NumericType.RingTaskId) == 0 && numericComponent.GetAsInt(NumericType.RingTaskNumber) < 1)
            {
                //self.ClearTypeTask(TaskTypeEnum.Ring);

                int roleLv = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>().RoleInfo.Level;
                int ringTaskId = TaskHelper.GetTaskIdByType(TaskTypeEnum.Ring, roleLv);
                numericComponent.ApplyValue(NumericType.RingTaskId, ringTaskId, false);
            }*/
        }

        public static void CheckWeeklyTask(this TaskComponentServer self)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            /*if (numericComponent.GetAsInt(NumericType.WeeklyTaskId) == 0 && numericComponent.GetAsInt(NumericType.WeeklyTaskNumber) < 1)
            {
                //self.ClearTypeTask(TaskTypeEnum.Ring);
                int roleLv = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>().RoleInfo.Level;
                int weekTaskId = TaskHelper.GetTaskIdByType(TaskTypeEnum.Weekly, roleLv);
                numericComponent.ApplyValue(NumericType.WeeklyTaskId, weekTaskId, false);
            }*/
        }

        public static void CheckSystemTask(this TaskComponentServer self)
        {
            bool have = false;
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (self.RoleTaskList[i].TaskType != TaskTypeEnum.System)
                {
                    continue;
                }

                have = true;
                break;
            }

            if (have)
            {
                return;
            }

            /// int curTakskid = 0;self.GetParent<Unit>().GetComponent<NumericComponent>().GetAsInt(NumericType.SystemTask);
            foreach ((int taskid, LDTask taskcofnig) in LDTaskCategory.Instance.GetAll())
            {
                /*if (taskcofnig.TaskType != TaskTypeEnum.System || taskid <= curTakskid)
                {
                    continue;
                }*/

                //self.OnAcceptedTask(taskid);
                break;
            }

           self.SendToUpdateTask();
        }

        public static void CheckUnionTask(this TaskComponentServer self)
        {
            /*NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.UnionTaskId) == 0 && numericComponent.GetAsInt(NumericType.UnionTaskNumber) < 1)
            {

                int roleLv = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>().RoleInfo.Level;
                numericComponent.ApplyValue(NumericType.UnionTaskId, TaskHelper.GetTaskIdByType(TaskTypeEnum.Union, roleLv), false);
            }*/
        }

        public static void OnResetSeason(this TaskComponentServer self, bool notice)
        {
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID);
                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Season)
                {
                    self.RoleTaskList.RemoveAt(i);  
                }
            }
            
        }

        public static void InitSeasonMainTask(this TaskComponentServer self, bool notice)
        {
            bool have = false;
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID);
                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Season)
                {
                    have = true;
                    break;
                }
            }
            if (have)
            {
                return;
            }

           // int curTakskid = self.GetParent<Unit>().GetComponent<NumericComponent>().GetAsInt(NumericType.SeasonTask);
            foreach ( ( int taskid, LDTask taskcofnig ) in LDTaskCategory.Instance.GetAll())
            {
                /*if (taskcofnig.TaskType == TaskTypeEnum.Season && taskid > curTakskid)
                {
                    self.OnAcceptedTask(taskid);
                    break;   
                }*/
            }

            if (notice) 
            {
               self.SendToUpdateTask();
            }
        }

        public static void UpdateTargetTask(this TaskComponentServer self, bool notice)
        {
            int createDay = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>().GetCrateDay();
            if (createDay == 0 || createDay > CommonConfig.WelfareTaskList.Count)
            {
                return;
            }

            //所有任务
            List<int> taskids = new List<int>();
            for (int i = 0; i < createDay; i++)
            {
                taskids.AddRange(CommonConfig.WelfareTaskList[i]);
            }
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = 0; i < taskids.Count; i++)
            {
                
                if (self.GetTaskById(taskids[i]) != null)
                {
                    continue;
                }
                if (completedTaskIds.Contains(taskids[i]))
                {
                    continue;
                }

                self.CreateTask(taskids[i]);
            }
        }

        public static void ClearTypeTask(this TaskComponentServer self, int taskType)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID);
                if (self.RoleTaskList[i].TaskType == taskType)
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
        }

        public static void UpdateDayTask(this TaskComponentServer self, bool notice)
        {

            //清空每日任务
            Unit unit = self.GetParent<Unit>();
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
                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Daily
                    || self.RoleTaskList[i].TaskType == TaskTypeEnum.Union)
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

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int roleLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
           
            /*
            numericComponent.ApplyValue(NumericType.DailyTaskNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.UnionTaskNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.DailyTaskID, TaskHelper.GetTaskIdByType(TaskTypeEnum.Daily, roleLv), notice);
            numericComponent.ApplyValue(NumericType.UnionTaskId, TaskHelper.GetTaskIdByType(TaskTypeEnum.Union, roleLv), notice);
            */

            //int ringTaskId = TaskHelper.GetTaskIdByType(TaskTypeEnum.Ring, roleLv);
            //numericComponent.ApplyValue(NumericType.RingTaskId, ringTaskId, notice);
            //Log.Debug($"更新每日任务: {numericComponent.GetAsInt(NumericType.DailyTaskID)}");
        }

        public static TaskPro GetTreasureMonster(this TaskComponentServer self, int fubenid)
        {
            List<TaskPro> taskPros = self.GetTaskList(TaskTypeEnum.Treasure);
            for (int i = 0; i < taskPros.Count; i++)
            {
                if (taskPros[i].taskStatus >= (int)TaskStatuEnum.Completed)
                {
                    continue;
                }
                if (taskPros[i].FubenId != fubenid)
                {
                    continue;
                }
                return taskPros[i];
            }
            return null;
        }

        public static void CheckWeeklyUpdate(this TaskComponentServer self)
        {
            System.DateTime dateTime = TimeHelper.DateTimeNow();
            if( dateTime.DayOfWeek == System.DayOfWeek.Monday)
            {
                //Log.Console($"ResetWeeklyTask: passday:{self.Id} {dateTime.DayOfWeek == System.DayOfWeek.Monday}");
                self.ResetWeeklyTask(true);
            }
        }

        public static void ResetWeeklyTask(this TaskComponentServer self, bool notice)
        {
            HashSet<int> completedTaskIds = new HashSet<int>(self.RoleComoleteTaskList);
            for (int i = self.RoleTaskList.Count - 1; i >= 0; i--)
            {
                if (!LDTaskCategory.Instance.Contain(self.RoleTaskList[i].taskID))
                {
                    self.RoleTaskList.RemoveAt(i);
                    continue;
                }

                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleTaskList[i].taskID);
                if (self.RoleTaskList[i].TaskType == TaskTypeEnum.Weekly
                    || self.RoleTaskList[i].TaskType == TaskTypeEnum.Ring)
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
            for (int i = self.RoleComoleteTaskList.Count - 1; i >= 0; i--)
            {
                if (!LDTaskCategory.Instance.Contain(self.RoleComoleteTaskList[i]))
                {
                    continue;
                }

                LDTask ldTask = LDTaskCategory.Instance.Get(self.RoleComoleteTaskList[i]);
                /*if (ldTask.TaskType == TaskTypeEnum.Weekly)
                {
                    self.RoleComoleteTaskList.RemoveAt(i);
                    continue;
                }*/
            }

            Unit unit = self.GetParent<Unit>();
            int roleLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();  
            /*
            numericComponent.ApplyValue(NumericType.RingTaskNumber, 0, false);
            numericComponent.ApplyValue(NumericType.RingTaskId, TaskHelper.GetTaskIdByType(TaskTypeEnum.Ring, roleLv), false);

            numericComponent.ApplyValue(NumericType.WeeklyTaskNumber, 0, false);
            numericComponent.ApplyValue(NumericType.WeeklyTaskId, TaskHelper.GetTaskIdByType(TaskTypeEnum.Weekly, roleLv), false);
            */

            self.UpdateSeasonWeekTask(false);
            self.UpdateActivityWeekTask(notice);
        }

        public static void InitActivityV1Task(this TaskComponentServer self, bool notice = false)
        {
            //if (!ConfigData.V1ActivityList.Contains(ActivityConfigHelper.ActivityV1_Task))
            //{
            //    return;
            //}
            
            List<int> taskCountryList = TaskHelper.GetActivityV1Task(self.GetParent<Unit>(), 120) ;
        }

        public static List<TaskPro> GetTaskCountryByType(this TaskComponentServer self, int tasktype)
        {
            List<TaskPro> taskPros = new List<TaskPro> { }; 
            
            return taskPros;
        }

        public static void InitActivityWeekTask(this TaskComponentServer self, bool notice)
        {
          
         
        }

        public static void UpdateActivityWeekTask(this TaskComponentServer self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            Log.Warning($"新活动任务清空: {unit.DomainZone()} {unit.Id}");

        

            List<int> taskCountryList = TaskHelper.GetActivityV1Task(unit, 120);

            bool isduihuan = unit.GetComponent<ActivityComponentServer>().ActivityV1Info.PointsReward.Count > 0;

            //每次活动扣除100积分， 对话任意积分可免扣除
            unit.GetComponent<ActivityComponentServer>().ActivityV1Reset(notice);
        }

        public static void UpdateSeasonWeekTask(this TaskComponentServer self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ApplyValue(NumericType.SeasonTowerId, 0, notice);

            Log.Warning($"赛季之塔清空: {unit.DomainZone()} {unit.Id}");

            //赛季任务每周清空
            
        }

        public static void CheckWeeklyUpdate(this TaskComponentServer self, long lastTime, long curTime)
        {
            //判断条件。 超过一周或者过了周末
            float passday = ((curTime - lastTime) * 1f / TimeHelper.OneDay);
            if (passday >= 7)
            {
                //Log.Warning($"ResetWeeklyTask: passday:{self.Id} {passday}");
                self.ResetWeeklyTask(false);
            }
            else
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastTime);
                DateTime curdateTime = TimeInfo.Instance.ToDateTime(curTime);
                if ((curdateTime.DayOfWeek < lastdateTime.DayOfWeek && curdateTime.DayOfWeek!= 0)
                 || (curdateTime.DayOfWeek > lastdateTime.DayOfWeek && lastdateTime.DayOfWeek == 0))
                {
                    Log.Warning($"ResetWeeklyTask:{self.Id} {curdateTime.DayOfWeek} {lastdateTime.DayOfWeek}");
                    self.ResetWeeklyTask(false);
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
            self.UpdateTargetTask(notice);
           
            if (notice)
            {
                self.SendToUpdateTask();
            }
        }
    }
}
