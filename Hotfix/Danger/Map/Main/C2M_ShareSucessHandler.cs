using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShareSucessHandler : AMActorLocationRpcHandler<Unit, C2M_ShareSucessRequest, M2C_ShareSucessResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShareSucessRequest request, M2C_ShareSucessResponse response, Action reply)
        {
            if (request.ShareType != 1 && request.ShareType != 2 && request.ShareType != 8)
            {
                reply();
                return;
            }

            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (roleInfo.Lv < 10)
            {
                response.Error = ErrorCode.ERR_LevelIsNot;
                reply();
                return;
            }
            
            TaskComponent taskComponent = unit.GetComponent<TaskComponent>();
            if (taskComponent.OnLineTime < 30)
            {
                response.Error = ErrorCode.Err_OnLineTimeNot;
                reply();
                return;
            }
            if (taskComponent.GetHuoYueDu() < 30)
            {
                response.Error = ErrorCode.ERR_HuoYueNot;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long shareSet = numericComponent.GetAsLong(NumericType.FenShangSet);
            if ((shareSet & request.ShareType) > 0)
            {
                response.Error = ErrorCode.ERR_TimesIsNot;
                reply();
                return;
            }

            long accountZone = DBHelper.GetRealmCenter();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            R2M_ShareSucessResponse centerAccount = (R2M_ShareSucessResponse)await ActorMessageSenderComponent.Instance.Call(accountZone, new M2R_ShareSucessRequest()
            {
                AccountId = roleInfoComponentServer.RoleInfo.AccInfoID
            });
            if (centerAccount.Error != ErrorCode.ERR_Success)
            {
                response.Error = centerAccount.Error;
                reply();
                return;
            }

            shareSet = shareSet | (long)request.ShareType;
            numericComponent.ApplyValue(NumericType.FenShangSet, shareSet);

            if (request.ShareType == 8)
            {
                //给金币
                unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(UserDataType.Gold, "1", true, ItemGetWay.Share);
            }
            else
            {
                //给钻石
                unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(UserDataType.Diamond, "120", true, ItemGetWay.Share);
            }

            unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.ShareTotalNumber_220, 0, 1);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
