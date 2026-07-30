using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetFragmentDuiHuanHandler : AMActorLocationRpcHandler<Unit, C2M_PetFragmentDuiHuan, M2C_PetFragmentDuiHuan>
    {
        protected override async ETTask Run(Unit unit, C2M_PetFragmentDuiHuan request, M2C_PetFragmentDuiHuan response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            PetComponentServer pet = unit.GetComponent<PetComponentServer>();
            if (!PetHelper.IsShenShouFull(pet.RolePetInfos))
            {
                Log.Error($"C2M_PetFragmentDuiHuan 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item,10000136) < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError ;
                reply();
                return;
            }

            //bagComponentServer.OnCostItemData("10000136;1", ItemLocType.ItemLocBag, ItemGetWay.PetEggDuiHuan);
            //bagComponentServer.OnAddItemData($"{CommonConfig.PetFramgeItemId};1", $"{ItemGetWay.DuiHuan}_{TimeHelper.ServerNow()}");
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
