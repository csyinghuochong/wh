using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2U_UnionKickOutHandler : AMActorRpcHandler<Scene, C2U_UnionKickOutRequest, U2C_UnionKickOutResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionKickOutRequest request, U2C_UnionKickOutResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo =await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo == null)
            {
                reply();
                return;
            }
            bool have = false;
            for (int i = dBUnionInfo.UnionInfo.UnionPlayerList.Count -1; i >= 0; i--)
            {
                if (dBUnionInfo.UnionInfo.UnionPlayerList[i].UserID == request.UserId)
                {
                    have = true;
                    dBUnionInfo.UnionInfo.UnionPlayerList.RemoveAt(i);
                }
            }

            if (!have)
            {
                reply();
                return;
            }

            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();

            // 在线走 Location；失败（含离线 ERR_NotFoundActor）再改库，无需先 T2G_GateUnitInfo
            U2M_UnionKickOutRequest kickRequest = new U2M_UnionKickOutRequest() { UserId = request.UserId };
            M2U_UnionKickOutResponse kickResponse = (M2U_UnionKickOutResponse)await ActorLocationSenderComponent.Instance.Call(request.UserId, kickRequest);
            if (kickResponse.Error != ErrorCode.ERR_Success)
            {
                long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
                D2G_GetComponent d2GGet = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.NumericComponent });
                NumericComponent numericComponent = d2GGet.Component as NumericComponent;
                numericComponent.Set(NumericType.UnionId_0, 0, false);
                D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = request.UserId, EntityByte = MongoHelper.ToBson(numericComponent), ComponentType = DBHelper.NumericComponent });

                d2GGet = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.RoleInfoComponent });
                RoleInfoComponentServer roleInfoComponentServer = d2GGet.Component as RoleInfoComponentServer;
                roleInfoComponentServer.RoleInfo.UnionName = string.Empty;
                d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = request.UserId, EntityByte = MongoHelper.ToBson(roleInfoComponentServer), ComponentType = DBHelper.RoleInfoComponent });
            }
            reply();
        }
    }
}
