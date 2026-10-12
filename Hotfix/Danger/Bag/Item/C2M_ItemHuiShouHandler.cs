using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemHuiShouHandler : AMActorLocationRpcHandler<Unit, C2M_ItemHuiShouRequest, M2C_ItemHuiShouResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ItemHuiShouRequest request, M2C_ItemHuiShouResponse response, Action reply)
        {
            List<long> huishouList = request.OperateBagID;
            if (huishouList.Count == 0)
            {
                response.Error = ErrorCode.ERR_ItemUseError;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<long>> idsByLoc = new Dictionary<int, List<long>>();
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bag.GetItemByUId(huishouList[i]);
                if (bagInfo == null)
                {
                    continue;
                }

                if (!idsByLoc.TryGetValue(bagInfo.Loc, out List<long> ids))
                {
                    ids = new List<long>();
                    idsByLoc[bagInfo.Loc] = ids;
                }

                ids.Add(bagInfo.BagInfoID);
            }

            if (idsByLoc.Count == 0)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<long>> kv in idsByLoc)
            {
                bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
