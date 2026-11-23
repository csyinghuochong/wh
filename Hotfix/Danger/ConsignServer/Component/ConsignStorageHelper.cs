using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 交易服暂存区的转存、查询和领取逻辑。数据保存在上架分类文档中，按 UserId 区分角色，当前不是个人仓库。
    /// 过期商品保留完整实例；成交收益仅保留空入口，待个人仓库接入。
    /// </summary>
    public static class ConsignStorageHelper
    {
        /// <summary>
        /// 将已过期的上架记录移动到同文档的暂存列表，不发邮件、不放入玩家背包。
        /// 调用方须持有交易锁，并在返回 true 时保存整个文档，使货架移除和暂存新增一起落库。
        /// </summary>
        /// <returns>本次是否移动了过期商品；已经转存的记录不会再次出现在货架中。</returns>
        public static bool MoveExpiredItems(DBConsignInfo db)
        {
            bool changed = false;
            for (int i = db.PaiMaiItemInfos.Count - 1; i >= 0; --i)
            {
                ConsignItemInfo item = db.PaiMaiItemInfos[i];
                if (!ConsignHelper.IsConsignExpired(item))
                {
                    continue;
                }

                db.StorageItems.Add(new ConsignStorageInfo
                {
                    Id = item.Id,
                    UserId = item.UserId,
                    BelongId = (int)db.Id,
                    BagInfo = item.BagInfo,
                    ExpiredItem = true,
                });
                db.PaiMaiItemInfos.RemoveAt(i);
                changed = true;
            }

            return changed;
        }

        /// <summary>成交收益预留入口，待个人仓库实现后接入；当前不保存或发放金币。</summary>
        /// <param name="sold">交易服确认的成交数据，包含卖家 ID、单价和实际成交数量。</param>
        public static void AddSaleIncome(ConsignItemInfo sold)
        {
            // TODO：个人仓库接入后，按实际成交金额扣除手续费，将收益存入卖家个人仓库。
        }

        /// <summary>在指定分类文档中同时匹配记录 ID 和所属角色；无匹配时返回 null，不修改暂存数据。</summary>
        public static ConsignStorageInfo FindStorageItem(DBConsignInfo db, long userId, long storageId)
        {
            foreach (ConsignStorageInfo item in db.StorageItems)
            {
                if (item.Id == storageId && item.UserId == userId)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>汇总该角色在全部上架分类中的暂存记录，返回深拷贝，防止后续入包修改交易服保留的实例。</summary>
        public static List<ConsignStorageInfo> GetStorageItems(this ConsignSceneComponent self, long userId)
        {
            List<ConsignStorageInfo> result = new List<ConsignStorageInfo>();
            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                foreach (ConsignStorageInfo item in db.StorageItems)
                {
                    if (item.UserId == userId)
                    {
                        result.Add(CloneHelper.DeepClone(item));
                    }
                }
            }

            return result;
        }

        /// <summary>角色逻辑服向该角色归属交易服发起内部暂存操作，角色 ID 固定使用当前 Unit.Id。</summary>
        /// <param name="unit">已经登录的当前角色，不能用客户端传入的角色 ID 代替。</param>
        /// <param name="operation">0 查询全部并检查过期转存；1 只读单条；2 在资产保存后删除单条。</param>
        /// <param name="belongId">单条操作的原上架分类及交易文档 ID，必须大于 0；查询全部时为 0。</param>
        /// <param name="storageId">单条操作的暂存记录 ID，必须大于 0；查询全部时为 0。</param>
        public static async ETTask<Consign2M_StorageResponse> RequestStorage(Unit unit, int operation = 0, int belongId = 0, long storageId = 0)
        {
            return (Consign2M_StorageResponse)await MessageHelper.CallActor(DBHelper.GetPaiMaiServerId(unit), new M2Consign_StorageRequest
            {
                UnitId = unit.Id,
                Operation = operation,
                BelongId = belongId,
                StorageId = storageId,
            });
        }

        /// <summary>
        /// 领取本人的一条交易暂存记录，统一通过背包入口分发，保存资产后删除交易服暂存记录。
        /// </summary>
        /// <param name="unit">当前已登录角色。</param>
        /// <param name="belongId">查询结果中该条记录的原上架分类及交易文档 ID。</param>
        /// <param name="storageId">查询结果中的暂存记录 ID。</param>
        /// <returns>0 表示发放和暂存记录删除均完成；非 0 为参数、归属、容量或内部处理错误。</returns>
        public static async ETTask<int> ReceiveStorageItem(Unit unit, int belongId, long storageId)
        {
            if (belongId <= 0 || storageId <= 0)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                Consign2M_StorageResponse read = await RequestStorage(unit, 1, belongId, storageId);
                if (read.Error != ErrorCode.ERR_Success)
                {
                    return read.Error;
                }

                if (unit.IsDisposed)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                }

                if (read.Items.Count == 0)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                }

                ConsignStorageInfo storage = read.Items[0];
                BagInfo item = storage.BagInfo;
                if (item.ItemNum <= 0 || !ItemNewHelper.IsValidItem(item.ItemType, item.ItemID))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                }

                BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                bool currency = ItemNewHelper.GetItemToUserDataType(item.ItemType, item.ItemID) != UserDataType.None;
                if (!currency)
                {
                    ItemLocType loc = ItemNewHelper.GetToItemLocType(item.ItemType, item.ItemID);
                    if (!ItemNewHelper.IsValideBagLoc((int)loc))
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }

                    int cells = ItemNewHelper.CalcNeedNewCells(bag.GetItemByLoc(loc), item.ItemType, item.ItemID, item.ItemNum,
                        ItemNewHelper.GetNewItemPileSum(item), item.ItemFlags, item.MakePlayer, item.MakeTime);
                    if (bag.GetBagLeftCell((int)loc) < cells)
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                    }
                }

                string getWay = $"{(storage.ExpiredItem ? ItemGetWay.XiaJia : ItemGetWay.PaiMaiSell)}_{TimeHelper.ServerNow()}";
                item.GetWay = getWay;
                if (!bag.OnAddItemData(item, getWay))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                }

                Entity assetComponent = currency ? (Entity)unit.GetComponent<RoleInfoComponentServer>() : bag;
                int zone = UnitZoneHelper.GetHomeZone(unit);
                await DBHelper.SaveComponentCache(zone, unit.Id, assetComponent);
                await DBHelper.SaveComponent(zone, unit.Id, assetComponent);
                Consign2M_StorageResponse remove = await RequestStorage(unit, 2, belongId, storageId);
                return remove.Error;
            }
        }
    }
}
