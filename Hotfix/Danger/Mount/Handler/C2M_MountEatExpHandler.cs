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
            if (ldItem.ItemType != ItemTypeEnum.SubType_Mount_Exp_31)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            int costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
            if (bagInfo.ItemNum < costNum)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            int addExp = ldItem.GetTypeParam1() * costNum;
            if (addExp <= 0)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            mountComponentServer.MountAddExp(mountInfo, addExp);
            bag.OnCostItemData($"{bagInfo.ItemID};{costNum}", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
