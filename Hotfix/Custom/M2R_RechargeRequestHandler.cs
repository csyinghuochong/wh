using MongoDB.Driver.Linq;
using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2R_RechargeRequestHandler : AMActorRpcHandler<Scene, M2R_RechargeRequest, R2M_RechargeResponse>
    {
        protected override async ETTask Run(Scene scene, M2R_RechargeRequest request, R2M_RechargeResponse response, Action reply)
        {
            switch (request.PayType)
            {
                case PayWayEnum.WeiXinPay:
                    response.Message = await scene.GetComponent<ReChargeWXComponent>().WeChatPay(request);
                    break;
                case PayWayEnum.AliPay:
                    response.Message =  scene.GetComponent<ReChargeAliComponent>().AliPay(request);
                    break;
                case PayWayEnum.QuDaoPay:
                    response.Message = scene.GetComponent<ReChargeQDComponent>().QudaoPay(request);
                    break;
                case PayWayEnum.IOSPay:
                    response.Error = await scene.GetComponent<ReChargeIOSComponent>().OnIOSPayVerify(request);
                    break;
                case PayWayEnum.Google:
                    Console.WriteLine($"C2R_GooglePayVerifyRequest C2R_GooglePayVerifyRequest yyy {TimeInfo.Instance.ToDateTime(TimeHelper.ServerNow())}");

                    response.Error = await scene.GetComponent<ReChargeGoogleComponent>().OnGooglePayVerify2(request);
                    break;
                case PayWayEnum.TikTok:
                    response.Message = scene.GetComponent<ReChargeTikTokComponent>().TikTokPay(request);
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
