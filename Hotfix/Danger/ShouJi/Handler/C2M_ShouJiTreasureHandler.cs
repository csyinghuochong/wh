using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShouJiTreasureHandler : AMActorLocationRpcHandler<Unit, C2M_ShouJiTreasureRequest, M2C_ShouJiTreasureResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShouJiTreasureRequest request, M2C_ShouJiTreasureResponse response, Action reply)
        {
            ShoujiComponentServer shoujiComponentServer = unit.GetComponent<ShoujiComponentServer>();
            KeyValuePairInt keyValuePairInt = shoujiComponentServer.GetTreasureInfo(request.ShouJiId);
            ShouJiItemConfig shouJiItemConfig = ShouJiItemConfigCategory.Instance.Get(request.ShouJiId);
            if (keyValuePairInt != null && keyValuePairInt.Value > shouJiItemConfig.AcitveNum)
            {
                response.Error = ErrorCode.ERR_ShouJIActived;
                reply();
                return;
            }

            List<long> huishouList = request.ItemIds;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemUseError;
                    reply();
                    return;
                }
            }

            int curNumber   = keyValuePairInt!=null ? (int)keyValuePairInt.Value : 0;  
            int needNumber  = shouJiItemConfig.AcitveNum - curNumber;
            
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                if (bagInfo == null)
                {
                    continue;
                }

                if (needNumber < bagInfo.ItemNum)
                {
                    curNumber += needNumber;
                    bagComponentServer.OnCostItemData(huishouList[i], needNumber);
                }
                else
                {
                    needNumber -= bagInfo.ItemNum;
                    curNumber += bagInfo.ItemNum;
                    bagComponentServer.OnCostItemData(huishouList[i], bagInfo.ItemNum);
                }

                if (curNumber >= needNumber)
                {
                    break;
                }
            }
           
            shoujiComponentServer.OnShouJiTreasure(request.ShouJiId, curNumber);
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            response.ActiveNum = curNumber;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
