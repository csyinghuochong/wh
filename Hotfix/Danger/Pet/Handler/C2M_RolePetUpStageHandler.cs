using System;

namespace ET
{

    //宠物进化
    [ActorMessageHandler]
    public class C2M_RolePetUpStageHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetUpStage, M2C_RolePetUpStage>
    {
        protected override async ETTask Run(Unit unit, C2M_RolePetUpStage request, M2C_RolePetUpStage response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);

            if (rolePetInfo ==null || request.PetInfoXianJiId <= 0) 
            {
                response.Error = ErrorCode.ERR_Pet_UpStage;
                reply();
                return;
            }

            //神兽不能进化
            LDPet ldPetCof = LDPetCategory.Instance.Get(rolePetInfo.ConfigId);
           

            RolePetInfo rolePetInfoXianJi = petComponentServer.GetPetInfo(request.PetInfoXianJiId);

            //判断当前宠物是否是进阶中的状态
            if (rolePetInfo.UpStageStatus == 1 || rolePetInfo.UpStageStatus == 0 && rolePetInfo.PetLv >= 70)
            {
                if (rolePetInfo.UpStageStatus == 2)
                {
                    response.Error = ErrorCode.ERR_Pet_UpStage;
                    reply();
                    return; 
                }

                //判断当前宠物是否有献祭
                //BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                if (rolePetInfoXianJi != null)
                {
                    //移除宠物
                    petComponentServer.RemovePet(request.PetInfoXianJiId,2);
                    response.OldPetInfo = CommonHelper.DeepCopy<RolePetInfo>(rolePetInfo);

                    //获取评分
                    int pingfen = PetHelper.PetPingJia(rolePetInfoXianJi);
                    petComponentServer.UpdatePetStage(rolePetInfo, pingfen);

                    petComponentServer.CheckPetPingFen();
                    petComponentServer.CheckPetZiZhi();

                    response.NewPetInfo = rolePetInfo;
                }
                else {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                }
            }
            else {
                response.Error = ErrorCode.ERR_Pet_UpStage;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

