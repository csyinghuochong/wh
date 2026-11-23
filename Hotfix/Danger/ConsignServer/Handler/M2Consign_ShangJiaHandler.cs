using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_ShangJiaHandler : AMActorRpcHandler<Scene, M2Consign_ShangJiaRequest, Consign2M_ShangJiaResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_ShangJiaRequest request, Consign2M_ShangJiaResponse response, Action reply)
        {
            ConsignSceneComponent consign = scene.GetComponent<ConsignSceneComponent>();
            using CoroutineLock consignLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Consign, consign.InstanceId);
            if (!consign.Initialized || request.ConsignItemInfo?.BagInfo == null
                || request.UnitID <= 0 || request.ConsignItemInfo.UserId != request.UnitID
                || !ItemNewHelper.CheckValiedItem(request.ConsignItemInfo.BagInfo))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            ConsignItemInfo consignItem = request.ConsignItemInfo;
            if (consignItem.BelongId <= 0 && consignItem.BagInfo != null)
            {
                consignItem.BelongId = ConsignHelper.GetConsignBelongId(consignItem.BagInfo);
            }

            DBConsignInfo dBPaiMainInfo = scene.GetComponent<ConsignSceneComponent>().GetOrCreatePaiMaiDBByBelongId(consignItem.BelongId);
            scene.GetComponent<ConsignSceneComponent>().InsertShangJiaItem(dBPaiMainInfo, consignItem);

            try
            {
                await consign.SavePaiMaiData(dBPaiMainInfo);
            }
            catch (Exception e)
            {
                dBPaiMainInfo.PaiMaiItemInfos.Remove(consignItem);
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                Log.Error(e);
                reply();
                return;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
