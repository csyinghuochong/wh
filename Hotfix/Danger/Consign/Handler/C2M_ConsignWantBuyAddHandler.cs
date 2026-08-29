using System;

namespace ET
{
    /// <summary>
    /// 发布求购
    /// </summary>
    [ActorMessageHandler]
    public class C2M_ConsignWantBuyAddHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignWantBuyAddRequest, M2C_ConsignWantBuyAddResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignWantBuyAddRequest request, M2C_ConsignWantBuyAddResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.WantBuy, unit.Id))
            {
                if (request.ItemId <= 0 || request.ItemNum <= 0 || request.Price <= 0)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                BagInfo checkItem = new BagInfo() { ItemType = request.ItemType, ItemID = request.ItemId, ItemNum = request.ItemNum };
                if (!ItemNewHelper.CheckValiedItem(checkItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                long allPrice = (long)request.Price * request.ItemNum;
                if (allPrice > 10000000 || allPrice <= 0)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold) < allPrice)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

                roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, (allPrice * -1).ToString(), true, ItemGetWay.PaiMaiBuy);

                ConsignWantBuyInfo wantBuy = new ConsignWantBuyInfo();
                wantBuy.Id = IdGenerater.Instance.GenerateId();
                wantBuy.UserId = roleInfoComponentServer.RoleInfo.UserId;
                wantBuy.PlayerName = roleInfoComponentServer.RoleInfo.Name;
                wantBuy.Account = roleInfoComponentServer.Account;
                wantBuy.ItemType = request.ItemType;
                wantBuy.ItemId = request.ItemId;
                wantBuy.ItemNum = request.ItemNum;
                wantBuy.Price = request.Price;
                wantBuy.Time = TimeHelper.ServerNow();

                long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_WantBuyAddResponse addResponse = (Consign2M_WantBuyAddResponse)await ActorMessageSenderComponent.Instance.Call(
                    paimaiServerId, new M2Consign_WantBuyAddRequest()
                    {
                        WantBuyInfo = wantBuy,
                    });

                if (addResponse.Error != ErrorCode.ERR_Success)
                {
                    roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, allPrice.ToString(), true, ItemGetWay.PaiMaiBuy);
                    response.Error = addResponse.Error;
                    reply();
                    return;
                }

                response.WantBuyInfo = wantBuy;
                reply();
                await ETTask.CompletedTask;
            }
        }
    }
}
