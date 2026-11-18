using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PickItemHandler : AMActorLocationRpcHandler<Unit, C2M_PickItemRequest, M2C_PickItemResponse>
    {
        private int OnFubenPick(Unit unit, C2M_PickItemRequest request, List<long> removeIds)
        {
            List<DropInfo> drops = request.ItemIds;
            long serverTime = TimeHelper.ServerNow();
            int errorCode = ErrorCode.ERR_Success;
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            string pickGetWay = $"{ItemGetWay.PickItem}_{serverTime}";

            for (int i = drops.Count - 1; i >= 0; i--)
            {
                // Run 已用服务端记录替换请求数据；装备直接入包，不能重新走 RewardItem 随机属性。
                DropInfo drop = drops[i];
                bool added;
                if (drop.ItemType == ItemBigType.Type_Equip)
                {
                    added = bag.OnAddItemData(drop.EquipInfo, pickGetWay);
                }
                else
                {
                    added = bag.OnAddItemData(new List<RewardItem>
                    {
                        new RewardItem
                        {
                            ItemType = drop.ItemType, ItemID = drop.ItemID, ItemNum = drop.ItemNum, ItemFlags = drop.ItemFlags,
                        }
                    }, pickGetWay);
                }
                if (!added)
                {
                    errorCode = ErrorCode.ERR_BagIsFull;
                    continue;
                }
                UnitHelper.RemovePickedDrop(unit, drop.UnitId);
                removeIds.Add(drop.UnitId);
                SceneCreatureHelp.SendFubenPickMessage(unit, drop);
            }
            return errorCode;
        }


        protected override async ETTask Run(Unit unit, C2M_PickItemRequest request, M2C_PickItemResponse response, Action reply)
        {
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
                if (info == null)
                {
                    response.RemoveIds.Add(dropId);
                    request.ItemIds.RemoveAt(i);
                    continue;
                }
                if (PositionHelper.Distance2D(unit.Position, new UnityEngine.Vector3(info.X, info.Y, info.Z)) > CommonConfig.PickDistance)
                {
                    // 已打开这具尸体的拾取界面后，不再限制拾取距离。
                    Unit monster = units.Get(info.BeKillId);
                    MonsterBodyComponent body = monster != null ? monster.GetComponent<MonsterBodyComponent>() : null;
                    if (body == null || !body.Viewers.TryGetValue(unit.Id, out long sessionId)
                        || unit.GetComponent<UnitGateComponent>().GateSessionActorId != sessionId)
                    {
                        response.Error = ErrorCode.ERR_Parameter;
                        reply();
                        return;
                    }
                }
                if (info.ItemType == ItemBigType.Type_Equip
                        && (info.EquipInfo == null || info.ItemNum != 1 || info.EquipInfo.ItemNum != 1
                            || info.EquipInfo.BagInfoID <= 0 || info.EquipInfo.ItemType != info.ItemType
                            || info.EquipInfo.ItemID != info.ItemID || info.EquipInfo.ItemFlags != info.ItemFlags))
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
                // 类型、数量、状态及装备属性均采用服务端记录，入包成功后才删除。
                request.ItemIds[i] = info;
            }

            if (request.ItemIds.Count ==0)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            List<long> removeIds = new List<long>();
            response.Error = OnFubenPick(unit, request, removeIds);
            response.RemoveIds.AddRange(removeIds);
            reply();
            await ETTask.CompletedTask;
        }
    }

}
