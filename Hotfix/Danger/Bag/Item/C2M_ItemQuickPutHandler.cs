using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    /// <summary>
    /// 放入可以堆叠的道具
    /// </summary>

    [ActorMessageHandler]
    public class C2M_ItemQuickPutHandler : AMActorLocationRpcHandler<Unit, C2M_ItemQuickPutRequest, M2C_ItemQuickPutResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemQuickPutRequest request, M2C_ItemQuickPutResponse response, Action reply)
        {
            int hourseId = request.HorseId;
            if (!ItemNewHelper.IsValideBagLoc(hourseId) || hourseId == (int)ItemLocType.ItemLocBag)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            //if (hourseId < (int)ItemLocType.ItemWareHouse1 || hourseId > (int)ItemLocType.ItemWareHouse4)
            //{
            //    Log.Error($"C2M_ItemQuickPutRequest 1");
            //    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
            //    reply();
            //    return;
            //}

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  

            List<BagInfo> warehourselist = bagComponentServer.GetItemByLoc((ItemLocType)hourseId);

            List<BagInfo> bagList = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag);
            HashSet<long> processedBagIds = new HashSet<long>();

            for (int w = 0; w < warehourselist.Count; w++)
            {
                BagInfo warehourseInfo = warehourselist[w];
                LDItem ldItemCof = LDItemCategory.Instance.Get(warehourseInfo.ItemID);
                int pileSum = ldItemCof.ItemPileSum;

                for (int b = bagList.Count - 1; b >= 0; b-- )
                {
                    BagInfo bagInfo = bagList[b];
                    if ((int)ItemNewHelper.GetToItemLocType(bagInfo.ItemType, bagInfo.ItemID) != hourseId) continue;
                    if (processedBagIds.Contains(bagInfo.BagInfoID))
                    {
                        continue;
                    }

                    int warehouseFlags = warehourseInfo.IsDungeonTradable()
                            ? ItemTradeStatus.NonTradable : warehourseInfo.ItemFlags;
                    int bagFlags = bagInfo.IsDungeonTradable()
                            ? ItemTradeStatus.NonTradable : bagInfo.ItemFlags;
                    if (warehourseInfo.ItemID != bagInfo.ItemID || warehourseInfo.ItemType != bagInfo.ItemType
                            || warehouseFlags != bagFlags
                            || !ItemNewHelper.SameMakeInfo(warehourseInfo.MakePlayer, warehourseInfo.MakeTime, bagInfo.MakePlayer, bagInfo.MakeTime))
                    {
                        continue;
                    }
                    if ( (warehourseInfo.ItemNum + bagInfo.ItemNum) > pileSum)
                    {
                        continue;
                    }
                    warehourseInfo.BindOnOperate();
                    bagInfo.BindOnOperate();
                    warehourseInfo.ItemNum = warehourseInfo.ItemNum + bagInfo.ItemNum;
                    m2c_bagUpdate.BagInfoUpdate.Add(warehourseInfo);
                    m2c_bagUpdate.BagInfoDelete.Add(bagInfo);
                    processedBagIds.Add(bagInfo.BagInfoID);
                    bagList.RemoveAt(b);
                }
            }


            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
