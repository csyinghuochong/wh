using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehousePutHandler : AMActorRpcHandler<Scene, M2U_UnionWarehousePutRequest, U2M_UnionWarehousePutResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehousePutRequest request, U2M_UnionWarehousePutResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                DBUnionInfo data = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
                if (data?.UnionInfo == null)
                {
                    response.Error = ErrorCode.ERR_Union_Not_Exist;
                    reply();
                    return;
                }

                UnionInfo info = data.UnionInfo;
                UnionWarehouseHelper.EnsureWarehouse(info);
                if (UnionHelper.GetUnionPlayerInfo(info.UnionPlayerList, request.UnitId) == null)
                {
                    response.Error = ErrorCode.ERR_Union_NoPlayer;
                    reply();
                    return;
                }

                if (request.BagInfo != null)
                {
                    if (request.BagInfo.ItemNum <= 0)
                    {
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    if (!UnionWarehouseHelper.TryPutItem(info, request.BagInfo))
                    {
                        response.Error = ErrorCode.ERR_WarehouseIsFull;
                        reply();
                        return;
                    }
                }
                else
                {
                    if (!UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0
                        || request.ItemNum > long.MaxValue - info.WarehouseGold)
                    {
                        response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                        reply();
                        return;
                    }

                    info.WarehouseGold += request.ItemNum;
                }

                await DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, data);
                response.WarehouseList = info.WarehouseList;
                response.WarehouseGold = info.WarehouseGold;
                reply();
            }
        }
    }
}
