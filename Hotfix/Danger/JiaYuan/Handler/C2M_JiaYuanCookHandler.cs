using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanCookHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanCookRequest, M2C_JiaYuanCookResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanCookRequest request, M2C_JiaYuanCookResponse response, Action reply)
        {
            List<long> huishouList = request.BagInfoIds;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            if (huishouList.Count < 2)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            int totallv = 0;
            List<int> itemIdList = new List<int>();
            Dictionary<int,int> itemNumber = new Dictionary<int,int>(); 
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                //查看背包是否为空
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    break;
                }
                //道具配置
                LDItem ldItem =LDItemCategory.Instance.Get(bagInfo.ItemID);
                totallv += (ldItem.UseLv);
                itemIdList.Add(bagInfo.ItemID);

                if (!itemNumber.ContainsKey(bagInfo.ItemID))
                {
                    itemNumber.Add(bagInfo.ItemID, 0);
                }
                itemNumber[bagInfo.ItemID] ++;
            }
            foreach (( int itemid, int itemnum ) in itemNumber)
            {
                if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item,itemid) < itemnum)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            if (response.Error != ErrorCode.ERR_Success)
            {
                reply();
                return;
            }

            //激活逻辑
            int getItemid = 0;
            bool ifActiveMake = false;
            int makeid = EquipMakeConfigCategory.Instance.GetCanMakeId(itemIdList);
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            if (makeid > 0 )
            {
                /*
                if (RandomHelper.RandFloat01() >= 0.1f)
                {
                */
                //制作成功


                if (!jiaYuanComponentServer.LearnMakeIds_7.Contains(makeid))
                {
                    jiaYuanComponentServer.LearnMakeIds_7.Add(makeid);
                }

                getItemid = EquipMakeConfigCategory.Instance.Get(makeid).MakeItemID;
                ifActiveMake = true;

                if (!jiaYuanComponentServer.LearnMakeIds_7.Contains(getItemid))
                {
                    response.LearnId = getItemid;
                    jiaYuanComponentServer.LearnMakeIds_7.Add(getItemid);
                }
                /*
                }
                else
                {
                    //制作失败
                    getItemid = 10036101;
                }
                */
            }

            //随机
            if (makeid == 0 && ifActiveMake == false)
            {
                if (RandomHelper.RandFloat01() >= 0.5f)
                {
                    int randLvMax = Mathf.CeilToInt(totallv * 1f / 4);
                    int randLv = RandomHelper.RandomNumber((int)(randLvMax * 0.5f), randLvMax + 1);
                    if (randLv < 1) {
                        randLv = 1;
                    }
                    getItemid = LDItemCategory.Instance.GetFoodId(randLv);
                    if (getItemid == 0)
                    {
                        reply();
                        return;
                    }
                }
                else {
                    //制作失败
                    getItemid = 10036102;
                }
            }

            for (int i = 0; i < huishouList.Count; i++)
            {
                bagComponentServer.OnCostItemData(huishouList[i],1);
            }
            bagComponentServer.OnAddItemData($"{getItemid};1", $"{ItemGetWay.JiaYuanCook}_{TimeHelper.ServerNow()}");
            response.LearnMakeIds = jiaYuanComponentServer.LearnMakeIds_7;
            response.ItemId = getItemid;
            DBHelper.SaveComponentCache(unit.DomainZone(), unit.Id, jiaYuanComponentServer).Coroutine();
            unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanCooking_403, 0, 1);
            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanCookNumber_91, 0, 1);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
