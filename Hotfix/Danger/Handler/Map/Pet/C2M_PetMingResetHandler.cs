using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetMingResetHandler : AMActorLocationRpcHandler<Unit, C2M_PetMingResetRequest, M2C_PetMingResetResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetMingResetRequest request, M2C_PetMingResetResponse response, Action reply)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.PetMineReset) >= 3)
            {
                response.Error = ErrorCode.ERR_TimesIsNot;
                reply();
                return;
            }

            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();   
            if (roleInfoComponent.RoleInfo.Diamond < 350)
            {
                response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                reply();
                return;
            }
            int sceneid = BattleHelper.GetSceneIdByType( MapTypeEnum.PetMing );
            numericComponent.ApplyChange( null, NumericType.PetMineReset, 1, 0 );
            roleInfoComponent.UpdateRoleData( UserDataType.Diamond,  "-350");
            roleInfoComponent.AddFubenTimes(sceneid, 5);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
