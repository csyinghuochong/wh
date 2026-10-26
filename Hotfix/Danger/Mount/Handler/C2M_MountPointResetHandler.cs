using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MountPointResetHandler : AMActorLocationRpcHandler<Unit, C2M_MountPointReset, M2C_MountPointReset>
    {
        protected override async ETTask Run(Unit unit, C2M_MountPointReset request, M2C_MountPointReset response, Action reply)
        {
            MountComponentServer mountComponentServer = unit.GetComponent<MountComponentServer>();
            MountInfo mountInfo = mountComponentServer.GetMountInfo(request.MountInfoId);
            if (mountInfo == null)
            {
                response.Error = ErrorCode.ERR_Mount_NoExist;
                reply();
                return;
            }

            List<RewardItem> costs = ItemNewHelper.GetRewardItems(ReadGlobalCost(GlobalValueKey.Global_Mount_Potential_Points_Reset_Consume));
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<RewardItem>> locCosts = costs.GroupCostByLoc();
            if (!bag.CanCostByLoc(locCosts) || !bag.CostByLoc(locCosts, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            MountHelper.InitAddPoint(mountInfo);
            MountHelper.ApplyAptitudeAttributes(mountInfo);
            mountComponentServer.NotifyMountUpdate(mountInfo);
            mountComponentServer.RefreshPlayerMountAttrs();
            response.MountInfo = mountInfo;
            reply();
            await ETTask.CompletedTask;
        }

        static string ReadGlobalCost(string key)
        {
            string raw = LDGlobalValueCategory.Instance.GetByKey(key).Value;
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            raw = raw.Trim();
            if (raw.Length >= 2 && raw[0] == '{' && raw[raw.Length - 1] == '}')
            {
                return raw.Substring(1, raw.Length - 2).Trim();
            }

            return raw;
        }
    }
}
