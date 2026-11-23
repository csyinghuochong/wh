using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2S_SoloEnterHandler : AMActorRpcHandler<Scene, M2S_SoloEnterRequest, S2M_SoloEnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2S_SoloEnterRequest request, S2M_SoloEnterResponse response, Action reply)
        {
            Scene soloscene =  scene.GetComponent<SoloSceneComponent>().GetChild<Scene>(request.FubenId);
            if (soloscene == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            if (soloscene.GetComponent<SoloDungeonComponent>().SendReward)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            response.FubenInstanceId = soloscene.InstanceId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
