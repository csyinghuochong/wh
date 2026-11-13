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

            // 与激活协议一致：续费后的服务端时间戳先同步给自己，再回 response，
            // 客户端不再本地 +1 天，避免客户端/服务端时钟与天数漂移。
            bagComponentServer.SyncFashionActiveList(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
