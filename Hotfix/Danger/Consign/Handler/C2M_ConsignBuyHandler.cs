using System;

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

 
            ConsignItemInfo paiMaiItemInfo = request.ConsignItemInfo;
            if (request.ConsignItemInfo == null || request.ConsignItemInfo.BagInfo == null)
            {
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

            BagInfo item = paiMaiItemInfo.BagInfo;
            ItemLocType loc = ItemNewHelper.GetToItemLocType(item.ItemType, item.ItemID);
            if (!ItemNewHelper.IsValideBagLoc((int)loc))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            int cell = ItemNewHelper.CalcNeedNewCells(bag.GetItemByLoc(loc), item.ItemType, item.ItemID, buyNum,
                    ItemNewHelper.GetNewItemPileSum(item), item.ItemFlags, item.MakePlayer, item.MakeTime);
            if (bag.GetBagLeftCell((int)loc) < cell)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                reply();
                return;
            }

            long needGold = (long)paiMaiItemInfo.Price * buyNum;
            if (paiMaiItemInfo.BagInfo.ItemNum < 0 || needGold < 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            //钱是否足够
            if (unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold) < needGold)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
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
                        BuyNum = buyNum,
                        UnitId = unit.Id,
                    });
                if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = r_GameStatusResponse.Error;
                    reply();
                    return;
                }

                needGold = (long)r_GameStatusResponse.ConsignItemInfo.Price * r_GameStatusResponse.ConsignItemInfo.BagInfo.ItemNum;
               
                roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, (needGold * -1).ToString(), true, ItemGetWay.PaiMaiBuy);
                bool ret = bag.OnAddItemData(r_GameStatusResponse.ConsignItemInfo.BagInfo, $"{ItemGetWay.PaiMaiBuy}_{TimeHelper.ServerNow()}");
                if (!ret)
                {
                    Log.Warning($"拍卖购买出错: {unit.Id} {bag.GetBagLeftCell()}  {paiMaiItemInfo.BagInfo.ItemID}  {paiMaiItemInfo.BagInfo.ItemNum}");
                }

                unit.GetComponent<TaskComponentServer>().OnConsignBuy(r_GameStatusResponse.ConsignItemInfo.BagInfo);

                // 这里只发送成交通知；收益发放预留在 AddSaleIncome，待个人仓库接入。
                MailHelp.SendPaiMaiEmail(r_GameStatusResponse.ConsignItemInfo).Coroutine();

                //Log.Warning($"拍卖购买者: {unit.Id} 购买 {r_GameStatusResponse.PaiMaiItemInfo.UserId} 道具ID：{r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");
                Log.Warning($"拍卖被购买: [出售者]{r_GameStatusResponse.ConsignItemInfo.UserId}  [购买者]{unit.Id} 道具ID：{r_GameStatusResponse.ConsignItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
