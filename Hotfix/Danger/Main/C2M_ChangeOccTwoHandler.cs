using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChangeOccTwoHandler : AMActorLocationRpcHandler<Unit, C2M_ChangeOccTwoRequest, M2C_ChangeOccTwoResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChangeOccTwoRequest request, M2C_ChangeOccTwoResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            SkillSetComponentServer skillSet = unit.GetComponent<SkillSetComponentServer>();
            RoleInfo roleInfoData = roleInfo.RoleInfo;

            if (!LDOccupation_TransferCategory.Instance.Contain(request.OccTwoID))
            {
                Log.Error($"C2M_ChangeOccTwoRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (roleInfoData.OccTwo == request.OccTwoID)
            {
                response.Error = ErrorCode.ERR_Occ_Hint_2;
                reply();
                return;
            }

            LDOccupation occupation = LDOccupationCategory.Instance.Get(roleInfoData.Occ);
            bool canTransfer = false;
            int[] transferIds = occupation.TransferId;
            if (transferIds != null)
            {
                for (int i = 0; i < transferIds.Length; i++)
                {
                    if (transferIds[i] == request.OccTwoID)
                    {
                        canTransfer = true;
                        break;
                    }
                }
            }
            if (!canTransfer)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            int transferCount = roleInfoData.OccTransferCount;
            if (transferCount <= 0 && roleInfoData.OccTwo != 0)
            {
                transferCount = 1;
            }

            if (transferCount >= 1)
            {
                int costNum = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Occupation_Transfer_Comsume);
                List<RewardItem> costs = ItemNewHelper.GetRewardItems($"3~5~{costNum}");
                BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                if (!bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            skillSet.OnChangeOccTwoRequest(request.OccTwoID);
            taskComponentServer.OnChangeOccTwo();

            reply();
            await ETTask.CompletedTask;
        }
    }
}
