using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemOperateHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOperateRequest, M2C_ItemOperateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemOperateRequest request, M2C_ItemOperateResponse response, Action reply)
        {
            try
            {
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                long bagInfoID = request.OperateBagID;

                ItemLocType locType = ItemLocType.ItemLocBag;
              
                if (request.OperateType == 4)
                {
                    locType = ItemLocType.ItemLocEquip;
                }
                if (request.OperateType == 7)
                {
                    locType = (ItemLocType)(int.Parse(request.OperatePar));
                }

                BagInfo useBagInfo = bagComponentServer.GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null && request.OperateType != 8)
                {
                    reply();
                    return;
                }

                //通知客户端背包刷新
                M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

                //出售道具
                if (request.OperateType == 2 && locType == ItemLocType.ItemLocBag)
                {
                    Log.Error("request.OperateType == 222");
                }

                //穿戴装备
                if (request.OperateType == 3)
                {
                    Log.Error("request.OperateType == 3");
                }

                //卸下装备
                if (request.OperateType == 4)
                {
                    Log.Error("request.OperateType == 4");
                }

                //鉴定装备
                if (request.OperateType == 5)
                {
                  
                }

                //放入仓库
                if (request.OperateType == 6)
                {
                    int hourseId = int.Parse(request.OperatePar);
                    if (bagComponentServer.IsBagFullByLoc(hourseId))
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;     //错误码:仓库已满
                        reply();
                        return;
                    }
                    if (useBagInfo.Loc != (int)ItemLocType.ItemLocBag)
                    {
                        Log.Error($"C2M_ItemOperateHandler 5");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    bagComponentServer.OnChangeItemLoc(useBagInfo, (ItemLocType)hourseId, ItemLocType.ItemLocBag);

                    m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }

                //放回背包
                if (request.OperateType == 7)
                {
                    int hourseId = useBagInfo.Loc;
                    if (bagComponentServer.IsBagFullByLoc((int)ItemLocType.ItemLocBag))
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;     //错误码:仓库已满
                        reply();
                        return;
                    }
                    if (useBagInfo.Loc != hourseId)
                    {
                        Log.Error($"C2M_ItemOperateHandler 6");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    bagComponentServer.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, (ItemLocType)hourseId);
                    unit.GetComponent<TaskComponentServer>().OnGetItemForWarehouse(useBagInfo.ItemID);
                    m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }

             
                if (unit.IsRobot())
                {
                    DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, bagComponentServer).Coroutine();
                }

                MessageHelper.SendToClient(unit, m2c_bagUpdate);
                //通知客户端属性刷新
                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }
    }
}
