using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehousePutHandler : AMActorRpcHandler<Scene, M2U_UnionWarehousePutRequest, U2M_UnionWarehousePutResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehousePutRequest request, U2M_UnionWarehousePutResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo?.UnionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            UnionInfo unionInfo = dBUnionInfo.UnionInfo;
            UnionWarehouseHelper.EnsureWarehouse(unionInfo);
            if (UnionHelper.GetUnionPlayerInfo(unionInfo.UnionPlayerList, request.UnitId) == null)
            {
                response.Error = ErrorCode.ERR_Union_NoPlayer;
                reply();
                return;
            }

            if (request.BagInfo != null)
            {
                if (!UnionWarehouseHelper.TryPutItem(unionInfo, request.BagInfo))
                {
                    response.Error = ErrorCode.ERR_WarehouseIsFull;
                    reply();
                    return;
                }
            }
            else
            {
                if (!UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0)
                {
                    response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                    reply();
                    return;
                }

                unionInfo.WarehouseGold += request.ItemNum;
            }

            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            response.WarehouseList = unionInfo.WarehouseList;
            response.WarehouseGold = unionInfo.WarehouseGold;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
