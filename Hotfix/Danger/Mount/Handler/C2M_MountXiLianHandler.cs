using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountXiLianHandler : AMActorLocationRpcHandler<Unit, C2M_MountXiLian, M2C_MountXiLian>
    {
        protected override async ETTask Run(Unit unit, C2M_MountXiLian request, M2C_MountXiLian response, Action reply)
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

            BagInfo bagInfo = bag.GetItemByUId(request.BagInfoID);
            if (bagInfo == null || !ItemNewHelper.IsValideBagLoc(bagInfo.Loc))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            if (!LDItemCategory.Instance.Contain(bagInfo.ItemID))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            int costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
            if (ldItem.ItemType == ItemTypeEnum.SubType_Mount_XiLian_30)
            {
                costNum = 1;
            }

            if (bagInfo.ItemNum < costNum)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            switch (ldItem.ItemType)
            {
                case ItemTypeEnum.SubType_Mount_XiLian_30:
                    MountHelper.ResetMountAptitude(mountInfo);
                    MountHelper.ApplyAptitudeAttributes(mountInfo);
                    break;
                case ItemTypeEnum.SubType_Mount_Exp_31:
                    int addExp = ldItem.GetTypeParam1() * costNum;
                    if (addExp <= 0)
                    {
                        response.Error = ErrorCode.ERR_Mount_NoUseItem;
                        reply();
                        return;
                    }

                    mountInfo.MountExp += addExp;
                    break;
                default:
                    response.Error = ErrorCode.ERR_Mount_NoUseItem;
                    reply();
                    return;
            }

            bag.OnCostItemData($"{bagInfo.ItemID};{costNum}", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
