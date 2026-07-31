using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JingLingUseHandler : AMActorLocationRpcHandler<Unit, C2M_JingLingUseRequest, M2C_JingLingUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingLingUseRequest request, M2C_JingLingUseResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            if (unitComponent.Get(chengJiuComponentServer.JingLingUnitId) != null)
            {
                unitComponent.Remove(chengJiuComponentServer.JingLingUnitId);
            }
            if (chengJiuComponentServer.JingLingId != 0)
            {
                LDElf ldElf = LDElfCategory.Instance.Get(chengJiuComponentServer.JingLingId);
                //if (ldElf.FunctionType == JingLingFunctionType.AddSkill)
                //{
                //    int skillid = int.Parse(ldElf.FunctionValue);
                //    BuffManagerComponent buffManagerComponent = unit.GetComponent<BuffManagerComponent>();
                //    buffManagerComponent.BuffRemoveBySkillid(skillid);
                //}
            }

            if (chengJiuComponentServer.JingLingId == request.JingLingId)
            {
                chengJiuComponentServer.JingLingId = 0;
                chengJiuComponentServer.JingLingUnitId = 0;
            }
            else
            {
                chengJiuComponentServer.JingLingId = (request.JingLingId);
                chengJiuComponentServer.JingLingUnitId = UnitFactory.CreateJingLing(unit, chengJiuComponentServer.JingLingId).Id;
            }
            response.JingLingId = chengJiuComponentServer.JingLingId;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
