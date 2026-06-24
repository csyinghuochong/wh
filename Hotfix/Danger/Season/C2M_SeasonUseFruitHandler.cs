using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_SeasonUseFruitHandler : AMActorLocationRpcHandler<Unit, C2M_SeasonUseFruitRequest, M2C_SeasonUseFruitResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_SeasonUseFruitRequest request, M2C_SeasonUseFruitResponse response, Action reply)
        {
            long reduceTime = 0;
            List<long> huishouList = request.BagInfoIDs;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

            if (request.BagInfoIDs.Count <= 0)
            {
                Log.Error($"C2M_SeasonUseFruitRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
            }
            
            // for (int i = 0; i < huishouList.Count; i++)
            // {
            //     BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
            //     if (bagInfo == null)
            //     {
            //         continue;
            //     }
            //
            //     Item Item = ItemCategory.Instance.Get( bagInfo.ItemID );
            //     if (Item.ItemType != ItemTypeEnum.Consume ||  Item.ItemSubType != 132 )
            //     {
            //         continue;
            //     }
            //
            //     reduceTime += long.Parse(Item.ItemUsePar);
            // }

            BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[0]);
            if (bagInfo == null)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            if (ldItem.ItemType != ItemTypeEnum.Consume || ldItem.ItemType != 132)
            {
                Log.Error($"C2M_SeasonUseFruitRequest 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            reduceTime += long.Parse(ldItem.ItemUsePar);

            bagComponentServer.OnCostItemData(request.BagInfoIDs[0], 1);
            unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.SeasonBossRefreshTime, -1 * reduceTime, 0);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
