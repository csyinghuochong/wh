using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShouJiTreasureHandler : AMActorLocationRpcHandler<Unit, C2M_ShouJiTreasureRequest, M2C_ShouJiTreasureResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShouJiTreasureRequest request, M2C_ShouJiTreasureResponse response, Action reply)
        {
            ShoujiComponentServer shoujiComponentServer = unit.GetComponent<ShoujiComponentServer>();
            KeyValuePairInt keyValuePairInt = shoujiComponentServer.GetTreasureInfo(request.ShouJiId);
          
            List<long> huishouList = request.ItemIds;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemUseError;
                    reply();
                    return;
                }
            }

          
            reply();
            await ETTask.CompletedTask;
        }
    }
}
