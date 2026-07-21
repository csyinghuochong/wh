using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanPastureBuyHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanPastureBuyRequest, M2C_JiaYuanPastureBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanPastureBuyRequest request, M2C_JiaYuanPastureBuyResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            int mysteryId = request.MysteryId;
            JiaYuanPastureConfig jiaYuanPastureConfig = JiaYuanPastureConfigCategory.Instance.Get(mysteryId);
            if (jiaYuanPastureConfig == null)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            if (mapComponent.MapTypeEnum != MapTypeEnum.JiaYuan)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }


            float jiagerate = 1f;
            if (request.BuyType == 1)
            {
                jiagerate = CommonHelper.JiaYuanPastureBuy();
            }

            if (!unit.GetComponent<BagComponentServer>().CheckNeedItem($"13;{(int)(jiaYuanPastureConfig.BuyGold * jiagerate)}"))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            LDHome ldHome = LDHomeCategory.Instance.Get(roleInfo.JiaYuanLv);

            if (jiaYuanPastureConfig.BuyJiaYuanLv > roleInfo.JiaYuanLv)
            {
                response.Error = ErrorCode.ERR_LvNoHigh;
                reply();
                return;
            }
            
            /*if (jiaYuanComponent.GetPeopleNumber() >= ldHome.PeopleNumMax)
            {
                response.Error = ErrorCode.ERR_PeopleNumber;
                reply();
                return;
            }
            if (jiaYuanComponent.GetPeopleNumber() + jiaYuanPastureConfig.PeopleNum > ldHome.PeopleNumMax)
            {
                response.Error = ErrorCode.ERR_PeopleNoEnough;
                reply();
                return;
            }*/

            if (request.ProductId != -1)
            {
                int errorCode = jiaYuanComponentServer.OnPastureBuyRequest(request.ProductId);
                if (errorCode != ErrorCode.ERR_Success)
                {
                    response.Error = errorCode;
                    reply();
                    return;
                }
            }

            roleInfoComponentServer.OnMysteryBuy(mysteryId);
            unit.GetComponent<BagComponentServer>().OnCostItemData($"13;{(int)(jiaYuanPastureConfig.BuyGold * jiagerate)}", ItemLocType.ItemLocBag, ItemGetWay.JiaYuanCost);
            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanPastureNumber_94, 0, 1);

            JiaYuanPastures jiaYuanPastures = new JiaYuanPastures()
            { 
                ConfigId = jiaYuanPastureConfig.Id,
                StartTime = TimeHelper.ServerNow(),
                UnitId = IdGenerater.Instance.GenerateId(), 
            };

            UnitFactory.CreatePasture(unit.DomainScene(), jiaYuanPastures, unit.Id);
            List<JiaYuanPastures> JiaYuanPastureList_3 = jiaYuanComponentServer.JiaYuanPastureList_7;
            JiaYuanPastureList_3.Add(jiaYuanPastures);

            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer ).Coroutine();
            response.JiaYuanPastureList = JiaYuanPastureList_3;
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}
