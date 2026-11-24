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
        /// <summary>校验启动、顺序推进和配置恢复；重复/滞后请求返回已保存进度，不倒退、不报错；已完成组重复请求直接成功。</summary>
        public static int Update(Unit unit, C2M_GuideUpdateRequest request, M2C_GuideUpdateResponse response)
        {
            if (!BeginnerGuideConfigHelper.TryGetSteps(request.GuideId, out List<LDGuide> steps, out string reason))
            {
                return Reject(request, response, $"配置无效：{reason}");
            }
            if (request.GuideStep < 0 || request.ContextId < 0)
            {
                return Reject(request, response, "步骤ID或目标实例ID不能为负数");
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
                if (request.GuideStep != request.GuideId)
                {
                    return Reject(request, response, "服务端尚无进度，必须从组首开始");
                }
                if (!CheckTrigger(unit, steps[0]))
                {
                    return Reject(request, response, $"服务端触发条件未满足，类型{steps[0].Trigger_Condition}，参数{string.Join("|", steps[0].Trigger_Condition_Param)}");
                }
                if (!CheckContext(unit, steps, request.ContextId))
                {
                    return Reject(request, response, "目标装备或灵宠不存在、不属于当前角色，或实例参数不符合本组要求");
                }

                progress = new BeginnerGuideProgress { GuideId = request.GuideId, StepId = request.GuideStep, ContextId = request.ContextId };
                role.BeginnerGuideProgresses.Add(progress);
                response.Progress = progress;
                return ErrorCode.ERR_Success;
            }

            // 这组已绑定实例时以服务端为准；不同实例的迟到请求只同步原进度，不能换绑或推进。
            if (request.ContextId != progress.ContextId && steps.Exists(x => x.Id == progress.StepId))
            {
                response.Progress = progress;
                return ErrorCode.ERR_Success;
            }
            if (request.ContextId != progress.ContextId)
            {
                return Reject(request, response, $"实例进度不一致，服务端实例{progress.ContextId}，请求实例{request.ContextId}");
            }

            LDGuide current = steps.Find(x => x.Id == progress.StepId);
            if (current == null)
            {
                // 只接受已关闭旧步骤显式配置的本组回溯目标，仍核对触发资格与实例归属。
                LDGuideCategory category = LDGuideCategory.Instance;
                if (!category.Contain(progress.StepId))
                {
                    return Reject(request, response, $"服务端保存的步骤{progress.StepId}已从配置表删除");
                }

                LDGuide retiredStep = category.Get(progress.StepId);
                current = steps.Find(x => x.Id == retiredStep.Break_Backup_Id);
                if (retiredStep.Is_Close == 0 || current == null || request.GuideStep != current.Id
                    || !CheckTrigger(unit, steps[0]) || !CheckContext(unit, steps, request.ContextId))
                {
                    return Reject(request, response, $"旧步骤{progress.StepId}无法恢复到请求步骤，请核对Is_Close、Break_Backup_Id、触发条件及目标实例");
                }
            }

            if (request.GuideStep == 0)
            {
                // 尚未到末步时同步实际进度，不接受提前完成，也不让客户端反复保存失败。
                if (current.Next_Id.Length != 0)
                {
                    response.Progress = progress;
                    return ErrorCode.ERR_Success;
                }

                role.BeginnerGuideProgresses.Remove(progress);
                role.CompleteGuideIds.Add(request.GuideId);
                response.Completed = true;
                return ErrorCode.ERR_Success;
            }

            bool next = current.Next_Id.Length == 1 && current.Next_Id[0] == request.GuideStep;
            // 除当前步和Next_Id外，只允许跳到当前步显式配置的中断节点；同组范围已由TryGetSteps校验。
            if (request.GuideStep != current.Id && request.GuideStep != current.Break_Backup_Id && !next)
            {
                // 回包丢失或重连可能带来旧请求；只返回已保存步骤，明确配置的回溯仍在上面的正常分支执行。
                int requestedIndex = steps.FindIndex(x => x.Id == request.GuideStep);
                if (requestedIndex >= 0 && requestedIndex < steps.IndexOf(current))
                {
                    response.Progress = progress;
                    return ErrorCode.ERR_Success;
                }
                return Reject(request, response, $"步骤不一致，服务端当前{current.Id}，下一步{string.Join("|", current.Next_Id)}，中断节点{current.Break_Backup_Id}");
            }

            progress.StepId = request.GuideStep;
            response.Progress = progress;
            return ErrorCode.ERR_Success;
        }

        /// <summary>保留原错误码，通过现有Message返回拒绝原因；不修改角色进度。</summary>
        private static int Reject(C2M_GuideUpdateRequest request, M2C_GuideUpdateResponse response, string reason)
        {
            response.Message = $"新手引导{request.GuideId}（请求步骤{request.GuideStep}）：{reason}。";
            return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
        }

        /// <summary>新建进度时校验服务器可确认的触发条件；装备事件还需通过实例归属校验。</summary>
        private static bool CheckTrigger(Unit unit, LDGuide root)
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

        /// <summary>装备或指定灵宠必须属于当前角色；灵宠上下文0表示默认界面第一只，需至少拥有一只；其他组只接受0。</summary>
        private static bool CheckContext(Unit unit, List<LDGuide> steps, long contextId)
        {
            if (BeginnerGuideConfigHelper.ContainsPosition(steps, BeginnerGuidePosition.Equipment_2))
            {
                BagInfo item = unit.GetComponent<BagComponentServer>().GetItemByUId(contextId);
                return item != null && item.ItemType == ItemBigType.Type_Equip
                    && (item.Loc == (int)ItemLocType.ItemLocBag || item.Loc == (int)ItemLocType.ItemLocEquip);
            }
            if (BeginnerGuideConfigHelper.ContainsPosition(steps, BeginnerGuidePosition.PetFight_12))
            {
                PetComponentServer pets = unit.GetComponent<PetComponentServer>();
                return contextId == 0 ? pets.PetInfos.Count > 0 : pets.GetPetInfo(contextId) != null;
            }

            return contextId == 0;
        }
    }
}
