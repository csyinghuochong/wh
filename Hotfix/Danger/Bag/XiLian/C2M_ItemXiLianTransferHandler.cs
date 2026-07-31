using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemXiLianTransferHandler : AMActorLocationRpcHandler<Unit, C2M_ItemXiLianTransferRequest, M2C_ItemXiLianTransferResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemXiLianTransferRequest request, M2C_ItemXiLianTransferResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo bagInfo_1 = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID_1);
            BagInfo bagInfo_2 = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID_2);
            if (bagInfo_1 == null || bagInfo_2 == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            //判断品质
            LDItem ldItemConfig0 = LDItemCategory.Instance.Get(bagInfo_1.ItemID);
            LDItem ldItemConfig1 = LDItemCategory.Instance.Get(bagInfo_2.ItemID);

            bool all60green = ldItemConfig0.UseLv >= 60 && ldItemConfig0.Quality >= 5 && ldItemConfig1.UseLv >= 60 && ldItemConfig1.Quality >= 5;


            //绑定装备无法转移(客户端已经给出对应提示)
            if (bagInfo_1.IsBinging == true && bagInfo_2.IsBinging == false && ldItemConfig1.Quality == 4)
            {
                bagInfo_2.IsBinging = true;
            }

            //紫色品质以上才可以转移
            if (ldItemConfig0.Quality < 4 || ldItemConfig1.Quality < 4)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            //相同部位  只有护甲类型相同的装备才能转移
            if (!all60green)
            {
               
            }

            if (!all60green)
            {
                //相同部位  只有相同部位的装备才能转移
                if (ldItemConfig0.ItemType != ldItemConfig1.ItemType)
                {
                    response.Error = ErrorCode.Pre_Condition_Error;
                    reply();
                    return;
                }
            }

            string costItem = LDGlobalValueCategory.Instance.Get(51).Value;
            if (!bagComponentServer.OnCostItemData(costItem, ItemLocType.ItemLocBag, ItemGetWay.ItemXiLian))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            try
            {
             
            }
            catch (Exception ex)
            {
                Console.WriteLine("C2M_ItemXiLianTransferRequest: " + ex.ToString());
                Console.WriteLine("C2M_ItemXiLianTransferRequest: " + unit.Id);
            }

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            //通知客户端背包道具发生改变
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo_1);
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo_2);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
