using System;

namespace ET
{
    //宠物出战设置
    [ActorMessageHandler]
    public class C2M_RolePetFormationSetHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetFormationSet, M2C_RolePetFormationSet>
    {
        protected override async ETTask Run(Unit unit, C2M_RolePetFormationSet request, M2C_RolePetFormationSet response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            switch (request.SceneType)
            {
                case MapTypeEnum.PetDungeon:
                    petComponentServer.PetFormations = request.PetFormat;
                    break;
                case MapTypeEnum.PetTianTi:
                    petComponentServer.TeamPetList = request.PetFormat;
                    break;
                case MapTypeEnum.PetMing:
                    petComponentServer.PetMingList = request.PetFormat;
                    petComponentServer.PetMingPosition = request.PetPosition;   
                    break;
            }
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, petComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
