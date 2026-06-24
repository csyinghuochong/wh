using System;

namespace ET
{
    //宠物蛋放入孵化池
    [ActorMessageHandler]
    public class C2M_RolePetEggPutHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetEggPut, M2C_RolePetEggPut>
    {
        protected override async ETTask Run(Unit unit, C2M_RolePetEggPut request, M2C_RolePetEggPut response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            RolePetEgg rolePetEgg = petComponentServer.RolePetEggs[request.Index];
            if (rolePetEgg.ItemId != 0)
            {
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoId);
            if (useBagInfo == null)
            {
                reply();
                return;
            }
            
            bagComponentServer.OnCostItemData(request.BagInfoId, 1);
            rolePetEgg.ItemId = useBagInfo.ItemID;
            rolePetEgg.FuLing = useBagInfo.FuLing;
            rolePetEgg.EndTime = 0;
            response.RolePetEgg = rolePetEgg;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
