using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_VipGiftBuyHandler : AMActorLocationRpcHandler<Unit, C2M_VipGiftBuyRequest, M2C_VipGiftBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_VipGiftBuyRequest request, M2C_VipGiftBuyResponse response, Action reply)
        {
            int vipLevel = request.VipLevel;
            RechargePro pro = VipHelp.GetPro(unit);
            LDVIP cfg = VipHelp.GetConfig(vipLevel);
            if (pro == null || cfg == null || pro.VipLevel < vipLevel)
            {
                response.Error = ErrorCode.ERR_VipLevelNotEnough;
                reply();
                return;
            }

            if (VipHelp.HasGiftBought(pro, vipLevel))
            {
                response.Error = ErrorCode.ERR_VipGiftBought;
                reply();
                return;
            }

            int error = VipHelp.TryBuyGift(unit, cfg);
            if (error != ErrorCode.ERR_Success)
            {
                response.Error = error;
                reply();
                return;
            }

            VipHelp.AddGiftBought(pro, vipLevel);
            unit.GetComponent<RechargeComponentServer>()?.NotifyClient();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
