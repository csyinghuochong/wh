using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleOpenCangKuHandler : AMActorLocationRpcHandler<Unit, C2M_RoleOpenCangKuRequest, M2C_RoleOpenCangKuResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleOpenCangKuRequest request, M2C_RoleOpenCangKuResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            int cangkuNumber = bagComponentServer.CangKuNumber;
            if (cangkuNumber >= BagComponentServer.MaxCangKuNumber)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }

            string costItems = LDGlobalValueCategory.Instance.Get(38).Value;
            if (!bagComponentServer.OnCostItemData(costItems, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            bagComponentServer.CangKuNumber = cangkuNumber + 1;
            response.CangKuNumber = bagComponentServer.CangKuNumber;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
