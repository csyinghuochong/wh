using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_RolePetHeXinHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetHeXin, M2C_RolePetHeXin>
    {
        protected override async ETTask Run(Unit unit, C2M_RolePetHeXin request, M2C_RolePetHeXin response, Action reply)
        {
            try
            {
                //通知客户端背包刷新
                M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
                //通知客户端背包道具发生改变
                m2c_bagUpdate.BagInfoUpdate = new List<BagInfo>();

                PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
                if (rolePetInfo == null)
                {
                    response.Error = ErrorCode.ERR_Pet_NoExist;
                    reply();
                    return;
                }

                //旧的返回到背包
                long oldItemId = rolePetInfo.PetHeXinList[request.Position];
                if (oldItemId != 0)
                {
                    BagInfo oldBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemPetHeXinEquip, oldItemId);
                    if (oldBagInfo != null)
                    {
                        bagComponentServer.OnChangeItemLoc(oldBagInfo, ItemLocType.ItemPetHeXinBag, ItemLocType.ItemPetHeXinEquip);
                        m2c_bagUpdate.BagInfoUpdate.Add(oldBagInfo);
                        rolePetInfo.PetHeXinList[request.Position] = 0;
                    }
                }
                if (request.OperateType == 1) //1 装备  2卸下[前面已经处理过了]
                {
                    ItemLocType itemLocType = request.OperateType == 1 ? ItemLocType.ItemPetHeXinBag : ItemLocType.ItemPetHeXinEquip;
                    BagInfo bagInfo = bagComponentServer.GetItemByLoc(itemLocType, request.BagInfoId);
                    if (bagInfo == null)
                    {
                        reply();
                        return;
                    }

                    //新的装备给宠物
                    bagComponentServer.OnChangeItemLoc(bagInfo, ItemLocType.ItemPetHeXinEquip, ItemLocType.ItemPetHeXinBag);
                    m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
                    rolePetInfo.PetHeXinList[request.Position] = request.BagInfoId;
                }
                petComponentServer.UpdatePetAttribute(rolePetInfo, true);
                MessageHelper.SendToClient(unit, m2c_bagUpdate);

                response.RolePetInfo = rolePetInfo;
                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
