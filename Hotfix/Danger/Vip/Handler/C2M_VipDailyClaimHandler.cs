using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_VipDailyClaimHandler : AMActorLocationRpcHandler<Unit, C2M_VipDailyClaimRequest, M2C_VipDailyClaimResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_VipDailyClaimRequest request, M2C_VipDailyClaimResponse response, Action reply)
        {
            RechargePro pro = VipHelp.GetPro(unit);
            LDVIP cfg = pro == null ? null : VipHelp.GetConfig(pro.VipLevel);
            if (cfg == null)
            {
                response.Error = ErrorCode.ERR_VipLevelNotEnough;
                reply();
                return;
            }

            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            if (daily == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }

            if (daily.GetVipDailyClaimed() == 1)
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }

            int error = VipHelp.TryAddReward(unit, cfg.Reward_Daily, ItemGetWay.VipDaily);
            if (error != ErrorCode.ERR_Success)
            {
                response.Error = error;
                reply();
                return;
            }

            daily.SetVipDailyClaimed(1);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
