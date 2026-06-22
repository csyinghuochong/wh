using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_RechargeHandler : AMActorLocationRpcHandler<Unit, C2M_RechargeRequest, M2C_RechargeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RechargeRequest request, M2C_RechargeResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Recharge, unit.Id))
            {
                long dbCacheId = DBHelper.GetRealmCenter();

                if (!LDPayCategory.Instance.Contain(request.PayID))
                {
                    reply();
                    return;
                }

                if (CommonHelper.IsBanHaoZone(unit.DomainZone()))
                {
                    LogHelper.LogWarning($"充值[版号服]SendDiamondToUnit: {unit.Id}");
                    Console.WriteLine($"充值[版号服]SendDiamondToUnit: {unit.Id}");
                    RechargeHelp.SendDiamondToUnit(unit, request.PayID, "版号服", 0);
                    reply();
                    return;
                }
                //if (ComHelp.IsInnerNet())
                //{
                //    //RechargeHelp.SendDiamondToUnit(unit, request.RechargeNumber, "内测服");
                //    reply();
                //    return;
                //}

             
                string serverName = ServerHelper.GetGetServerItem(false, unit.DomainZone()).ServerName;
                UserInfoComponent userInfoComponent = unit.GetComponent<RoleInfoComponent>();
                string userName = userInfoComponent.UserInfo.Name;

                if (request.PayType == PayTypeEnum.IOSPay)
                {
                    ///IOS仅用来打印日志
                    Log.Warning($"支付订单[IOS]拉起: 服务器:{serverName} 玩家:{userName}  充值金额:{request.PayID}");
                    Log.Console($"支付订单[IOS]拉起: 服务器:{serverName} 玩家:{userName}  充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                    reply();
                    return;
                }
                
                if (request.PayType == PayTypeEnum.Google)
                {
                    Log.Warning($"支付订单[Google]拉起: 服务器:{serverName} 玩家:{userName}  充值金额:{request.PayID}");
                    Log.Console($"支付订单[Google]拉起: 服务器:{serverName} 玩家:{userName}  充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                    reply();
                    return;
                }

                if (request.PayType == PayTypeEnum.WeiXinPay)
                {
                    Log.Warning($"支付订单[微信支付]拉起:服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}");
                    Log.Console($"支付订单[微信支付]拉起:服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                }

                if (request.PayType == PayTypeEnum.AliPay)
                {
                    Log.Warning($"支付订单[支付宝]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}");
                    Log.Console($"支付订单[支付宝]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                }

                if (request.PayType == PayTypeEnum.TikTok)
                {
                    Log.Warning($"支付订单[TikTok]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}");
                    Log.Console($"支付订单[TikTok]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                }

                if (request.PayType == PayTypeEnum.QuDaoPay)
                {
                    Log.Warning($"支付订单[QuDaoPay]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}");
                    Log.Console($"支付订单[QuDaoPay]拉起: 服务器:{serverName} 玩家:{userName}   充值金额:{request.PayID}  时间:{TimeHelper.DateTimeNow().ToString()}");
                }

                long rechareId = DBHelper.GetRechargeCenter();
              
                R2M_RechargeResponse r2M_RechargeResponse = (R2M_RechargeResponse)await ActorMessageSenderComponent.Instance.Call(rechareId, new M2R_RechargeRequest()
                {
                    Zone = unit.DomainZone(),
                    PayType = request.PayType,
                    UnitId = unit.Id,
                    UnitName = userName,
                    RechargeNumber = request.PayID,
                    Account = userInfoComponent.Account,
                    payMessage = request.RiskControlInfo,
                    ClientIp = userInfoComponent.RemoteAddress,
                    RechargeType = request.RechargeType,    
                });

                response.Message = r2M_RechargeResponse.Message;
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
