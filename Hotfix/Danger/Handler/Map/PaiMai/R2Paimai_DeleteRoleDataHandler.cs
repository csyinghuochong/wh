using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class R2Paimai_DeleteRoleDataHandler : AMActorRpcHandler<Scene, R2Paimai_DeleteRoleData, Paimai2R_DeleteRoleData>
    {

        protected override async ETTask Run(Scene scene, R2Paimai_DeleteRoleData request, Paimai2R_DeleteRoleData response, Action reply)
        {
            PaiMaiSceneComponent rankScene = scene.GetComponent<PaiMaiSceneComponent>();
            rankScene.OnDeleteRole(request.DeleteType, request.DeleUserID);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
