using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_SeasonOpenJingHeHandler : AMActorLocationRpcHandler<Unit, C2M_SeasonOpenJingHeRequest, M2C_SeasonOpenJingHeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SeasonOpenJingHeRequest request, M2C_SeasonOpenJingHeResponse response, Action reply)
        {
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();   
            if (roleInfoComponent.RoleInfo.OpenJingHeIds.Contains(request.JingHeId))
            {
                response.Error = ErrorCode.ERR_AlreadyLearn;
                reply();
                return;
            }

            if (!SeasonJingHeConfigCategory.Instance.Contain(request.JingHeId))
            {
                Log.Error($"C2M_SeasonLevelRewardRequest 4");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            SeasonJingHeConfig seasonJingHeConfig = SeasonJingHeConfigCategory.Instance.Get(request.JingHeId);
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  
            if (!bagComponentServer.CheckNeedItem(seasonJingHeConfig.Cost))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            bagComponentServer.OnCostItemData(seasonJingHeConfig.Cost, ItemLocType.ItemLocBag, ItemGetWay.Season);
            roleInfoComponent.RoleInfo.OpenJingHeIds.Add(request.JingHeId);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
