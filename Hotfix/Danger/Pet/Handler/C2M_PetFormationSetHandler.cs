using System;

namespace ET
{
    //宠物出战设置
    [ActorMessageHandler]
    public class C2M_PetFormationSetHandler : AMActorLocationRpcHandler<Unit, C2M_PetFormationSet, M2C_PetFormationSet>
    {
        protected override async ETTask Run(Unit unit, C2M_PetFormationSet request, M2C_PetFormationSet response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            switch (request.SceneType)
            {
                case MapTypeEnum.PetDungeon:
                    petComponentServer.PetFormations = request.PetFormat;
                    break;
            }
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, petComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
