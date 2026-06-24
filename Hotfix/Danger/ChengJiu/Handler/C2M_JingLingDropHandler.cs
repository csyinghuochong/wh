using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JingLingDropHandler : AMActorLocationRpcHandler<Unit, C2M_JingLingDropRequest, M2C_JingLingDropResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingLingDropRequest request, M2C_JingLingDropResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            int jinglingid = chengJiuComponentServer.JingLingId;
            if (jinglingid == 0 || chengJiuComponentServer.RandomDrop == 1)
            {
                reply();
                return;
            }
            LDElf ldElf = LDElfCategory.Instance.Get(jinglingid);
            if (ldElf.FunctionType!= JingLingFunctionType.RandomDrop)
            {
                reply();
                return;
            }
            int dropId = int.Parse(ldElf.FunctionValue);
            if (dropId == 0)
            {
                Log.Warning($"C2M_JingLingDropRequest.dropId == 0");
            }
            List<RewardItem> droplist = new List<RewardItem>();
            DropHelper.DropIDToDropItem_2(dropId, droplist);
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < droplist.Count)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            unit.GetComponent<BagComponentServer>().OnAddItemData(droplist, string.Empty, $"{ItemGetWay.JingLing}_{TimeHelper.ServerNow()}", false);

            chengJiuComponentServer.RandomDrop = 1;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
