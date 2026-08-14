using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ConsignDuiHuanHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignDuiHuanRequest, M2C_ConsignDuiHuanResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ConsignDuiHuanRequest request, M2C_ConsignDuiHuanResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            long dbCacheId = DBHelper.GetRankServerId(unit);
            R2M_DBServerInfoResponse d2GGetUnit = (R2M_DBServerInfoResponse)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2R_DBServerInfoRequest() { });
            long diamond = request.DiamondsNumber;
            if (request.DiamondsNumber <= 0)
            {
                reply();
                return;
            }

            if (numericComponent.GetAsInt(NumericType.RechargeNumber) <= 0)
            {
                reply();
                return;
            }

            //服务器限制,单次最多兑换100000钻石
            if (request.DiamondsNumber > 100000)
            {
                reply();
                return;
            }
       
            //判断钻石是否足够
            if (roleInfo.RoleInfo.Diamond >= diamond)
            {
                roleInfo.UpdateRoleData(UserDataType.Diamond, (diamond * -1).ToString(), true, ItemGetWay.DuiHuan);
                roleInfo.UpdateRoleData(UserDataType.Gold, (diamond * d2GGetUnit.ServerInfo.ExChangeGold).ToString(), true, ItemGetWay.DuiHuan);
                taskComponentServer.OnDuiHuanGold((int)diamond);
            }
            else 
            {
                response.Error = ErrorCode.ERR_DiamondNotEnoughError;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
