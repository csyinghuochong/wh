using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JingLingCatchHandler : AMActorLocationRpcHandler<Unit, C2M_JingLingCatchRequest, M2C_JingLingCatchResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingLingCatchRequest request, M2C_JingLingCatchResponse response, Action reply)
        {

            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            Unit zhupuUnit = unitComponent.Get(request.JingLingId);
            if (zhupuUnit == null)
            {
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            if (request.ItemId != 0)
            {
                bool costresult =  bagComponentServer.OnCostItemData($"{request.ItemId};1", ItemLocType.ItemLocBag, ItemGetWay.JingLing);
                if (costresult == false)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            int gailv = CommonHelper.GetZhuPuGaiLv(zhupuUnit.ConfigId, request.ItemId, int.Parse(request.OperateType));
            if (RandomHelper.RandFloat01() <= gailv * 0.0001f)
            {
                response.Message = String.Empty;
                int skinId = zhupuUnit.GetComponent<NumericComponent>().GetAsInt(NumericType.PetSkin);

                LDMonster ldMonster = LDMonsterCategory.Instance.Get(zhupuUnit.ConfigId);
                int getItemid = -1;///ldMonster.Parameter[1];
                //bagComponentServer.OnAddItemData($"{getItemid};1",$"{ItemGetWay.PickItem}_{TimeHelper.ServerNow()}");

                List<BagInfo> bagInfolist = bagComponentServer.GetIdItemList(getItemid);
                if (bagInfolist.Count > 0)
                {
                    bagInfolist[bagInfolist.Count - 1].ItemPar = skinId.ToString();
                }
            }
            else
            {
                response.Error = ErrorCode.ERR_ZhuaBuFail;
            }

            unitComponent.Remove(request.JingLingId);
            reply();
            await ETTask.CompletedTask;
        }
    }

}