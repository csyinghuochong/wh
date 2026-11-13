using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionRenameHandler : AMActorLocationRpcHandler<Unit, C2M_UnionRenameRequest, M2C_UnionRenameResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionRenameRequest request, M2C_UnionRenameResponse response, Action reply)
        {
            long unionId = unit.GetUnionId();
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            if (!UnionHelper.IsValidUnionName(request.NewName))
            {
                response.Error = ErrorCode.ERR_Union_NameValied;
                reply();
                return;
            }

            // 公会服先检查身份及名字，扣费后提交仍重新校验。
            M2U_UnionRenameRequest innerRequest = new M2U_UnionRenameRequest
            {
                UnionId = unionId,
                UnitId = unit.Id,
                NewName = request.NewName,
                Preview = true,
            };
            U2M_UnionRenameResponse inner = (U2M_UnionRenameResponse)await ActorMessageSenderComponent.Instance.Call(DBHelper.GetUnionServerId(unit), innerRequest);
            if (inner.Error != ErrorCode.ERR_Success)
            {
                response.Error = inner.Error;
                reply();
                return;
            }

            string raw = LDGlobalValueCategory.Instance.GetByKey(GlobalValueKey.Global_Union_Name_Reset_Consume).Value.Trim();
            if (raw.StartsWith("{") && raw.EndsWith("}"))
            {
                raw = raw.Substring(1, raw.Length - 2).Trim();
            }
            List<RewardItem> costs = ItemNewHelper.GetRewardItems(raw);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            List<BagInfo> refundItems = new List<BagInfo>();
            foreach (BagInfo item in bag.GetItemByLoc(ItemLocType.ItemLocBag))
            {
                refundItems.Add(UnionWarehouseHelper.CloneBagInfo(item, item.ItemNum));
            }
            Dictionary<int, long> refundCurrency = new Dictionary<int, long>();
            foreach (RewardItem cost in costs)
            {
                SnapshotCurrency(bag, refundCurrency, cost.ItemType, cost.ItemID);
                SnapshotCurrency(bag, refundCurrency, cost.ItemType, ItemNewHelper.GetCostFallbackItemId(cost.ItemType, cost.ItemID));
            }
            if (costs.Count > 0 && !bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            for (int i = refundItems.Count - 1; i >= 0; i--)
            {
                BagInfo original = refundItems[i];
                BagInfo current = bag.GetItemByUId(original.BagInfoID);
                original.ItemNum -= current == null ? 0 : current.ItemNum;
                if (original.ItemNum <= 0)
                {
                    refundItems.RemoveAt(i);
                    continue;
                }
                original.BagInfoID = IdGenerater.Instance.GenerateId();
            }
            foreach (int dataType in new List<int>(refundCurrency.Keys))
            {
                refundCurrency[dataType] -= bag.GetItemNumber(ItemBigType.Type_Item, dataType);
            }

            innerRequest.Preview = false;
            inner = (U2M_UnionRenameResponse)await ActorMessageSenderComponent.Instance.Call(DBHelper.GetUnionServerId(unit), innerRequest);
            response.Error = inner.Error;
            if (inner.Error != ErrorCode.ERR_Success && costs.Count > 0)
            {
                // 按实际扣除退还，包含绑定货币不足时补扣的非绑定货币及原物属性。
                foreach (var currency in refundCurrency)
                {
                    if (currency.Value > 0)
                    {
                        unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(currency.Key, currency.Value.ToString(), true, ItemGetWay.UnionWarehouse);
                    }
                }
                foreach (BagInfo item in refundItems)
                {
                    int error = await UnionWarehouseServerHelper.DeliverItem(unit, item);
                    if (error != ErrorCode.ERR_Success)
                    {
                        response.Error = error;
                    }
                }
            }
            response.UnionName = inner.UnionName;
            reply();
        }

        private static void SnapshotCurrency(BagComponentServer bag, Dictionary<int, long> snapshot, int itemType, int itemId)
        {
            int dataType = ItemNewHelper.GetItemToUserDataType(itemType, itemId);
            if (dataType != UserDataType.None && !snapshot.ContainsKey(dataType))
            {
                snapshot.Add(dataType, bag.GetItemNumber(itemType, itemId));
            }
        }
    }
}
