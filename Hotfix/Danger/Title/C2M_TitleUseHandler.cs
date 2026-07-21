using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TitleUseHandler : AMActorLocationRpcHandler<Unit, C2M_TitleUseRequest, M2C_TitleUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TitleUseRequest request, M2C_TitleUseResponse response, Action reply)
        {
            TitleComponentServer titleComponent = unit.GetComponent<TitleComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (!titleComponent.IsHaveTitle(request.TitleId))
            {
                response.Error = ErrorCode.ERR_TitleNoActived;
                reply();
                return;
            }

            numericComponent.ApplyValue(NumericType.TitleID, request.TitleId);
            Function_Fight.UnitUpdateProperty_Base(unit,true, true);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
