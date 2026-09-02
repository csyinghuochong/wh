using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_CollectHandler : AMActorRpcHandler<Scene, C2Consign_CollectRequest, Consign2C_CollectResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_CollectRequest request, Consign2C_CollectResponse response, Action reply)
        {
            if (request.UserId <= 0 || request.ConsignItemInfoId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            if (request.Collect == 0)
            {
                await consignScene.RemoveCollect(request.UserId, request.ConsignItemInfoId);
                response.Collect = 0;
                reply();
                return;
            }

            if (request.Collect != 1)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            ConsignItemInfo item = consignScene.FindShangJiaItem(request.BelongId, request.ConsignItemInfoId, out _);
            if (item == null || ConsignHelper.IsConsignExpired(item))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            if (item.UserId == request.UserId)
            {
                response.Error = ErrorCode.Err_ConsignCollectSelfError;
                reply();
                return;
            }

            int error = await consignScene.AddCollect(request.UserId, request.ConsignItemInfoId);
            if (error != ErrorCode.ERR_Success)
            {
                response.Error = error;
                reply();
                return;
            }

            response.Collect = 1;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
