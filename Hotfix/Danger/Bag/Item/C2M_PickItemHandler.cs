using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PickItemHandler : AMActorLocationRpcHandler<Unit, C2M_PickItemRequest, M2C_PickItemResponse>
    {
        private int OnFubenPick(Unit unit, C2M_PickItemRequest request, int sceneTypeEnum, List<long> removeIds)
        {
            List<DropInfo> drops = request.ItemIds;
           
            long serverTime = TimeHelper.ServerNow();
            int errorCode = ErrorCode.ERR_Success;
            //DropType ==  0 公共掉落 2保护掉落   1私有掉落 3 归属掉落

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            List<RewardItem> pendingRewards = new List<RewardItem>();
            List<(int itemId, long unitId, int dropType)> pendingMeta = new List<(int, long, int)>();
            string pickGetWay = $"{ItemGetWay.PickItem}_{serverTime}";

            for (int i = drops.Count - 1; i >= 0; i--)
            {
                Unit unitDrop = unitComponent.Get(drops[i].UnitId);
                DropComponent dropComponent = null;
                if (drops[i].DropType != 1)
                {
                    if (unitDrop == null)
                    {
                        errorCode = ErrorCode.ERR_NetWorkError;
                        continue;
                    }
                    dropComponent = unitDrop.GetComponent<DropComponent>();
                    int dropType = dropComponent.GetDropType();
                }
                int addItemID = dropComponent !=null ? dropComponent.ItemID : drops[i].ItemID;
                int addItemNum = dropComponent != null ? dropComponent.ItemNum : drops[i].ItemNum;
                pendingRewards.Add(new RewardItem() {  ItemType = drops[i].ItemType , ItemID = addItemID, ItemNum = addItemNum });
                pendingMeta.Add((addItemID, drops[i].UnitId, drops[i].DropType));
            }

            if (pendingRewards.Count > 0)
            {
                bool batchOk = bag.OnAddItemData(pendingRewards, pickGetWay);
                if (!batchOk)
                {
                    // 背包空间不足时回退逐个添加，保留部分成功语义
                    for (int i = 0; i < pendingRewards.Count; i++)
                    {
                        List<RewardItem> one = new List<RewardItem> { pendingRewards[i] };
                        if (!bag.OnAddItemData(one, pickGetWay))
                        {
                            errorCode = ErrorCode.ERR_BagIsFull;
                            continue;
                        }
                        DropInfo dropInfo = null;
                        for (int d = 0; d < drops.Count; d++)
                        {
                            if (drops[d].UnitId == pendingMeta[i].unitId)
                            {
                                dropInfo = drops[d];
                                break;
                            }
                        }
                        if (dropInfo != null)
                        {
                            SceneCreatureHelp.SendFubenPickMessage(unit, dropInfo);
                        }
                        UnitHelper.RemovePickedDrop(unit, pendingMeta[i].unitId);
                        removeIds.Add(pendingMeta[i].unitId);
                        LDItem ldItemOne = LDItemCategory.Instance.Get(pendingMeta[i].itemId);
                        
                    }
                }
                else
                {
                    for (int i = 0; i < pendingMeta.Count; i++)
                    {
                        DropInfo dropInfo = null;
                        for (int d = 0; d < drops.Count; d++)
                        {
                            if (drops[d].UnitId == pendingMeta[i].unitId)
                            {
                                dropInfo = drops[d];
                                break;
                            }
                        }
                        if (dropInfo != null)
                        {
                            SceneCreatureHelp.SendFubenPickMessage(unit, dropInfo);
                        }
                        UnitHelper.RemovePickedDrop(unit, pendingMeta[i].unitId);
                        removeIds.Add(pendingMeta[i].unitId);
                        LDItem ldItem = LDItemCategory.Instance.Get(pendingMeta[i].itemId);
                    }
                }
            }
            
            return errorCode;
        }


        protected override async ETTask Run(Unit unit, C2M_PickItemRequest request, M2C_PickItemResponse response, Action reply)
        {
            UnitInfoComponent unitInfoComponent = unit.GetComponent<UnitInfoComponent>();
            if (unit.Type != UnitType.Player || unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1
                    || request.ItemIds.Count > 100)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            UnitComponent units = unit.GetParent<UnitComponent>();
            HashSet<long> seen = new HashSet<long>();
            for (int i = request.ItemIds.Count - 1; i >= 0; i--) 
            {
                long dropId = request.ItemIds[i].UnitId;
                if (!seen.Add(dropId))
                {
                    request.ItemIds.RemoveAt(i);
                    continue;
                }
                Unit dropUnit = units.Get(dropId);
                DropInfo info = null;
                if (dropUnit != null && dropUnit.Type == UnitType.DropItem)
                {
                    info = dropUnit.GetComponent<DropComponent>().ToDropInfo(dropUnit);
                }
                else
                {
                    info = unitInfoComponent.Drops.Find(drop => drop.UnitId == dropId);
                }
                if (info == null)
                {
                    response.RemoveIds.Add(dropId);
                    request.ItemIds.RemoveAt(i);
                    continue;
                }
                if (PositionHelper.Distance2D(unit.Position, new UnityEngine.Vector3(info.X, info.Y, info.Z)) > CommonConfig.PickDistance)
                {
                    response.Error = ErrorCode.ERR_Parameter;
                    reply();
                    return;
                }
                // 类型、数量和来源均采用服务端记录，私人掉落入包成功后才删除。
                request.ItemIds[i] = info;
            }

            if (request.ItemIds.Count ==0)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            List<long> removeIds = new List<long>();
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            int sceneTypeEnum = mapComponent.MapTypeEnum;
            response.Error = OnFubenPick(unit, request, sceneTypeEnum, removeIds);

            response.RemoveIds.AddRange(removeIds);
            reply();
            await ETTask.CompletedTask;
        }
    }

}
