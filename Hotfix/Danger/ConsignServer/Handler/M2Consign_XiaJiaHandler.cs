using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_XiaJiaHandler : AMActorRpcHandler<Scene, M2Consign_XiaJiaRequest, Consign2M_XiaJiaResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_XiaJiaRequest request, Consign2M_XiaJiaResponse response, Action reply)
        {
            ConsignSceneComponent consign = scene.GetComponent<ConsignSceneComponent>();
            using CoroutineLock consignLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Consign, consign.InstanceId);
            ConsignItemInfo item = consign.FindShangJiaItem(request.BelongId, request.ConsignItemInfoId, out var items);
            if (!consign.Initialized || request.UnitID <= 0 || item == null || item.UserId != request.UnitID)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            DBConsignInfo db = consign.GetPaiMaiDBByBelongId(item.BelongId);
            if (ConsignHelper.IsConsignExpired(item))
            {
                ConsignStorageHelper.MoveExpiredItems(db);
                await consign.SavePaiMaiData(db);
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            int index = items.IndexOf(item);
            items.RemoveAt(index);
            try
            {
                await consign.SavePaiMaiData(db);
            }
            catch (Exception e)
            {
                items.Insert(index, item);
                response.Error = ErrorCode.ERR_Error;
                Log.Error(e);
                reply();
                return;
            }
            response.ConsignItemInfo = item;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
