using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_FashionWearHandler : AMActorLocationRpcHandler<Unit, C2M_FashionWearRequest, M2C_FashionWearResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FashionWearRequest request, M2C_FashionWearResponse response, Action reply)
        {

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int fashionId = request.FashionId;

            // 穿戴规则只有一份，与 GM 命令共用 BagComponentServer.ApplyFashionWear
            response.Error = bagComponentServer.ApplyFashionWear(fashionId, request.OperatateType, roleInfo.Occ, roleInfo.Sex);
            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"C2M_FashionWearRequest failed FashionId={fashionId} Operate={request.OperatateType} Error={response.Error}");
                reply();
                return;
            }

            UnitHelper.BroadcastUnitAppear(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
