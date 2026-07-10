using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemFumoUseHandler : AMActorLocationRpcHandler<Unit, C2M_ItemFumoUseRequest, M2C_ItemFumoUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemFumoUseRequest request, M2C_ItemFumoUseResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocBag, bagInfoID);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            unit.GetComponent<BagComponentServer>().OnCostItemData(bagInfoID, 1);
            unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.FoMoNumber_213, 0, 1);

            LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);

            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent( TastConditionType.FuMoQulity_41, ldItem.Quality, 1 );
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
