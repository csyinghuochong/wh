using System;

namespace ET
{
    /// <summary>客户端查询入口：用当前登录 Unit.Id 向交易服查询本人的暂存记录，不接受客户端指定他人角色。</summary>
    [ActorMessageHandler]
    public class C2M_ConsignStorageListHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignStorageListRequest, M2C_ConsignStorageListResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignStorageListRequest request, M2C_ConsignStorageListResponse response, Action reply)
        {
            Consign2M_StorageResponse storage = await ConsignStorageHelper.RequestStorage(unit);
            response.Error = storage.Error;
            if (storage.Error == ErrorCode.ERR_Success)
            {
                response.Items = storage.Items;
            }

            reply();
        }
    }

    /// <summary>客户端领取入口：校验暂存归属、背包容量并通过背包入口分发，回包返回处理结果，资产变化沿用现有推送。</summary>
    [ActorMessageHandler]
    public class C2M_ConsignStorageReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignStorageReceiveRequest, M2C_ConsignStorageReceiveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignStorageReceiveRequest request, M2C_ConsignStorageReceiveResponse response, Action reply)
        {
            response.Error = await ConsignStorageHelper.ReceiveStorageItem(unit, request.BelongId, request.StorageId);
            reply();
        }
    }
}
