using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetHeXinHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_PetHeXinHeChengRequest, M2C_PetHeXinHeChengResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_PetHeXinHeChengRequest request, M2C_PetHeXinHeChengResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo bagInfo_1 = null;
            BagInfo bagInfo_2 = null;
            if (bagInfo_1 == null || bagInfo_2 == null)
            {
                reply();
                return;
            }
            if (bagInfo_1.ItemID != bagInfo_2.ItemID)
            {
                reply();
                return;
            }
            
            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo_1.ItemID);
            /*if (Item.PetHeXinHeChengID==0)
            {
                reply();
                return;
            }
            */

            using ListComponent<long> costids = new ListComponent<long>() { bagInfo_1.BagInfoID,bagInfo_2.BagInfoID };
            //bagComponentServer.OnAddItemData($"{Item.PetHeXinHeChengID};1", $"{ItemGetWay.PetHeXinHeCheng}_{TimeHelper.ServerNow()}");
            reply();
            await ETTask.CompletedTask;
        }

    }
}
