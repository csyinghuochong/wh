using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionCreateHandler : AMActorLocationRpcHandler<Unit, C2M_UnionCreateRequest, M2C_UnionCreateResponse>
    {
        private static int unionCreateNeedLevel;
        private static List<RewardItem> unionCreateCostItems;
        private static bool unionCreateCacheInit;

        private static void EnsureUnionCreateCache()
        {
            if (unionCreateCacheInit)
            {
                return;
            }

            unionCreateNeedLevel = 1;
            unionCreateCostItems = ItemNewHelper.GetRewardItems(UnionHelper.GetUnion_CreateCost());
            unionCreateCacheInit = true;
        }

        protected override async ETTask Run(Unit unit, C2M_UnionCreateRequest request, M2C_UnionCreateResponse response, Action reply)
        {
            EnsureUnionCreateCache();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsLong(NumericType.UnionId_0) != 0)
            {
                response.Error = ErrorCode.ERR_PlayerHaveUnion;
                reply();
                return;
            }
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Lv < unionCreateNeedLevel)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (unionCreateCostItems.Count > 0 && !bag.CheckNeedItem(unionCreateCostItems))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            long dbCacheId = DBHelper.GetUnionServerId(unit);
            U2M_UnionCreateResponse d2GGetUnit = (U2M_UnionCreateResponse)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2U_UnionCreateRequest() 
            {
                UnionName =request.UnionName,
                UnionPurpose = request.UnionPurpose,
                UserID = roleInfo.UserId,
                UnionBanner = request.UnionBanner,
                UnionPattern = request.UnionPattern,
            });

            if (d2GGetUnit.Error == ErrorCode.ERR_Success)
            {
                if (unionCreateCostItems.Count > 0)
                {
                    bag.OnCostItemData(unionCreateCostItems, ItemLocType.ItemLocBag, ItemGetWay.CostItem);
                }

                roleInfoComponentServer.SetUnionName(request.UnionName);
                numericComponent.ApplyValue( NumericType.UnionLeader, 1, true);
                numericComponent.ApplyValue( NumericType.UnionId_0, d2GGetUnit.UnionId, true);
            }
            response.Error = d2GGetUnit.Error;
            reply();
            await ETTask.CompletedTask;
        }

    }
}
