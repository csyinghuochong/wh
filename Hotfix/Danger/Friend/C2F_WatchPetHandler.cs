using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPetHandler : AMActorRpcHandler<Scene, C2F_WatchPetRequest, F2C_WatchPetResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPetRequest request, F2C_WatchPetResponse response, Action reply)
        {
            // 可跨区查看：按被查看玩家 UnitId 归属服取 DBCache
            long dbCacheId = DBHelper.GetUnitCacheConfig(request.UnitID);
            D2G_GetComponent d2GGetUnit_1 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UnitID, Component = DBHelper.PetComponent });
            PetComponentServer petComponentServer = d2GGetUnit_1.Component as PetComponentServer;
            if (petComponentServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }

            D2G_GetComponent d2GGetUnit_2 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UnitID, Component = DBHelper.BagComponentServer });
            BagComponentServer bagComponentsServer = d2GGetUnit_2.Component as BagComponentServer;
            if (bagComponentsServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            response.RolePetInfos = petComponentServer.GetPetInfo( request.PetId );
            response.PetHeXinList = bagComponentsServer.PetHeXinList;
            
            D2G_GetComponent d2GGetUnit_3 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UnitID, Component = DBHelper.NumericComponent });
            NumericComponent numericComponent = d2GGetUnit_3.Component as NumericComponent;
            if (numericComponent == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            foreach ((int key, long value) in numericComponent.NumericDic)
            {
                if (key >= (int)NumericType.Max)
                {
                    continue;
                }
                response.Ks.Add(key);
                response.Vs.Add(value);
            }
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
