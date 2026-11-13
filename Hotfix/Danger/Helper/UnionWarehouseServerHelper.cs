namespace ET
{
    public static class UnionWarehouseServerHelper
    {
        public static string GrantAccess(UnionSceneComponent scene, UnionInfo info, long unitId)
        {
            string token = System.Guid.NewGuid().ToString("N");
            scene.WarehouseAccess[unitId] = (info.UnionId, token, UnionHelper.GetUnionPlayerInfo(info.UnionPlayerList, unitId));
            return token;
        }

        public static int ValidateAccess(UnionSceneComponent scene, DBUnionInfo data, long unitId, string token)
        {
            UnionInfo info = data.UnionInfo;
            UnionPlayerInfo member = UnionHelper.GetUnionPlayerInfo(info.UnionPlayerList, unitId);
            if (member == null)
            {
                return ErrorCode.ERR_Union_NoPlayer;
            }
            if (info.LeaderId == unitId || string.IsNullOrEmpty(data.WarehousePassword))
            {
                return ErrorCode.ERR_Success;
            }
            if (!string.IsNullOrEmpty(token) && scene.WarehouseAccess.TryGetValue(unitId, out var access)
                && access.UnionId == info.UnionId && access.Token == token && ReferenceEquals(access.Member, member))
            {
                return ErrorCode.ERR_Success;
            }
            return ErrorCode.ERR_Union_WarehouseNeedPassword;
        }

        public static void RevokeAccess(UnionSceneComponent scene, long unionId)
        {
            foreach (long unitId in new System.Collections.Generic.List<long>(scene.WarehouseAccess.Keys))
            {
                if (scene.WarehouseAccess[unitId].UnionId == unionId)
                {
                    scene.WarehouseAccess.Remove(unitId);
                }
            }
        }

        public static bool CanReceive(BagComponentServer bag, BagInfo item)
        {
            ItemLocType loc = UnionWarehouseHelper.GetBagType(item);
            int pileSum = ItemNewHelper.GetNewItemPileSum(item);
            int cells = ItemNewHelper.CalcNeedNewCells(bag.GetItemByLoc(loc), item.ItemType, item.ItemID, item.ItemNum,
                pileSum, item.ItemFlags, item.MakePlayer, item.MakeTime);
            return cells <= bag.GetBagLeftCell((int)loc);
        }

        public static async ETTask<int> DeliverItem(Unit unit, BagInfo item)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (CanReceive(bag, item) && bag.OnAddItemData(item, $"{ItemGetWay.UnionWarehouse}_0"))
            {
                return ErrorCode.ERR_Success;
            }

            MailInfo mail = new MailInfo { MailId = IdGenerater.Instance.GenerateId() };
            mail.ParamList.Add("公会仓库道具");
            mail.ParamList.Add("背包空间不足，道具已通过邮件交付。");
            mail.ItemList.Add(item);
            return await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(unit.Id), unit.Id, mail);
        }
    }
}
