using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_FashionActiveHandler : AMActorLocationRpcHandler<Unit, C2M_FashionActiveRequest, M2C_FashionActiveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FashionActiveRequest request, M2C_FashionActiveResponse response, Action reply)
        {
            if (request.FashionId == 0 || !LDFashionCategory.Instance.Contain(request.FashionId))
            {
                Log.Error($"C2M_FashionActiveRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

            // 没有记录则新增，激活时间为当前服务器时间；已有记录则把激活时间往后推一天
            FashionInfo fashionInfo = bagComponentServer.GetFashionInfo(request.FashionId);
            if (fashionInfo == null)
            {
                bagComponentServer.FashionActiveList.Add(new FashionInfo()
                {
                    FashionId = request.FashionId,
                    ActivteTime = TimeHelper.ServerNow()
                });
            }
            else
            {
                fashionInfo.ActivteTime += TimeHelper.OneDay;
            }

            // TODO 以后接入激活消耗：CheckNeedItem / OnCostItemData

            Function_Fight.UnitUpdateProperty_Base(unit, true, true);

            // 服务端权威下发全量持有列表，客户端不再本地预更新。
            // 必须在 reply() 之前发：同一 Session 消息有序，客户端 Session.Call 返回时列表已是最新；
            // 否则 UI 拿到 response 立刻读 GetState 会短暂显示「未拥有」。
            bagComponentServer.SyncFashionActiveList(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
