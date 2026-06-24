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
            if (bagComponentServer.FashionActiveIds.Contains(request.FashionId))
            {
                response.Error = ErrorCode.ERR_AlreadyLearn;
                reply();
                return;
            }

            LDFashion ldFashion = LDFashionCategory.Instance.Get(request.FashionId  );
            if (!bagComponentServer.CheckNeedItem(ldFashion.ActiveCost))
            {
                response.Error = ErrorCode.ERR_HouBiNotEnough;
                reply();
                return;
            }

            Function_Fight.UnitUpdateProperty_Base(unit, true, true);

            bagComponentServer.OnCostItemData(ldFashion.ActiveCost, ItemLocType.ItemLocBag, 98 );
            bagComponentServer.FashionActiveIds.Add( request.FashionId );

            reply();
            await ETTask.CompletedTask;
        }
    }
}
