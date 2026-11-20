using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_BuyHandler : AMActorRpcHandler<Scene, M2Consign_BuyRequest, Consign2M_BuyResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_BuyRequest request, Consign2M_BuyResponse response, Action reply)
        {
            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            using CoroutineLock consignLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Consign, consignScene.InstanceId);
            //获取列表,对应的缓存进行清空
            if (!consignScene.Initialized || request.UnitId <= 0 || request.ConsignItemInfo?.BagInfo == null
                || !ItemNewHelper.CheckValiedItem(request.ConsignItemInfo.BagInfo))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            int belongId = request.ConsignItemInfo.BelongId;
            if (belongId <= 0 && request.ConsignItemInfo.BagInfo != null)
            {
                belongId = ConsignHelper.GetConsignBelongId(request.ConsignItemInfo.BagInfo);
            }

            DBConsignInfo dBPaiMainInfo = consignScene.GetPaiMaiDBByBelongId(belongId);
            List<ConsignItemInfo> paiMaiItemInfos = dBPaiMainInfo?.PaiMaiItemInfos;
            ConsignItemInfo paiMaiItemInfo = consignScene.FindShangJiaItemInList(paiMaiItemInfos, request.ConsignItemInfo.Id);
            if (paiMaiItemInfo == null)
            {
                paiMaiItemInfo = consignScene.FindShangJiaItem(0, request.ConsignItemInfo.Id, out paiMaiItemInfos);
            }

            if (paiMaiItemInfo == null || paiMaiItemInfos == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            if (ConsignHelper.IsConsignExpired(paiMaiItemInfo))
            {
                DBConsignInfo expiredDB = consignScene.GetPaiMaiDBByBelongId(paiMaiItemInfo.BelongId);
                ConsignStorageHelper.MoveExpiredItems(expiredDB);
                await consignScene.SavePaiMaiData(expiredDB);
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            
            if (request.BuyNum <= 0 || request.BuyNum > paiMaiItemInfo.BagInfo.ItemNum
                || request.UnitId == paiMaiItemInfo.UserId
                || (ConsignHelper.IsDesignatedShangJia(paiMaiItemInfo) && paiMaiItemInfo.TargetUserId != request.UnitId))
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            
            long needGold = (long)paiMaiItemInfo.Price * request.BuyNum;
            if (paiMaiItemInfo.Price < 0 || needGold > int.MaxValue)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            if (request.Gold < needGold)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            BagInfo bagInfo = paiMaiItemInfo.BagInfo;
            int originalNum = bagInfo.ItemNum;
            int originalIndex = paiMaiItemInfos.IndexOf(paiMaiItemInfo);
            if (request.BuyNum == bagInfo.ItemNum)
            {
                response.ConsignItemInfo = paiMaiItemInfo;
                paiMaiItemInfos.Remove(paiMaiItemInfo);
            }
            else
            {
                bagInfo.ItemNum -= request.BuyNum;
                ConsignItemInfo sold = CloneHelper.DeepClone(paiMaiItemInfo);
                sold.BagInfo.ItemNum = request.BuyNum;
                sold.BagInfo.Loc = (int)ItemLocType.ItemLocBag;
                sold.BagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
                sold.Id = IdGenerater.Instance.GenerateId();
                response.ConsignItemInfo = sold;
            }
            DBConsignInfo soldDB = consignScene.GetPaiMaiDBByBelongId(paiMaiItemInfo.BelongId);
            try
            {
                await consignScene.SavePaiMaiData(soldDB);
            }
            catch (Exception e)
            {
                bagInfo.ItemNum = originalNum;
                if (!paiMaiItemInfos.Contains(paiMaiItemInfo))
                {
                    paiMaiItemInfos.Insert(originalIndex, paiMaiItemInfo);
                }

                response.ConsignItemInfo = null;
                response.Error = ErrorCode.ERR_Error;
                Log.Error(e);
                reply();
                return;
            }
            ConsignStorageHelper.AddSaleIncome(response.ConsignItemInfo);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
