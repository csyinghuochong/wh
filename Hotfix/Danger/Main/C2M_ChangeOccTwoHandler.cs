using System;

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
            
            int OccTwo = roleInfoData.OccTwo;
            ////判断当前角色是否已经进行转职
            if (OccTwo != 0 )
            {
                response.Error = ErrorCode.ERR_Occ_Hint_2;
                reply();
                return;
            }

            if (!LDOccupation_TransferCategory.Instance.Contain(request.OccTwoID))
            {
                Log.Error($"C2M_ChangeOccTwoRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
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

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

            skillSet.OnChangeOccTwoRequest(request.OccTwoID);
            taskComponentServer.OnChangeOccTwo();

         
            reply();
            await ETTask.CompletedTask;
        }
    }
}
