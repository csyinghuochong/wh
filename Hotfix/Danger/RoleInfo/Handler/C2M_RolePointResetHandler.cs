using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_RolePointResetHandler : AMActorLocationRpcHandler<Unit, C2M_RolePointResetRequest, M2C_RolePointResetResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_RolePointResetRequest request, M2C_RolePointResetResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Lv < RoleAddPointHelper.GetAutoLevel())
            {
                response.Error = ErrorCode.ERR_LevelNoEnough;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int resetNumber = numericComponent.GetAsInt(NumericType.PointResetTimes);

            if (resetNumber >= LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Add_Point_Free_Resets))
            {
                //扣除消耗
            }

              
            numericComponent.ApplyValue(NumericType.Point_Ti_1, 0);
            numericComponent.ApplyValue(NumericType.Point_Li_2, 0);
            numericComponent.ApplyValue(NumericType.Point_Zhi_3, 0);
            numericComponent.ApplyValue(NumericType.Point_Nian_4, 0);
            numericComponent.ApplyValue(NumericType.Point_Min_5, 0);
            numericComponent.ApplyValue(NumericType.Point_Xun_6, 0);

            int remainPoint = RoleAddPointHelper.GetTotalFreePointByLevel(roleInfo.Lv);

            numericComponent.ApplyValue(NumericType.PointRemain, remainPoint);
            numericComponent.ApplyChange(null, NumericType.PointResetTimes, 1, 0);

            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}