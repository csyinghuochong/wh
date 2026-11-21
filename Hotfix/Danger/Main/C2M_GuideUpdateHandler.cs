using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_GuideUpdateHandler : AMActorLocationRpcHandler<Unit, C2M_GuideUpdateRequest, M2C_GuideUpdateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_GuideUpdateRequest request, M2C_GuideUpdateResponse response, Action reply)
        {
            await ETTask.CompletedTask;
            response.Error = BeginnerGuideServerHelper.Update(unit, request, response);
            reply();
        }
    }

    public static class BeginnerGuideServerHelper
    {
        /// <summary>校验并保存启动、顺序推进、配置回溯或整组完成；已完成组重复请求直接成功回包。</summary>
        public static int Update(Unit unit, C2M_GuideUpdateRequest request, M2C_GuideUpdateResponse response)
        {
            if (!BeginnerGuideConfigHelper.TryGetSteps(request.GuideId, out List<LDBeginner_Guide> steps, out _)
                || request.GuideStep < 0 || request.ContextId < 0)
            {
                return ErrorCode.ERR_Parameter;
            }

            RoleInfo role = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (role.CompleteGuideIds.Contains(request.GuideId))
            {
                response.Completed = true;
                return ErrorCode.ERR_Success;
            }

            BeginnerGuideProgress progress = role.BeginnerGuideProgresses.Find(x => x.GuideId == request.GuideId);
            if (progress == null)
            {
                if (request.GuideStep != request.GuideId || !CheckTrigger(unit, steps[0])
                    || !CheckContext(unit, steps, request.ContextId))
                {
                    return ErrorCode.ERR_Parameter;
                }

                progress = new BeginnerGuideProgress { GuideId = request.GuideId, StepId = request.GuideStep, ContextId = request.ContextId };
                role.BeginnerGuideProgresses.Add(progress);
                response.Progress = progress;
                return ErrorCode.ERR_Success;
            }

            if (request.ContextId != progress.ContextId)
            {
                return ErrorCode.ERR_Parameter;
            }

            LDBeginner_Guide current = steps.Find(x => x.Id == progress.StepId);
            if (current == null)
            {
                return ErrorCode.ERR_Parameter;
            }

            if (request.GuideStep == 0)
            {
                // 只有保存进度已到末步才接受整组完成；完成列表记录组首ID。
                if (current.Next_Id.Length != 0)
                {
                    return ErrorCode.ERR_Parameter;
                }

                role.BeginnerGuideProgresses.Remove(progress);
                role.CompleteGuideIds.Add(request.GuideId);
                response.Completed = true;
                return ErrorCode.ERR_Success;
            }

            bool next = current.Next_Id.Length == 1 && current.Next_Id[0] == request.GuideStep;
            if (request.GuideStep != current.Id && request.GuideStep != current.Break_Backup_Id && !next)
            {
                return ErrorCode.ERR_Parameter;
            }

            progress.StepId = request.GuideStep;
            response.Progress = progress;
            return ErrorCode.ERR_Success;
        }

        /// <summary>新建进度时校验服务器可确认的触发条件；装备事件还需通过实例归属校验。</summary>
        private static bool CheckTrigger(Unit unit, LDBeginner_Guide root)
        {
            TaskComponentServer tasks = unit.GetComponent<TaskComponentServer>();
            switch (root.Trigger_Condition)
            {
                case 1:
                    return unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum == MapTypeEnum.MainCityScene;
                case 2:
                    return unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv >= root.Trigger_Condition_Param[0];
                case 3:
                    return tasks.RoleComoleteTaskList_1.Contains(root.Trigger_Condition_Param[0]);
                case 4:
                    int taskId = root.Trigger_Condition_Param[0];
                    return tasks.RoleComoleteTaskList_1.Contains(taskId)
                        || tasks.RoleTaskList_1.Exists(x => x.taskID == taskId
                            && (x.taskStatus == TaskStatuEnum.Accepted || x.taskStatus == TaskStatuEnum.Completed));
                case 5:
                    return true; // 获得装备事件由客户端上报；必须绑定本角色持有的真实装备实例。
                default:
                    return false;
            }
        }

        /// <summary>装备或灵宠必须属于当前角色；不需要实例的组只接受contextId=0。</summary>
        private static bool CheckContext(Unit unit, List<LDBeginner_Guide> steps, long contextId)
        {
            if (BeginnerGuideConfigHelper.ContainsPosition(steps, BeginnerGuidePosition.Equipment))
            {
                BagInfo item = unit.GetComponent<BagComponentServer>().GetItemByUId(contextId);
                return item != null && item.ItemType == ItemBigType.Type_Equip
                    && (item.Loc == (int)ItemLocType.ItemLocBag || item.Loc == (int)ItemLocType.ItemLocEquip);
            }
            if (BeginnerGuideConfigHelper.ContainsPosition(steps, BeginnerGuidePosition.PetFight))
            {
                return contextId > 0 && unit.GetComponent<PetComponentServer>().GetPetInfo(contextId) != null;
            }

            return contextId == 0;
        }
    }
}
