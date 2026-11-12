using System;

namespace ET
{
    /// <summary>时装续费：把已激活记录的 ActivteTime 往后推一天。</summary>
    [ActorMessageHandler]
    public class C2M_FashionRenewHandler : AMActorLocationRpcHandler<Unit, C2M_FashionRenewRequest, M2C_FashionRenewResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FashionRenewRequest request, M2C_FashionRenewResponse response, Action reply)
        {
            if (request.FashionId == 0 || !LDFashionCategory.Instance.Contain(request.FashionId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            FashionInfo fashionInfo = bagComponentServer.GetFashionInfo(request.FashionId);
            if (fashionInfo == null)
            {
                response.Error = ErrorCode.ERR_FashionNoActive;
                reply();
                return;
            }

            fashionInfo.ActivteTime += TimeHelper.OneDay;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
