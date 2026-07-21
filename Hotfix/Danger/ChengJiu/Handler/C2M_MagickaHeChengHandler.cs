using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_MagickaHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_MagickaHeChengRequest, M2C_MagickaHeChengResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MagickaHeChengRequest request, M2C_MagickaHeChengResponse response, Action reply)
        {
            if (request.OperateBagID.Count != 3)
            {
                response.Error = ErrorCode.ERR_MagicHeCheng_1;
                reply();
                return; 
            }


            List<int> removeids = new List<int>();

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  

            for (int  i= 0;i < request.OperateBagID.Count; i++)
            { 
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID[i]);

                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                removeids.Add(bagInfo.ItemID);
            }

            if (removeids.Count != 3)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            int magiclevel = 0;

            foreach ( ( int key, List<int>  idlist) in CommonConfig.MagicHeChengList)
            {
                HashSet<int> idSet = new HashSet<int>(idlist);
                foreach (int itemid in removeids)
                {
                    if (!idSet.Contains(itemid))
                    {
                        continue;
                    }
                    if (magiclevel == 0)
                    {
                        magiclevel = key;
                        continue;
                    }
                    if (magiclevel != key)
                    {
                        //必须相同等级。
                        response.Error = ErrorCode.ERR_MagicHeCheng_2;
                        reply();
                        return;
                    }
                }
            }

            //已经最大等级了。
            if (magiclevel >= CommonConfig.MagicHeChengList.Count)
            {
                response.Error = ErrorCode.ERR_MagicHeCheng_3;
                reply();
                return;
            }

            //但是只有50%概率，50%随机刷新一个同等级魔能
            int newlevel = 0;
            if (RandomHelper.RandFloat01() < 0.5f)
            {
                newlevel = magiclevel;
            }
            else
            {
                newlevel = magiclevel + 1;
            }

            List<int> newmagicids = CommonConfig.MagicHeChengList[newlevel];
            int idindex =  RandomHelper.RandomNumber(0, newmagicids.Count);
            int newmagid = newmagicids[idindex];

            bagComponentServer.OnCostItemData(request.OperateBagID, ItemLocType.ItemLocBag);
            bagComponentServer.OnAddItemData($"{newmagid};1", $"{ItemGetWay.GemHeCheng}_{TimeHelper.ServerNow()}");

            List<BagInfo> bagInfos =  bagComponentServer.GetIdItemList(newmagid);
            if (bagInfos.Count > 0)
            {
                response.RewardList.Add(bagInfos[^1]);
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
