using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ConsignBuyHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignBuyRequest, M2C_ConsignBuyResponse>
    {
        //拍卖行购买道具
        protected override async ETTask Run(Unit unit, C2M_ConsignBuyRequest request, M2C_ConsignBuyResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            DataCollationComponent dataCollation = unit.GetComponent<DataCollationComponent>();

            //背包是否有位置
            if (bag.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

 
            ConsignItemInfo paiMaiItemInfo = request.ConsignItemInfo;
            if (request.ConsignItemInfo == null || request.ConsignItemInfo.BagInfo == null)
            {
                reply();
                return;
            }

            int pileNumber = ItemNewHelper.GetNewItemPileSum(paiMaiItemInfo.BagInfo);
            int cell = Mathf.CeilToInt(paiMaiItemInfo.BagInfo.ItemNum * 1f / pileNumber);
            if (bag.GetBagLeftCell() < cell)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            int buyNum = 0;
            if (request.BuyNum < 0 || request.BuyNum > paiMaiItemInfo.BagInfo.ItemNum)
            {
                Log.Error($"C2M_PaiMaiBuyRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            else if (request.BuyNum == 0)
            {
                buyNum = paiMaiItemInfo.BagInfo.ItemNum;
            }
            else
            {
                buyNum = request.BuyNum;
            }

            long needGold = (long)paiMaiItemInfo.Price * buyNum;
            if (paiMaiItemInfo.BagInfo.ItemNum < 0 || needGold < 0)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }

            //钱是否足够
            if (unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold) < needGold)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }


            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_BuyResponse r_GameStatusResponse = (Consign2M_BuyResponse)await ActorMessageSenderComponent.Instance.Call
                    (paimaiServerId, new M2Consign_BuyRequest()
                    {
                        ConsignItemInfo = request.ConsignItemInfo,
                        Gold = unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold),
                        BuyNum = buyNum
                    });
                if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = r_GameStatusResponse.Error;
                    reply();
                    return;
                }

                needGold = (long)r_GameStatusResponse.ConsignItemInfo.Price * r_GameStatusResponse.ConsignItemInfo.BagInfo.ItemNum;
               
                roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, (needGold * -1).ToString(), true, ItemGetWay.PaiMaiBuy);
                //背包添加道具
                bool ret = bag.OnAddItemData(r_GameStatusResponse.ConsignItemInfo.BagInfo, $"{ItemGetWay.PaiMaiBuy}_{TimeHelper.ServerNow()}");

                if (!ret)
                {
                    Log.Warning($"拍卖购买出错: {unit.Id} {bag.GetBagLeftCell()}  {paiMaiItemInfo.BagInfo.ItemID}  {paiMaiItemInfo.BagInfo.ItemNum}");
                }

                //给出售者邮件发送金币
                MailHelp.SendPaiMaiEmail(
                    UnitZoneHelper.GetHomeZone(r_GameStatusResponse.ConsignItemInfo.UserId),
                    r_GameStatusResponse.ConsignItemInfo,
                    r_GameStatusResponse.ConsignItemInfo.BagInfo.ItemNum,
                    r_GameStatusResponse.ConsignItemInfo.UserId).Coroutine();

                //Log.Warning($"拍卖购买者: {unit.Id} 购买 {r_GameStatusResponse.PaiMaiItemInfo.UserId} 道具ID：{r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");
                Log.Warning($"拍卖被购买: [出售者]{r_GameStatusResponse.ConsignItemInfo.UserId}  [购买者]{unit.Id} 道具ID：{r_GameStatusResponse.ConsignItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
