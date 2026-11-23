using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionRenameHandler : AMActorRpcHandler<Scene, M2U_UnionRenameRequest, U2M_UnionRenameResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionRenameRequest request, U2M_UnionRenameResponse response, Action reply)
        {
            // 与创建共用名称锁，避免创建与改名同时通过重名校验。
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionCreate, scene.DomainZone()))
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                UnionSceneComponent component = scene.GetComponent<UnionSceneComponent>();
                await component.LoadAllUnionInfos();
                DBUnionInfo data = await component.GetDBUnionInfo(request.UnionId);
                if (data?.UnionInfo == null)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                if (data.UnionInfo.LeaderId != request.UnitId)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                if (!UnionHelper.IsValidUnionName(request.NewName))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                foreach (DBUnionInfo exist in component.DBUnionInfos.Values)
                {
                    if (exist.UnionInfo.UnionName == request.NewName)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                        reply();
                        return;
                    }
                }

                if (request.Preview)
                {
                    reply();
                    return;
                }

                data.UnionInfo.UnionName = request.NewName;
                await DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, data);
                response.UnionName = data.UnionInfo.UnionName;
                reply();
                ServerMessageHelper.NoticeUnionName(data, request.NewName).Coroutine();
            }
        }
    }
}
