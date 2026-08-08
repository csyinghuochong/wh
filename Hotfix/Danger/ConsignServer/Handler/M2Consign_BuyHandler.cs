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
            LDItem ldItemCof = LDItemCategory.Instance.Get(request.ConsignItemInfo.BagInfo.ItemID);
            int itemType = ldItemCof.ItemType;
            DBConsignInfo dBPaiMainInfo = scene.GetComponent<ConsignSceneComponent>().GetPaiMaiDBByType(itemType);
            if (dBPaiMainInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            long needGold = 0 ;
            ConsignItemInfo paiMaiItemInfo = null;
            List<ConsignItemInfo> paiMaiItemInfos = dBPaiMainInfo.PaiMaiItemInfos;
            for (int i = paiMaiItemInfos.Count - 1; i >= 0; i--)
            {
                if (paiMaiItemInfos[i].Id == request.ConsignItemInfo.Id)
                {
                    paiMaiItemInfo = paiMaiItemInfos[i];
                    break;
                }
            }

            if (paiMaiItemInfo == null)
            {
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
            
            needGold = (long)paiMaiItemInfo.Price * request.BuyNum;
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
                
                response.ConsignItemInfo = paiMaiItemInfo2;
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
