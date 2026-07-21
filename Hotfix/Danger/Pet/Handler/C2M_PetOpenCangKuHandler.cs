using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetOpenCangKuHandler : AMActorLocationRpcHandler<Unit, C2M_PetOpenCangKu, M2C_PetOpenCangKu>
    {
        protected override async ETTask Run(Unit unit, C2M_PetOpenCangKu request, M2C_PetOpenCangKu response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            string costitem = CommonConfig.PetOpenCangKu[request.OpenIndex - 1];
            if (!bagComponentServer.CheckNeedItem(costitem))
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }
            if (petComponentServer.PetCangKuOpen.Contains(request.OpenIndex - 1)) 
            {
                response.Error = ErrorCode.ERR_CangKu_Already;
                reply();
                return;
            }

            petComponentServer.PetCangKuOpen.Add(request.OpenIndex - 1);
            bagComponentServer.OnCostItemData(costitem, ItemLocType.ItemLocBag, ItemGetWay.PetHeXinExplore);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
