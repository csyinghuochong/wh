using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_BuyHandler : AMActorRpcHandler<Scene, M2Consign_BuyRequest, Consign2M_BuyResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_BuyRequest request, Consign2M_BuyResponse response, Action reply)
        {
            //获取列表,对应的缓存进行清空
            if (!ItemNewHelper.CheckValiedItem(request.ConsignItemInfo.BagInfo))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            int belongId = request.ConsignItemInfo.BelongId;
            if (belongId <= 0 && request.ConsignItemInfo.BagInfo != null)
            {
                belongId = ItemNewHelper.GetConsignBelongId(request.ConsignItemInfo.BagInfo);
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

            if (paiMaiItemInfo.OverTime > 0 && TimeHelper.ServerNow() >= paiMaiItemInfo.OverTime)
            {
                paiMaiItemInfos.Remove(paiMaiItemInfo);
                MailHelp.SendConsignOverTimeMail(paiMaiItemInfo).Coroutine();
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            
            if (request.BuyNum < 0 || request.BuyNum > paiMaiItemInfo.BagInfo.ItemNum)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            
            long needGold = (long)paiMaiItemInfo.Price * request.BuyNum;
            if (request.Gold < needGold)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }

            BagInfo bagInfo = paiMaiItemInfo.BagInfo;
            if (request.BuyNum == bagInfo.ItemNum)
            {
                response.ConsignItemInfo = paiMaiItemInfo;
                paiMaiItemInfos.Remove(paiMaiItemInfo);
            }
            else
            {
                bagInfo.ItemNum -= request.BuyNum;
                ConsignItemInfo paiMaiItemInfo2 = new ConsignItemInfo();
                
                BagInfo useBagInfo = new BagInfo();
                useBagInfo.ItemID = bagInfo.ItemID;
                useBagInfo.ItemNum = request.BuyNum;
                useBagInfo.Loc =(int)ItemLocType.ItemLocBag;
                useBagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
                useBagInfo.GetWay = bagInfo.GetWay;
                useBagInfo.SetBinding(bagInfo.IsBinding());
                
                paiMaiItemInfo2.Id = IdGenerater.Instance.GenerateId();
                paiMaiItemInfo2.BagInfo = useBagInfo;
                paiMaiItemInfo2.UserId = paiMaiItemInfo.UserId;
                paiMaiItemInfo2.Price = paiMaiItemInfo.Price;
                paiMaiItemInfo2.PlayerName = paiMaiItemInfo.PlayerName;
                paiMaiItemInfo2.SellTime = paiMaiItemInfo.SellTime;
                paiMaiItemInfo2.BelongId = paiMaiItemInfo.BelongId;
                paiMaiItemInfo2.Account = paiMaiItemInfo.Account;
                paiMaiItemInfo2.TargetPlayer = paiMaiItemInfo.TargetPlayer;
                paiMaiItemInfo2.OverTime = paiMaiItemInfo.OverTime;
                
                response.ConsignItemInfo = paiMaiItemInfo2;
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
