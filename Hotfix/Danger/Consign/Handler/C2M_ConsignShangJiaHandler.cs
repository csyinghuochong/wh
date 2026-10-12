using System;
using System.Collections.Generic;

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

                request.ConsignItemInfo.TargetUserId = 0;
                if (!string.IsNullOrEmpty(request.ConsignItemInfo.TargetPlayer))
                {
                    int homeZone = UnitZoneHelper.GetHomeZone(unit);
                    string targetName = request.ConsignItemInfo.TargetPlayer;
                    List<RoleInfoComponentServer> targetRoles = await Game.Scene.GetComponent<DBComponent>()
                            .Query<RoleInfoComponentServer>(homeZone, d => d.RoleInfo.Name == targetName);
                    if (targetRoles == null || targetRoles.Count == 0)
                    {
                        response.Error = ErrorCode.ERR_NonePlayerError;
                        reply();
                        return;
                    }

                    long targetUserId = targetRoles[0].RoleInfo.UserId;
                    if (targetUserId <= 0)
                    {
                        targetUserId = targetRoles[0].Id;
                    }

                    if (targetUserId == roleInfoComponentServer.RoleInfo.UserId)
                    {
                        response.Error = ErrorCode.ERR_Parameter;
                        reply();
                        return;
                    }

                    request.ConsignItemInfo.TargetUserId = targetUserId;
                }

                int days = ConsignHelper.NormalizeConsignDays((int)request.ConsignItemInfo.OverTime);
                request.ConsignItemInfo.OverTime = days;

                request.ConsignItemInfo.BelongId = ConsignHelper.GetConsignBelongId(request.ConsignItemInfo.BagInfo);
                if (request.ConsignItemInfo.BelongId <= 0)
                {
                    response.Error = ErrorCode.Err_ConsignNoSellType;
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

				//上架需要需要的金币
                int serviceFee = ConsignHelper.GetConsignServiceFee(days);
                if (serviceFee > 0
                    && bagComponentServer.GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold) < serviceFee)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

				int sellNum = request.ConsignItemInfo.BagInfo.ItemNum;
				int sellItemId = bagInfo.ItemID;
				if (!bagComponentServer.OnCostItemData(bagInfoId, sellNum))
				{
					response.Error = ErrorCode.ERR_ItemNotEnoughError;
					reply();
					return;
				}

				if (serviceFee > 0)
				{
					roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, (-serviceFee).ToString(), true, ItemGetWay.PaiMaiSell);
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

				if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
				{
					BagInfo current = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoId);
					M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
					if (current != null)
					{
						current.ItemNum += sellNum;
						bagUpdate.BagInfoUpdate.Add(current);
					}
					else
					{
						bagInfo.ItemNum = sellNum;
						bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag).Add(bagInfo);
						bagUpdate.BagInfoAdd.Add(bagInfo);
					}

					MessageHelper.SendToClient(unit, bagUpdate);
					if (serviceFee > 0)
					{
						roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, serviceFee.ToString(), true, ItemGetWay.PaiMaiSell);
					}

					response.Error = r_GameStatusResponse.Error;
					reply();
					return;
				}

				unit.GetComponent<TaskComponentServer>().OnConsignShangJia();
				response.ConsignItemInfo = request.ConsignItemInfo;
				LogHelper.LogWarning(response.ConsignItemInfo.PlayerName + "上架道具：" + sellItemId + "数量" + sellNum + "时间戳:" + currentTime.ToString(), true);
                response.Error = r_GameStatusResponse.Error;
				reply();
				await ETTask.CompletedTask;
			}
		}
	}
}
