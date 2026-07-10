using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_AccountWarehousOperateHandler : AMActorLocationRpcHandler<Unit, C2M_AccountWarehousOperateRequest, M2C_AccountWarehousOperateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_AccountWarehousOperateRequest request, M2C_AccountWarehousOperateResponse response, Action reply)
        {

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                long accountId = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.AccInfoID;
                DBAccountBagInfo dBAccountBagWarehouse = await DBHelper.GetComponent<DBAccountBagInfo>(unit.DomainZone(), accountId);
                if (dBAccountBagWarehouse == null)
                {
                    Log.Error("dBAccountBagWarehouse == null");
                    Console.WriteLine("dBAccountBagWarehouse == null");
                    response.Error = ErrorCode.ERR_NetWorkError;
                    reply();
                    return;
                }

                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                switch (request.OperatateType)
                {
                    ///1放入仓库  2取出仓库 3整理仓库 
                    case 1:
                        if (dBAccountBagWarehouse.BagInfoList.Count >= LDGlobalValueCategory.Instance.AccountBagMax)
                        {
                            response.Error = ErrorCode.ERR_WarehouseIsFull;
                            reply();
                            return;
                        }
                        BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
                        if (bagInfo == null)
                        {
                            response.Error = ErrorCode.ERR_ItemNotExist;
                            reply();
                            return;
                        }
                       
                        LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
                        int equipType = ItemNewHelper.GetNewEquipType(bagInfo);
                        if (ldItem.ItemType != 3 || equipType > 100)
                        {
                            response.Error = ErrorCode.ERR_ItemNotExist;
                            reply();
                            return;
                        }

                        if (dBAccountBagWarehouse.HaveItemById(bagInfo.BagInfoID) != -1)
                        {
                            response.Error = ErrorCode.ERR_AlreadyHave;
                            reply();
                            return;
                        }
                        dBAccountBagWarehouse.BagInfoList.Add(bagInfo);
                        bagComponentServer.OnCostItemData(new List<long>(){ bagInfo.BagInfoID }, ItemLocType.ItemLocBag);
                        break;
                    case 2:
                        if (bagComponentServer.GetBagLeftCell() < 1)
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }
                        int index = dBAccountBagWarehouse.HaveItemById(request.OperateBagID);
                        if (index == -1)
                        {
                            response.Error = ErrorCode.ERR_ItemNotExist;
                            reply();
                            return;
                        }
                        bagInfo = dBAccountBagWarehouse.BagInfoList[index];
                        dBAccountBagWarehouse.BagInfoList.RemoveAt(index);
                        bagComponentServer.OnAddItemData(bagInfo, bagInfo.GetWay);
                        break;
                    case 3:
                        ItemNewHelper.ItemLitSort(dBAccountBagWarehouse.BagInfoList);
                        break;
                    default:
                        break;
                }

                DBHelper.SaveComponentCache(unit.DomainZone(), unit.Id, bagComponentServer).Coroutine();

                DBHelper.SaveComponent(unit.DomainZone(), accountId, dBAccountBagWarehouse).Coroutine();
                reply();
            }
            await ETTask.CompletedTask;
        }
    }
}
