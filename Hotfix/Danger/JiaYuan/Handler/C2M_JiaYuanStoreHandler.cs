using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public  class C2M_JiaYuanStoreHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanStoreRequest, M2C_JiaYuanStoreResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanStoreRequest request, M2C_JiaYuanStoreResponse response, Action reply)
        {
            int hourseId = request.HorseId;
            if (hourseId >= (int)ItemLocType.ItemLocMax)
            {
                Log.Error($"C2M_JiaYuanStoreRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;    
                reply();
                return;
            }
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            int leftCell = bag.GetBagLeftCell(hourseId);
            if (leftCell<= 0)
            {
                response.Error = ErrorCode.ERR_BagIsFull;     //错误码:仓库已满
                reply();
                return;
            }
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            List <BagInfo> bagInfos = bag.BagItemList;

            List<BagInfo> itemList = new List<BagInfo>();
            
            for (int i = 0; i < itemList.Count; i++)
            {
                bag.OnChangeItemLoc(itemList[i], (ItemLocType)hourseId, ItemLocType.ItemLocBag);
                m2c_bagUpdate.BagInfoUpdate.Add(itemList[i]);
                leftCell--;
                if (leftCell <= 0)
                {
                    break;
                }
            }
            MessageHelper.SendToClient(unit, m2c_bagUpdate);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
