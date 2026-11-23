using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_TaskNavigateRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskNavigateRequest, M2C_TaskNavigateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskNavigateRequest request, M2C_TaskNavigateResponse response, Action reply)
        {
            response.Error = GetRoute(unit, request, response);
            reply();
            await ETTask.CompletedTask;
        }

        private static int GetRoute(Unit unit, C2M_TaskNavigateRequest request, M2C_TaskNavigateResponse response)
        {
            TaskComponentServer tasks = unit.GetComponent<TaskComponentServer>();
            TaskPro task;
            if (request.TaskTable == TaskTableType.Task_1 && LDTask_1Category.Instance.Contain(request.TaskId))
            {
                task = tasks.GetTaskById_1(request.TaskId);
                if (task == null || task.taskStatus == TaskStatuEnum.UnActive)
                {
                    RoleInfo role = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
                    if (!TaskHelper.GetOpenTask1Ids(0, tasks.NextTask1_Id, tasks.RoleTaskList_1,
                        tasks.RoleComoleteTaskList_1, role.Lv, role.Occ).Contains(request.TaskId))
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }

                    task = new TaskPro { taskID = request.TaskId, taskStatus = TaskStatuEnum.UnActive };
                }
            }
            else if (request.TaskTable == TaskTableType.Task_2 && LDTask_2Category.Instance.Contain(request.TaskId))
            {
                task = tasks.GetTaskById_2(request.TaskId);
            }
            else
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            if (task == null || task.taskStatus == TaskStatuEnum.Commited)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            if (!TaskNavigationHelper.TryGetTarget(task, request.TaskTable, out int unitType, out int configId))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            MapComponent map = unit.DomainScene().GetComponent<MapComponent>();
            response.SourceSceneId = map.SceneId;
            LDScene_Creature creature = SceneRouteHelper.FindTarget(map.SceneId, unit.Position, unitType, configId,
                map.MapTypeEnum == MapTypeEnum.LocalDungeon, response.TeleportIds);
            if (creature == null)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            response.CreatureId = creature.Id;
            response.NeedReturn = creature.Scene_Id != map.SceneId && response.TeleportIds.Count == 0;
            return ErrorCode.ERR_Success;
        }
    }
}
