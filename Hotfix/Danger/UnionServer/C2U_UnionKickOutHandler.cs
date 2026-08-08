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

            // 在线走 Location；失败（含离线 ERR_NotFoundActor）直连归属服 Mongo，不进 DBCache
            U2M_UnionKickOutRequest kickRequest = new U2M_UnionKickOutRequest() { UserId = request.UserId };
            M2U_UnionKickOutResponse kickResponse = (M2U_UnionKickOutResponse)await ActorLocationSenderComponent.Instance.Call(request.UserId, kickRequest);
            if (kickResponse.Error != ErrorCode.ERR_Success)
            {
                int homeZone = UnitZoneHelper.GetHomeZone(request.UserId);
                NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(homeZone, request.UserId);
                if (numericComponent != null)
                {
                    numericComponent.Set(NumericType.UnionId_0, 0, false);
                    await DBHelper.SaveComponent(homeZone, request.UserId, numericComponent);
                }

                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, request.UserId);
                if (roleInfoComponentServer != null)
                {
                    roleInfoComponentServer.RoleInfo.UnionName = string.Empty;
                    await DBHelper.SaveComponent(homeZone, request.UserId, roleInfoComponentServer);
                }
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
