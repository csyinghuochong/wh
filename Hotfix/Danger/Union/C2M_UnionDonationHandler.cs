using System;

namespace ET
{
    /// <summary>
    /// 捐献业务已下线：工会资源统一走道具发放链路
    /// OnAddItemData -> UnionHelper.IsUnionResourceItem -> SendUnionResource(OperateType=6) -> UnionInfo.ResourceList。
    /// 这里保留空实现，只为让旧客户端的捐献请求能正常回包；服务端不再有 OperateType=3/4。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_UnionDonationHandler : AMActorLocationRpcHandler<Unit, C2M_UnionDonationRequest, M2C_UnionDonationResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionDonationRequest request, M2C_UnionDonationResponse response, Action reply)
        {
            reply();
            await ETTask.CompletedTask;
        }
    }
}
