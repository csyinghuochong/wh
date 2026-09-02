using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ConsignShangJiaHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignShangJiaRequest, M2C_ConsignShangJiaResponse>
    {

		protected override async ETTask Run(Unit unit, C2M_ConsignShangJiaRequest request, M2C_ConsignShangJiaResponse response, Action reply)
		{
           
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Sell, unit.Id))
			{
				if (request.ConsignItemInfo.BagInfo == null 
					|| request.ConsignItemInfo.BagInfo.ItemNum <= 0)
                {
                    Log.Error($"C2M_PaiMaiSellRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
					return;
				}
				long allprice = request.ConsignItemInfo.BagInfo.ItemNum * request.ConsignItemInfo.Price;
                if (allprice > 10000000 || allprice < 0)
                {
                    Log.Error($"C2M_PaiMaiSellRequest 2");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }


                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

                //获取时间戳
                long currentTime = TimeHelper.ServerNow();

                //获取出售数据
                long paimaiItemId = IdGenerater.Instance.GenerateId();
				request.ConsignItemInfo.Id = paimaiItemId;

                request.ConsignItemInfo.PlayerName = roleInfoComponentServer.RoleInfo.Name;
				request.ConsignItemInfo.UserId = roleInfoComponentServer.RoleInfo.UserId;
                request.ConsignItemInfo.Account = roleInfoComponentServer.Account;
                request.ConsignItemInfo.SellTime = currentTime;
                if (string.IsNullOrEmpty(request.ConsignItemInfo.TargetPlayer)
                    && !string.IsNullOrEmpty(request.TargetPlayer))
                {
                    request.ConsignItemInfo.TargetPlayer = request.TargetPlayer;
                }

                int days = ConsignHelper.NormalizeConsignDays((int)request.ConsignItemInfo.OverTime);
                request.ConsignItemInfo.OverTime = days;

                request.ConsignItemInfo.BelongId = ConsignHelper.GetConsignBelongId(request.ConsignItemInfo.BagInfo);
                if (request.ConsignItemInfo.BelongId <= 0)
                {
                    response.Error = ErrorCode.ERR_Parameter;
                    reply();
                    return;
                }

				//对比出售数量和道具是否匹配
				long bagInfoId = request.ConsignItemInfo.BagInfo.BagInfoID;
				BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoId);
				if (bagInfo == null)
				{
					response.Error = ErrorCode.ERR_ItemNotEnoughError;      //道具不足
					reply();
					return;
				}
				if (bagInfo.ItemNum < request.ConsignItemInfo.BagInfo.ItemNum)
				{
					response.Error = ErrorCode.ERR_ItemNotEnoughError;      //道具不足
					reply();
					return;
				}

				if (bagInfo.IsBinding())
				{
					response.Error = ErrorCode.ERR_ItemBing;      //道具绑定
					reply();
					return;
				}

				if (allprice < 0)
                {
                    Log.Error($"C2M_PaiMaiSellRequest 3");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
					return;
				}

				//发送对应拍卖行信息
				long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
				Consign2M_ShangJiaResponse r_GameStatusResponse = (Consign2M_ShangJiaResponse)await ActorMessageSenderComponent.Instance.Call
					(paimaiServerId, new M2Consign_ShangJiaRequest()
					{
						UnitID = unit.Id,
						ConsignItemInfo = request.ConsignItemInfo,
						PaiMaiTodayGold = 0,
					});

				if (r_GameStatusResponse.Error == ErrorCode.ERR_Success)
				{
					//扣除对应道具
					bagComponentServer.OnCostItemData(request.ConsignItemInfo.BagInfo.BagInfoID, request.ConsignItemInfo.BagInfo.ItemNum);
					response.ConsignItemInfo = request.ConsignItemInfo;
					LogHelper.LogWarning(response.ConsignItemInfo.PlayerName + "上架道具：" + request.ConsignItemInfo.BagInfo.ItemID + "数量" + request.ConsignItemInfo.BagInfo.ItemNum + "时间戳:" + currentTime.ToString(), true);
                }
                response.Error = r_GameStatusResponse.Error;
				reply();
				await ETTask.CompletedTask;
			}
		}
	}
}
