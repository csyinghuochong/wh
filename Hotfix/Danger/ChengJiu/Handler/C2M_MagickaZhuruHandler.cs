using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MagickaZhuruHandler : AMActorLocationRpcHandler<Unit, C2M_MagickaZhuruRequest, M2C_MagickaZhuruResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_MagickaZhuruRequest request, M2C_MagickaZhuruResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            int nexid = chengJiuComponentServer.GetNextMagickaSlotIdByPosition(request.Position);
            int curid = chengJiuComponentServer.GetCurrentMagickaSlotIdByPosition(request.Position);
            if (nexid <= curid)
            {
                //response.Error = ErrorCode.ERR_MagicMaxLevel;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            int addExp = 0;
            List<long> bagidList = new List<long>();

            for (int i = 0; i < request.OperateBagID.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID[i]);
                if (bagInfo == null)
                {
                    continue;
                }
                if (!CommonConfig.MagicAddShieldExp.TryGetValue(bagInfo.ItemID, out int addValue))
                {
                    continue;
                }

                if (addValue > 10)
                {
                    addValue = RandomHelper.NextInt((int)(addValue * 0.8f), (int)(addValue * 1.2f));
                }
                addExp += addValue * bagInfo.ItemNum;
                bagidList.Add(request.OperateBagID[i]);
            }

            response.AddExp = addExp;

            chengJiuComponentServer.OnAddMagickaExpByPosition( request.Position, addExp);

            //扣除装备
            bagComponentServer.OnCostItemData(bagidList, ItemLocType.ItemLocBag);

            Function_Fight.UnitUpdateProperty_Base(unit, true, true);

            response.MagickaSlotIds = chengJiuComponentServer.MagickaSlotIdList;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
