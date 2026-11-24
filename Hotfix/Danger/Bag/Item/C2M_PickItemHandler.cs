using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PickItemHandler : AMActorLocationRpcHandler<Unit, C2M_PickItemRequest, M2C_PickItemResponse>
    {
        private int OnFubenPick(Unit unit, Unit monster, C2M_PickItemRequest request, List<long> removeIds)
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
                    errorCode = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                    continue;
                }
                if (monster != null)
                {
                    monster.GetComponent<UnitInfoComponent>().Drops.Remove(drop);
                }
                else
                {
                    // 保留旧格子副本的无尸体拾取入口。
                    UnitHelper.RemovePickedDrop(unit, drop.UnitId);
                }
                removeIds.Add(drop.UnitId);
                SceneCreatureHelp.SendFubenPickMessage(unit, drop);
            }
            if (monster != null && removeIds.Count > 0) UnitHelper.NotifyMonsterDropsPicked(monster, removeIds);
            return errorCode;
        }


        protected override async ETTask Run(Unit unit, C2M_PickItemRequest request, M2C_PickItemResponse response, Action reply)
        {
            if (unit.Type != UnitType.Player || unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1
                    || request.MonsterId < 0 || request.ItemIds.Count > 100)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            UnitComponent units = unit.GetParent<UnitComponent>();
            Unit monster = null;
            if (request.MonsterId > 0)
            {
                monster = units.Get(request.MonsterId);
                if (monster == null || monster.Type != UnitType.Monster
                    || monster.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 1)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                    reply();
                    return;
                }
                MonsterBodyComponent body = monster.GetComponent<MonsterBodyComponent>();
                bool isViewing = body != null && body.Viewers.TryGetValue(unit.Id, out long sessionId)
                    && unit.GetComponent<UnitGateComponent>().GateSessionActorId == sessionId;
                if (!isViewing && PositionHelper.Distance2D(unit, monster) > CommonConfig.PickDistance)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }
            }
            HashSet<long> seen = new HashSet<long>();
            for (int i = request.ItemIds.Count - 1; i >= 0; i--) 
            {
                long dropId = request.ItemIds[i].UnitId;
                if (!seen.Add(dropId))
                {
                    request.ItemIds.RemoveAt(i);
                    continue;
                }
                DropInfo info = null;
                if (monster != null)
                {
                    info = monster.GetComponent<UnitInfoComponent>().Drops.Find(drop => drop.UnitId == dropId);
                }
                else
                {
                    Unit dropUnit = units.Get(dropId);
                    if (dropUnit != null && dropUnit.Type == UnitType.DropItem)
                    {
                        info = dropUnit.GetComponent<DropComponent>().ToDropInfo(dropUnit);
                    }
                }
                if (info == null)
                {
                    response.RemoveIds.Add(dropId);
                    request.ItemIds.RemoveAt(i);
                    continue;
                }
                if (monster == null && PositionHelper.Distance2D(unit.Position, new UnityEngine.Vector3(info.X, info.Y, info.Z)) > CommonConfig.PickDistance)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }
                if (info.ItemType == ItemBigType.Type_Equip
                        && (info.EquipInfo == null || info.ItemNum != 1 || info.EquipInfo.ItemNum != 1
                            || info.EquipInfo.BagInfoID <= 0 || info.EquipInfo.ItemType != info.ItemType
                            || info.EquipInfo.ItemID != info.ItemID || info.EquipInfo.ItemFlags != info.ItemFlags))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                    reply();
                    return;
                }
                // 类型、数量、状态及装备属性均采用服务端记录，入包成功后才删除。
                request.ItemIds[i] = info;
            }

            if (request.ItemIds.Count ==0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            List<long> removeIds = new List<long>();
            response.Error = OnFubenPick(unit, monster, request, removeIds);
            response.RemoveIds.AddRange(removeIds);
            reply();
            await ETTask.CompletedTask;
        }
    }

}
