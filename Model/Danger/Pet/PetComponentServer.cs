using System.Collections.Generic;

namespace ET
{

    public class PetComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {

        public long FightPetId = 0;
        public List<PetInfo> PetInfos = new List<PetInfo>();

        public List<long> PetFormations = new List<long>() { };     //宠物副本
    }
}
