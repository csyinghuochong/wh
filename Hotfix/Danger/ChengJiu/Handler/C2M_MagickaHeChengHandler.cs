using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_MagickaHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_MagickaHeChengRequest, M2C_MagickaHeChengResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MagickaHeChengRequest request, M2C_MagickaHeChengResponse response, Action reply)
        {
            if (request.OperateBagID.Count != 3)
            {
                response.Error = ErrorCode.ERR_MagicHeCheng_1;
                reply();
                return; 
            }


            List<int> removeids = new List<int>();

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  

            for (int  i= 0;i < request.OperateBagID.Count; i++)
            { 
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID[i]);

                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                removeids.Add(bagInfo.ItemID);
            }

            if (removeids.Count != 3)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
