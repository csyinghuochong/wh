using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPetHandler : AMActorRpcHandler<Scene, C2F_WatchPetRequest, F2C_WatchPetResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPetRequest request, F2C_WatchPetResponse response, Action reply)
        {
            int homeZone = FriendHelper.PrepareHomeZone(request.UnitID);
            if (homeZone == 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            PetComponentServer petComponentServer = await DBHelper.GetComponent<PetComponentServer>(homeZone, request.UnitID);
            if (petComponentServer == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            BagComponentServer bagComponentsServer = await DBHelper.GetComponent<BagComponentServer>(homeZone, request.UnitID);
            if (bagComponentsServer == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            response.PetInfos = petComponentServer.GetPetInfo( request.PetId );
            
            NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(homeZone, request.UnitID);
            if (numericComponent == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            foreach ((int key, long value) in numericComponent.NumericDic)
            {
                if (key >= (int)NumericType.Max)
                {
                    continue;
                }
                response.Ks.Add(key);
                response.Vs.Add(value);
            }
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
