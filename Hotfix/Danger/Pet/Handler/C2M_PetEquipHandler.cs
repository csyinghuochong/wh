using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetEquipHandler : AMActorLocationRpcHandler<Unit, C2M_PetEquipRequest, M2C_PetEquipResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetEquipRequest request, M2C_PetEquipResponse response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            PetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
            if (rolePetInfo == null)
            {
                response.Error = ErrorCode.ERR_Pet_NoExist;
                reply();
                return;
            }

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            long takeOffId = 0;
            BagInfo bagInfo = null;
            if (request.OperateType == 1) //穿戴
            {
                bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoId);
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }
                LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            }
            if (request.OperateType == 2)
            {
                takeOffId = request.BagInfoId;
            }

            //先卸下
            if (takeOffId != 0)
            {
                /*
                BagInfo oldBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.PetLocEquip, takeOffId);
                if (oldBagInfo != null)
                {
                    bagComponentServer.OnChangeItemLoc(oldBagInfo, ItemLocType.ItemLocBag, ItemLocType.PetLocEquip);
                    m2c_bagUpdate.BagInfoUpdate.Add(oldBagInfo);
                    rolePetInfo.PetEquipList.Remove(takeOffId);
                }
               
                petComponentServer.RemoveEquipSkill(rolePetInfo, oldBagInfo); */
            }

            if (request.OperateType == 1) //穿戴
            {
                //新的装备给宠物
                //bagComponentServer.OnChangeItemLoc(bagInfo, ItemLocType.PetLocEquip, ItemLocType.ItemLocBag);
                m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
                rolePetInfo.PetEquipList.Add(request.BagInfoId);
            }
            petComponentServer.UpdatePetAttribute(rolePetInfo, false);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            response.PetInfo = rolePetInfo;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
