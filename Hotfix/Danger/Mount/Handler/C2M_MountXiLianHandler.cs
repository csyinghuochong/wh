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
            if (ldItem.ItemType != ItemTypeEnum.SubType_Mount_XiLian_30)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            if (bagInfo.ItemNum < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            MountXiLianResult result = MountHelper.BuildXiLianResult(mountInfo);
            if (result == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoUseItem;
                reply();
                return;
            }

            mountInfo.XiLianResult = result;
            bag.OnCostItemData($"{bagInfo.ItemID};1", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            response.MountInfo = mountInfo;
            response.XiLianResult = result;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
