using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionKeJiLearnHandler : AMActorLocationRpcHandler<Unit, C2M_UnionKeJiLearnRequest, M2C_UnionKeJiLearnResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionKeJiLearnRequest request, M2C_UnionKeJiLearnResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();   
            int kejiid = roleInfoComponentServer.RoleInfo.UnionKeJiList[request.Position];

            /*
            UnionKeJiConfig unionKeJiConfig = UnionKeJiConfigCategory.Instance.Get(kejiid);
            if (unionKeJiConfig.NextID == 0)
            {
                response.UnionKeJiList = roleInfoComponentServer.RoleInfo.UnionKeJiList;
                response.Error = ErrorCode.ERR_UnionXiuLianMax;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (!bagComponentServer.CheckNeedItem( unionKeJiConfig.LearnCost ))
            {
                response.UnionKeJiList = roleInfoComponentServer.RoleInfo.UnionKeJiList;
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            long dbCacheId = DBHelper.GetUnionServerId(unit);
            U2M_UnionKeJiLearnResponse d2GGetUnit = (U2M_UnionKeJiLearnResponse)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2U_UnionKeJiLearnRequest()
            {
                UnionId = unit.GetUnionId(),    
                KeJiId = unionKeJiConfig.NextID,
                Position = request.Position,    
            });

            if(d2GGetUnit.Error != ErrorCode.ERR_Success)
            {
                response.UnionKeJiList = roleInfoComponentServer.RoleInfo.UnionKeJiList;
                response.Error = d2GGetUnit.Error;
                reply();
                return;
            }

            bagComponentServer.OnCostItemData(unionKeJiConfig.LearnCost, ItemLocType.ItemLocBag, ItemGetWay.UnionXiuLian);
            roleInfoComponentServer.RoleInfo.UnionKeJiList[request.Position] = unionKeJiConfig.NextID;
            response.UnionKeJiList = roleInfoComponentServer.RoleInfo.UnionKeJiList;
            */
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}
