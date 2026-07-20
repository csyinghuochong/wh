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
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            ChengJiuComponentServer chengJiu = unit.GetComponent<ChengJiuComponentServer>();
            TaskComponentServer task = unit.GetComponent<TaskComponentServer>();
            BagInfo useBagInfo = bag.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoID);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            bag.OnCostItemData(bagInfoID, 1);
            chengJiu.TriggerEvent(ChengJiuTargetEnum.FoMoNumber_213, 0, 1);

            LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);

            task.TriggerTaskEvent( TastConditionType.FuMoQulity_41, ldItem.Quality, 1 );
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
