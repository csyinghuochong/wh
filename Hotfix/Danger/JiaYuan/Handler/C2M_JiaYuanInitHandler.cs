using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JiaYuanInitHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanInitRequest, M2C_JiaYuanInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanInitRequest request, M2C_JiaYuanInitResponse response, Action reply)
        {
            JiaYuanComponentServer jiaYuanComponentServer = await DBHelper.GetComponentCache<JiaYuanComponentServer>(unit.DomainZone(), request.MasterId);
            RoleInfoComponent roleInfoComponent = await DBHelper.GetComponentCache<RoleInfoComponent>(unit.DomainZone(), request.MasterId);
            if (unit.Id != request.MasterId)
            {

                long gateServerId = DBHelper.GetGateServerId(unit.DomainZone());
                G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                    (gateServerId, new T2G_GateUnitInfoRequest()
                    {
                        UserID = request.MasterId
                    });

                //玩家在线
                if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
                {
                    JiaYuanOperate jiaYuanOperate = new JiaYuanOperate();
                    jiaYuanOperate = new JiaYuanOperate();
                    jiaYuanOperate.OperateType = JiaYuanOperateType.Visit;
                    jiaYuanOperate.PlayerName = unit.GetComponent<RoleInfoComponent>().RoleInfo.Name;
                    M2M_JiaYuanOperateMessage opmessage = new M2M_JiaYuanOperateMessage()
                    {
                        JiaYuanOperate = jiaYuanOperate,
                    };
                    MessageHelper.SendToLocationActor(request.MasterId, opmessage);
                }
                else
                {
                    jiaYuanComponentServer.AddJiaYuanRecord(new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.Visit,
                        OperateId = 0,
                        PlayerName = unit.GetComponent<RoleInfoComponent>().RoleInfo.Name,
                        Time = TimeHelper.ServerNow(),
                    });
                    await DBHelper.SaveComponentCache(unit.DomainZone(), request.MasterId, jiaYuanComponentServer);
                }
            }
            else
            {
               
            }

            response.PlanOpenList = jiaYuanComponentServer.InitOpenList();
            response.PurchaseItemList = jiaYuanComponentServer.PurchaseItemList_7;
            response.LearnMakeIds = jiaYuanComponentServer.LearnMakeIds_7;
            response.JiaYuanPastureList = jiaYuanComponentServer.JiaYuanPastureList_7;
            response.JianYuanPlantList = jiaYuanComponentServer.JianYuanPlantList_7;
            response.JiaYuanProList = jiaYuanComponentServer.JiaYuanProList_7;
            response.JiaYuanDaShiTime = jiaYuanComponentServer.JiaYuanDaShiTime_1;
            response.JiaYuanPetList = jiaYuanComponentServer.JiaYuanPetList_2;

            response.JiaYuanLv = roleInfoComponent.RoleInfo.JiaYuanLv;
            response.MasterName = roleInfoComponent.RoleInfo.Name;
            reply();
        }
    }
}
