using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetDuiHuanHandler : AMActorLocationRpcHandler<Unit, C2M_PetDuiHuanRequest, M2C_PetDuiHuanResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetDuiHuanRequest request, M2C_PetDuiHuanResponse response, Action reply)
        {
            PetComponent petComponent = unit.GetComponent<PetComponent>();
            int userLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
            if (PetHelper.GetBagPetNum(petComponent.RolePetInfos) >= PetHelper.GetPetMaxNumber(unit, userLv))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            int configId = request.OperateId;
            LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(configId);
            string[] configInfo = ldGlobalValue.Value.Split('@');
            if(configInfo.Length < 2)
            {
                Log.Error($"C2M_PetDuiHuanRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (!bagComponentServer.OnCostItemData(configInfo[0], ItemLocType.ItemLocBag, ItemGetWay.DuiHuan))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            response.RolePetInfo = petComponent.OnAddPet(ItemGetWay.PetEggDuiHuan, int.Parse(configInfo[1]));
            unit.GetComponent<DataCollationComponent>().OnPetDuiHuan();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
