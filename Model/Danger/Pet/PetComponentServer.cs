using System.Collections.Generic;

namespace ET
{

    public class PetHeChengResult
    {
        public PetInfo UpdatePet;
        public PetInfo DeletePet;
        public int PetID;
        public int PetLv;
        public int PetExp;
        public int AddPropretyNum;
        public string AddPropretyValue;
        public bool IfBaby;
        public int ZiZhi_Hp;
        public int ZiZhi_Act;
        public int ZiZhi_MageAct;
        public int ZiZhi_Def;
        public int ZiZhi_Adf;
        public int ZiZhi_ActSpeed;
        public float ZiZhi_ChengZhang;
        public List<int> SavePetSkillID = new List<int>();
    }

    public class PetComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {

        public long FightPetId = 0;
        public List<PetInfo> PetInfos = new List<PetInfo>();

        public List<long> PetFormations = new List<long>() { };     //宠物副本
    }
}
