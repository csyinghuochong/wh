using System;

namespace ET
{
    [ActorMessageHandler]
    public class ObjectUnLockRequestHandler: AMActorRpcHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response, Action reply)
        {
            if (!scene.GetComponent<LocationComponent>().UnLock(request.Key, request.OldInstanceId, request.InstanceId))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            reply();

            await ETTask.CompletedTask;
        }
    }
}
