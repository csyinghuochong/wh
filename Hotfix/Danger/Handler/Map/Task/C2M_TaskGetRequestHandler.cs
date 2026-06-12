using System;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskGetRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskGetRequest, M2C_TaskGetResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskGetRequest request, M2C_TaskGetResponse response, Action reply)
        {
            if (!LDTaskCategory.Instance.Contain(request.TaskId))
            {
                Log.Error($"C2M_TaskGetRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            LDTask ldTask = LDTaskCategory.Instance.Get(request.TaskId);
           // if (ldTask.TaskType == TaskTypeEnum.Daily)
            {
                TaskComponent taskComponent = unit.GetComponent<TaskComponent>();
                if (taskComponent.GetTaskList(TaskTypeEnum.Daily).Count > 0)
                {
                    response.Error = ErrorCode.ERR_TaskCanNotGet;
                    reply();
                    return;
                }

                //获取当前任务是否已达上限
                if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.DailyTaskNumber) >=  GlobalValueCategory.Instance.TempValue)
                {
                    response.Error = ErrorCode.ERR_ShangJinNumFull;
                    reply();
                    return;
                }

                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                int dailyTask = numericComponent.GetAsInt(NumericType.DailyTaskID);
                if (dailyTask == 0)
                {
                    LogHelper.LogDebug($"{unit.Id}  dailyTask == 0");
                    response.Error = ErrorCode.ERR_TaskCanNotGet;
                    reply();
                    return;
                }
                response.TaskPro = taskComponent.OnGetDailyTask(dailyTask);
            }
            /*else if (ldTask.TaskType == TaskTypeEnum.Union)
            {
                TaskComponent taskComponent = unit.GetComponent<TaskComponent>();
                if (taskComponent.GetTaskList(TaskTypeEnum.Union).Count > 0)
                {
                    response.Error = ErrorCode.ERR_TaskNoComplete;
                    reply();
                    return;
                }

                //获取当前任务是否已达上限
                int uniontask = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.UnionTaskNumber);
                if (uniontask >= GlobalValueCategory.Instance.TempValue)
                {
                    response.Error = ErrorCode.ERR_TaskLimited;
                    reply();
                    return;
                }

                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                int unionTaskId = numericComponent.GetAsInt(NumericType.UnionTaskId);
                if (unionTaskId == 0)
                {
                    LogHelper.LogDebug($"{unit.Id}  unionTaskId == 0");
                    response.Error = ErrorCode.ERR_TaskCanNotGet;
                    reply();
                    return;
                }
                response.TaskPro = taskComponent.OnGetDailyTask(unionTaskId);
            }
            else if (ldTask.TaskType == TaskTypeEnum.Treasure
                || ldTask.TaskType == TaskTypeEnum.Ring)
            {
                if (unit.GetComponent<TaskComponent>().GetTaskList(ldTask.TaskType).Count > 1)
                {
                    Log.Error($"C2M_TaskGetRequest 2");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
                (TaskPro taskPro, int error) = unit.GetComponent<TaskComponent>().OnAcceptedTask(request.TaskId);
                response.Error = error;
                response.TaskPro = taskPro;

                if(ldTask.TaskType == TaskTypeEnum.Treasure && error == ErrorCode.ERR_Success)
                {
                    NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                    int treasureTask = numericComponent.GetAsInt(NumericType.TreasureTask);
                    numericComponent.ApplyValue(NumericType.TreasureTask, treasureTask + 1);
                }

                MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();  
                if (ldTask.TaskType == TaskTypeEnum.Treasure && error == ErrorCode.ERR_Success
                    && mapComponent.MapTypeEnum == MapTypeEnum.LocalDungeon && taskPro.FubenId == mapComponent.SceneId)
                {
                    //Console.WriteLine("副本内接取藏宝图任务！");
                    
                    int wave = taskPro.WaveId;             //第几波
                    int monsterid = ldTask.Target[0];       //怪物

                    string[] vector3 = SceneConfigHelper.GetPostionMonster(taskPro.FubenId, ldTask.Target[0], wave);
                    if (vector3 != null) 
                    {
                        Vector3 target = new Vector3(float.Parse(vector3[0]), float.Parse(vector3[1]), float.Parse(vector3[2]));
                        MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(monsterid);    
                        Unit unitmonster = UnitFactory.CreateMonster(unit.DomainScene(), monsterid, target, new CreateMonsterInfo()
                        {
                            Camp = monsterConfig.MonsterCamp,
                            SkinId = 0,
                        });
                    }
                }
            }
            else
            {
                (TaskPro taskPro, int error) = unit.GetComponent<TaskComponent>().OnAcceptedTask(request.TaskId);
                response.Error = error;
                response.TaskPro = taskPro;
            }*/
            reply();
            await ETTask.CompletedTask;
        }
    }
}
