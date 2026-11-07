using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountEatExpHandler : AMActorLocationRpcHandler<Unit,  C2M_MountEatExp, M2C_MountEatExp>
    {
        protected override async ETTask Run(Unit unit,  C2M_MountEatExp request, M2C_MountEatExp response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            int itemId = request.ItemID;
            if (!LDItemCategory.Instance.Contain(itemId))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            if (ldItem.ItemType != ItemTypeEnum.SubType_Mount_Exp_31)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            int costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
            int addExp = ldItem.GetTypeParam1() * costNum;
            if (addExp <= 0)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            ItemLocType loc = ItemNewHelper.GetToItemLocType(ItemBigType.Type_Item, itemId);
            if (bag.GetItemNumber(ItemBigType.Type_Item, itemId, loc) < costNum)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            // 按配置 Id 扣。同一 Id 分在多个格子时，一次扣完。
            if (!bag.OnCostItemData($"{ItemBigType.Type_Item}~{itemId}~{costNum}", loc, ItemGetWay.ItemXiLian))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            mountComponentServer.MountAddExp(mountInfo, addExp);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
