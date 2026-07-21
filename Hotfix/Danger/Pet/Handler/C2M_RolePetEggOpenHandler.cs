using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RolePetEggOpenHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetEggOpen, M2C_RolePetEggOpen>
    {
        protected override async ETTask Run(Unit unit, C2M_RolePetEggOpen request, M2C_RolePetEggOpen response, Action reply)
        {
            PetComponentServer pet = unit.GetComponent<PetComponentServer>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            TaskComponentServer task = unit.GetComponent<TaskComponentServer>();
            RolePetEgg rolePetEgg = pet.RolePetEggs[request.Index];
            if (rolePetEgg.ItemId == 0)
            {
                reply();
                return;
            }

            LDItem ldItemConf = LDItemCategory.Instance.Get(rolePetEgg.ItemId);
            string[] petinfos =  null;//gemitemCof.ItemUsePar.Split('@');
            int needCost = CommonHelper.ReturnPetOpenTimeDiamond(rolePetEgg.ItemId,rolePetEgg.EndTime);

            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            if (roleInfo.Diamond < needCost)
            {
                response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                reply();
                return;
            }
            roleInfoComponent.UpdateRoleMoneySub(UserDataType.Diamond, (needCost * -1).ToString(), true,ItemGetWay.PetChouKa);
            List<int> weights = new List<int>();
            List<int> petlists = new List<int>();
            for (int i = 2; i < petinfos.Length; i++)
            {
                string[] petitem = petinfos[i].Split(';');
                petlists.Add(int.Parse(petitem[0]));
                weights.Add(int.Parse(petitem[1]));
            }
            int index = RandomHelper.RandomByWeight(weights);
            if (petlists.Count <= index)
            {
                index = 0;
            }
            response.PetInfo = pet.OnAddPet(ItemGetWay.PetEggDuiHuan, petlists[index],0, rolePetEgg.FuLing);
            task.OnPetEggOpen(rolePetEgg.ItemId);

            rolePetEgg.ItemId = 0;
            rolePetEgg.EndTime = 0;
            rolePetEgg.FuLing = 0;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
