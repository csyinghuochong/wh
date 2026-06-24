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
            RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
            if (rolePetInfo == null)
            {
                response.Error = ErrorCode.ERR_Pet_NoExist;
                reply();
                return;
            }

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            //通知客户端背包道具发生改变
            m2c_bagUpdate.BagInfoUpdate = new List<BagInfo>();

            long takeOffId = 0;
            if (request.OperateType == 1) //穿戴
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoId);
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }
                LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
                if (rolePetInfo.PetLv < ldItem.UseLv)
                {
                    response.Error = ErrorCode.ERR_LevelIsNot;
                    reply();
                    return;
                }

                int itemSubType = ldItem.ItemType;
                for (int i = rolePetInfo.PetEquipList.Count - 1; i >= 0; i--)
                { 
                    BagInfo petequipInfo = bagComponentServer.GetItemByLoc(ItemLocType.PetLocEquip, rolePetInfo.PetEquipList[i]);
                    if (petequipInfo == null)
                    {
                        rolePetInfo.PetEquipList.RemoveAt(i);   
                    }
                    if(LDItemCategory.Instance.Get(petequipInfo.ItemID).ItemType == itemSubType)
                    {
                        takeOffId = rolePetInfo.PetEquipList[i];
                        break;
                    }
                }
            }
            if (request.OperateType == 2)
            {
                takeOffId = request.BagInfoId;
            }

            //先卸下
            if (takeOffId != 0)
            {
                BagInfo oldBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.PetLocEquip, takeOffId);
                if (oldBagInfo != null)
                {
                    bagComponentServer.OnChangeItemLoc(oldBagInfo, ItemLocType.ItemLocBag, ItemLocType.PetLocEquip);
                    m2c_bagUpdate.BagInfoUpdate.Add(oldBagInfo);
                    rolePetInfo.PetEquipList.Remove(takeOffId);
                }

                petComponentServer.RemoveEquipSkill(rolePetInfo, oldBagInfo);
            }

            if (request.OperateType == 1) //穿戴
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoId);

                //新的装备给宠物
                bagComponentServer.OnChangeItemLoc(bagInfo, ItemLocType.PetLocEquip, ItemLocType.ItemLocBag);
                m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
                rolePetInfo.PetEquipList.Add(request.BagInfoId);
            }
            petComponentServer.UpdatePetAttribute(rolePetInfo, false);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            response.RolePetInfo = rolePetInfo;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
