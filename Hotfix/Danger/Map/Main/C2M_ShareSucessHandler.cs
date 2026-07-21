using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShareSucessHandler : AMActorLocationRpcHandler<Unit, C2M_ShareSucessRequest, M2C_ShareSucessResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShareSucessRequest request, M2C_ShareSucessResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            if (request.ShareType != 1 && request.ShareType != 2 && request.ShareType != 8)
            {
                reply();
                return;
            }

            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Lv < 10)
            {
                response.Error = ErrorCode.ERR_LevelIsNot;
                reply();
                return;
            }
            
            if (taskComponentServer.OnLineTime < 30)
            {
                response.Error = ErrorCode.Err_OnLineTimeNot;
                reply();
                return;
            }
            if (taskComponentServer.GetHuoYueDu() < 30)
            {
                response.Error = ErrorCode.ERR_HuoYueNot;
                reply();
                return;
            }

            long shareSet = numericComponent.GetAsLong(NumericType.FenShangSet);
            if ((shareSet & request.ShareType) > 0)
            {
                response.Error = ErrorCode.ERR_TimesIsNot;
                reply();
                return;
            }

            long accountZone = DBHelper.GetRealmCenter();
            R2M_ShareSucessResponse centerAccount = (R2M_ShareSucessResponse)await ActorMessageSenderComponent.Instance.Call(accountZone, new M2R_ShareSucessRequest()
            {
                AccountId = roleInfo.AccInfoID
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
                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Gold, "1", true, ItemGetWay.Share);
            }
            else
            {
                //给钻石
                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Diamond, "120", true, ItemGetWay.Share);
            }

            chengJiuComponentServer.TriggerEvent(ChengJiuTargetEnum.ShareTotalNumber_220, 0, 1);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
