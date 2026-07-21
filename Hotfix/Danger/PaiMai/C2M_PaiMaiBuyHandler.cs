using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PaiMaiBuyHandler : AMActorLocationRpcHandler<Unit, C2M_PaiMaiBuyRequest, M2C_PaiMaiBuyResponse>
    {
        //拍卖行购买道具
        protected override async ETTask Run(Unit unit, C2M_PaiMaiBuyRequest request, M2C_PaiMaiBuyResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();

            //背包是否有位置
            if (bag.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            if (unit.Id == 2268423382062137344 && unit.DomainZone() == 32)
            {
                List<long> removeIds = new List<long>();    
                MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();

                if (mapComponent.MapTypeEnum == MapTypeEnum.BaoZangZhiDi)
                {
                    List<Unit> monsterid = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Monster);
                    for (int i = 0; i < monsterid.Count; i++)
                    {
                        NumericComponent numericComponent = monsterid[i].GetComponent<NumericComponent>();

                        if (numericComponent.GetAsInt(NumericType.Now_Dead) == 1
                            && (monsterid[i].ConfigId == 70005012 || monsterid[i].ConfigId == 70005013))
                        {
                            removeIds.Add(monsterid[i].Id);
                            Console.WriteLine($"umericType.Now_Dead: {monsterid[i].ConfigId}");
                        }
                    }
                }
                for (int i = 0; i < removeIds.Count; i++)
                {
                    unit.GetParent<UnitComponent>().Remove(removeIds[i]);
                }
            }

            PaiMaiItemInfo paiMaiItemInfo = request.PaiMaiItemInfo;
            if (request.PaiMaiItemInfo == null || request.PaiMaiItemInfo.BagInfo == null)
            {
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(paiMaiItemInfo.BagInfo.ItemID);
            int cell = Mathf.CeilToInt(paiMaiItemInfo.BagInfo.ItemNum * 1f / ldItem.ItemPileSum);
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

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            //钱是否足够
            if (roleInfoComponentServer.RoleInfo.Gold < needGold)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }

            bool firstDay = false;
            int openPaiMai = 0;//unit.GetComponent<NumericComponent>().GetAsInt(NumericType.PaiMaiOpen);

            if (openPaiMai == 0)
            {
                int createDay = roleInfoComponentServer.GetCrateDay();

                //firstDay = createDay <= 1 && roleInfoComponent.RoleInfo.Level <= 10;
                request.IsRecharge = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.RechargeNumber);

                if (request.IsRecharge > 0
                    || CommonHelper.IsCanPaiMai_KillBoss(roleInfoComponentServer.RoleInfo.MonsterRevives, roleInfoComponentServer.RoleInfo.Lv)
                    || CommonHelper.IsCanPaiMai_Level(createDay, roleInfoComponentServer.RoleInfo.Lv) == 0)
                {
                    openPaiMai = 1;
                    //unit.GetComponent<NumericComponent>().ApplyValue(NumericType.PaiMaiOpen, 1);
                }
            }

            if (!firstDay && openPaiMai == 0)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
                P2M_PaiMaiBuyResponse r_GameStatusResponse = (P2M_PaiMaiBuyResponse)await ActorMessageSenderComponent.Instance.Call
                    (paimaiServerId, new M2P_PaiMaiBuyRequest()
                    {
                        PaiMaiItemInfo = request.PaiMaiItemInfo,
                        Gold = roleInfoComponentServer.RoleInfo.Gold,
                        BuyNum = buyNum
                    });
                if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = r_GameStatusResponse.Error;
                    reply();
                    return;
                }

                needGold = (long)r_GameStatusResponse.PaiMaiItemInfo.Price * r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemNum;
               
                roleInfoComponentServer.UpdateRoleMoneySub(UserDataType.Gold, (needGold * -1).ToString(), true, ItemGetWay.PaiMaiBuy);
                //背包添加道具
                bool ret = bag.OnAddItemData(r_GameStatusResponse.PaiMaiItemInfo.BagInfo, $"{ItemGetWay.PaiMaiBuy}_{TimeHelper.ServerNow()}");

                if (!ret)
                {
                    Log.Warning($"拍卖购买出错: {unit.Id} {bag.GetBagLeftCell()}  {paiMaiItemInfo.BagInfo.ItemID}  {paiMaiItemInfo.BagInfo.ItemNum}");
                }

                //给出售者邮件发送金币
                MailHelp.SendPaiMaiEmail(UnitZoneHelper.GetHomeZone(unit), r_GameStatusResponse.PaiMaiItemInfo, r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemNum, unit.Id).Coroutine();

                //Log.Warning($"拍卖购买者: {unit.Id} 购买 {r_GameStatusResponse.PaiMaiItemInfo.UserId} 道具ID：{r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");
                Log.Warning($"拍卖被购买: [出售者]{r_GameStatusResponse.PaiMaiItemInfo.UserId}  [购买者]{unit.Id} 道具ID：{r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemID} 花费：{needGold} {ret}");

                DataCollationComponent dataCollation = unit.GetComponent<DataCollationComponent>();
                dataCollation.PaiMaiCostGoldToday += needGold;
                if (dataCollation.PaiMaiCostGoldToday >= 50000000)
                {
                    string levelInfo = $"区： {unit.DomainZone()}  {roleInfoComponentServer.RoleInfo.Name}   \t拍卖消耗金币:{dataCollation.PaiMaiCostGoldToday}  " +
                        $" \t账号:{roleInfoComponentServer.Account}   \t钻石:{roleInfoComponentServer.RoleInfo.Diamond}  \t金币:{roleInfoComponentServer.RoleInfo.Gold} \n";
                    LogHelper.PaiMaiInfo(levelInfo);
                }

                //long gateServerId = DBHelper.GetGateServerId(unit);
                //G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                //   (gateServerId, new T2G_GateUnitInfoRequest()
                //   {
                //       UserID = r_GameStatusResponse.PaiMaiItemInfo.UserId
                //   });
                //if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
                //{ 
                //}


                long baginfoid = 0;
                if (LDItemCategory.Instance.Get(r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemID).ItemType == ItemTypeEnum.Equipment)
                {
                    baginfoid = r_GameStatusResponse.PaiMaiItemInfo.BagInfo.BagInfoID;
                }


                if (unit.Id != r_GameStatusResponse.PaiMaiItemInfo.UserId)
                {
                    long locationactor = r_GameStatusResponse.PaiMaiItemInfo.UserId;

                    M2M_PaiMaiBuyInfoRequest r2M_RechargeRequest = new M2M_PaiMaiBuyInfoRequest() { PlayerId = unit.Id, BagInfoID = baginfoid,  CostGold = (long)(needGold * 0.95f) };
                    M2M_PaiMaiBuyInfoResponse m2G_RechargeResponse = (M2M_PaiMaiBuyInfoResponse)await MessageHelper.CallLocationActor(locationactor, r2M_RechargeRequest);

                    if (m2G_RechargeResponse.Error != ErrorCode.ERR_Success)
                    {
                        DataCollationComponent dataCollationComponent = await DBHelper.GetComponentCache<DataCollationComponent>(UnitZoneHelper.GetHomeZone(r_GameStatusResponse.PaiMaiItemInfo.UserId), r_GameStatusResponse.PaiMaiItemInfo.UserId);
                        if (dataCollationComponent != null)
                        {
                            dataCollationComponent.UpdateBuySelfPlayerList((long)(needGold * 0.95f), unit.Id, baginfoid, false);
                            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(r_GameStatusResponse.PaiMaiItemInfo.UserId), r_GameStatusResponse.PaiMaiItemInfo.UserId, dataCollationComponent).Coroutine();
                        }
                        
                    }
                }
                else
                {
                    DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
                    NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                    dataCollationComponent.UpdateBuySelfPlayerList((long)(needGold * 0.95f), unit.Id, baginfoid, true);
                }
                
                //每天更新文本。
                //今天拍卖出售获取金币数量>=50000000  打印出来
                //充值《100 金币大于5亿
                if (needGold >= 500000)
                {
                    //服务器 道具名称 数量  价格  购买者名称 购买者等级  购买者充值 购买者当前金币 购买者账号 出售者名称   出售者账号  出售者等级 出售者当前金币
                    string serverName = ServerHelper.GetGetServerItem(false, UnitZoneHelper.GetHomeZone(unit)).ServerName;
                    string itemName = WordHelper.GetShowText(ldItem.Name, 0);
                    int itemNumber = r_GameStatusResponse.PaiMaiItemInfo.BagInfo.ItemNum;
                    long price = r_GameStatusResponse.PaiMaiItemInfo.Price;

                    string buyPlayerName = roleInfoComponentServer.RoleInfo.Name;
                    int buyPlayerLv = roleInfoComponentServer.RoleInfo.Lv;
                    int buyPlayerRecharge = request.IsRecharge;
                    long buyNowGold = roleInfoComponentServer.RoleInfo.Gold;
                    string buyAccount = roleInfoComponentServer.Account;
                    
                    string sellPlayerName = r_GameStatusResponse.PaiMaiItemInfo.PlayerName;
                    string sellAccoount = r_GameStatusResponse.PaiMaiItemInfo.Account;
                    RoleInfoComponentServer roleInfoComponentServerSell = await DBHelper.GetComponentCache<RoleInfoComponentServer>(UnitZoneHelper.GetHomeZone(r_GameStatusResponse.PaiMaiItemInfo.UserId), r_GameStatusResponse.PaiMaiItemInfo.UserId);
                    if (roleInfoComponentServerSell != null)
                    {
                        int sellPlayerLv = roleInfoComponentServerSell.RoleInfo.Lv;
                        long sellNowGold = roleInfoComponentServerSell.RoleInfo.Gold;

                        string paimaiInfo = $"服务器:{serverName}   \t道具名称:{itemName}   \t数量:{itemNumber}   \t价格:{price}  \t购买者名称:{buyPlayerName}   \t购买者等级:{buyPlayerLv}    " +
                            $"\t购买者充值:{buyPlayerRecharge}   \t购买者当前金币:{buyNowGold}   \t购买者账号:{buyAccount}    \t出售者名称:{sellPlayerName}   \t出售者账号:{sellAccoount}   \t出售者等级:{sellPlayerLv}    \t出售者当前金币:{sellNowGold} ";
                        LogHelper.PaiMaiInfo(paimaiInfo);
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
