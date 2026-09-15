using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_VipPrivilegeClaimHandler : AMActorLocationRpcHandler<Unit, C2M_VipPrivilegeClaimRequest, M2C_VipPrivilegeClaimResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_VipPrivilegeClaimRequest request, M2C_VipPrivilegeClaimResponse response, Action reply)
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

            if (VipHelp.HasPrivilegeClaimed(pro, vipLevel))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }

            int error = VipHelp.TryAddReward(unit, cfg.Reward, ItemGetWay.VipPrivilege);
            if (error != ErrorCode.ERR_Success)
            {
                response.Error = error;
                reply();
                return;
            }

            VipHelp.AddPrivilegeClaimed(pro, vipLevel);
            unit.GetComponent<RechargeComponentServer>()?.NotifyClient();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
