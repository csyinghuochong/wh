using System;

namespace ET
{

    [ActorMessageHandler]
    public class A2M_PetMingLoginHandler : AMActorRpcHandler<Unit, A2M_PetMingLoginRequest, M2A_PetMingLoginResponse>
    {
        protected override async ETTask Run(Unit unit, A2M_PetMingLoginRequest request, M2A_PetMingLoginResponse response, Action reply)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            
            //numericComponent.ApplyValue(NumericType.PetMineLogin, 1);
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            taskComponentServer.OnPetMineLogin(request.PetMineList, request.PetMingExtend);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
