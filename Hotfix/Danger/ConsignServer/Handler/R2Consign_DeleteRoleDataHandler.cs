using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class R2Consign_DeleteRoleDataHandler : AMActorRpcHandler<Scene, R2Consign_DeleteRoleData, Consign2R_DeleteRoleData>
    {

        protected override async ETTask Run(Scene scene, R2Consign_DeleteRoleData request, Consign2R_DeleteRoleData response, Action reply)
        {
            ConsignSceneComponent rankScene = scene.GetComponent<ConsignSceneComponent>();
            await rankScene.OnDeleteRole(request.DeleteType, request.DeleUserID);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
