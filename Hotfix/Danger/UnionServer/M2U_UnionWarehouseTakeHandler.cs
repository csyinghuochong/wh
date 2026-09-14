using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehouseTakeHandler : AMActorRpcHandler<Scene, M2U_UnionWarehouseTakeRequest, U2M_UnionWarehouseTakeResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehouseTakeRequest request, U2M_UnionWarehouseTakeResponse response, Action reply)
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

            if (request.BagInfoID > 0)
            {
                BagInfo exist = UnionWarehouseHelper.FindWarehouseItem(unionInfo, request.BagInfoID);
                if (exist == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                int num = request.ItemNum > 0 ? request.ItemNum : exist.ItemNum;
                if (num <= 0 || num > exist.ItemNum)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                response.BagInfo = UnionWarehouseHelper.CloneBagInfo(exist, num);
                exist.ItemNum -= num;
                if (exist.ItemNum <= 0)
                {
                    unionInfo.WarehouseList.Remove(exist);
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

                string pwd = dBUnionInfo.WarehousePassword ?? string.Empty;
                if (string.IsNullOrEmpty(pwd) || pwd != (request.Password ?? string.Empty))
                {
                    response.Error = string.IsNullOrEmpty(request.Password)
                            ? ErrorCode.ERR_Union_WarehouseNeedPassword
                            : ErrorCode.ERR_Union_WarehousePassword;
                    reply();
                    return;
                }

                if (unionInfo.WarehouseGold < request.ItemNum)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

                unionInfo.WarehouseGold -= request.ItemNum;
            }

            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            response.WarehouseList = unionInfo.WarehouseList;
            response.WarehouseGold = unionInfo.WarehouseGold;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
