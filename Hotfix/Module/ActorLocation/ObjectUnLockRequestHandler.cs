using System;

namespace ET
{
    [ActorMessageHandler]
    public class ObjectUnLockRequestHandler: AMActorRpcHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response, Action reply)
        {
            if (!scene.GetComponent<LocationComponent>().UnLock(request.Key, request.OldInstanceId, request.InstanceId))
            {
                response.Error = ErrorCode.ERR_TransferFailError;
            }

            reply();

            await ETTask.CompletedTask;
        }
    }
}
