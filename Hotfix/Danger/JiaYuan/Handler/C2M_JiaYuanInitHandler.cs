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
            int masterHomeZone = UnitZoneHelper.GetHomeZone(request.MasterId);
            JiaYuanComponentServer jiaYuanComponentServer = await DBHelper.GetComponent<JiaYuanComponentServer>(masterHomeZone, request.MasterId);
            RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(masterHomeZone, request.MasterId);
            if (jiaYuanComponentServer == null || roleInfoComponentServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            if (unit.Id != request.MasterId)
            {
                string playerName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;

                JiaYuanOperate jiaYuanOperate = new JiaYuanOperate();
                jiaYuanOperate.OperateType = JiaYuanOperateType.Visit;
                jiaYuanOperate.PlayerName = playerName;
                M2M_JiaYuanOperateRequest opRequest = new M2M_JiaYuanOperateRequest() { JiaYuanOperate = jiaYuanOperate };
                M2M_JiaYuanOperateResponse opResponse = (M2M_JiaYuanOperateResponse)await MessageHelper.CallLocationActor(request.MasterId, opRequest);
                if (opResponse.Error != ErrorCode.ERR_Success)
                {
                    jiaYuanComponentServer.AddJiaYuanRecord(new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.Visit,
                        OperateId = 0,
                        PlayerName = playerName,
                        Time = TimeHelper.ServerNow(),
                    });
                    await DBHelper.SaveComponent(masterHomeZone, request.MasterId, jiaYuanComponentServer);
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

            response.JiaYuanLv = roleInfoComponentServer.RoleInfo.JiaYuanLv;
            response.MasterName = roleInfoComponentServer.RoleInfo.Name;
            reply();
        }
    }
}
