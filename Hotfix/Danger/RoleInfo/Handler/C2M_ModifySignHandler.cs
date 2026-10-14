using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ModifySignHandler : AMActorLocationRpcHandler<Unit, C2M_ModifySignRequest, M2C_ModifySignResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ModifySignRequest request, M2C_ModifySignResponse response, Action reply)
        {
            string sign = request.Sign ?? "";
            sign = sign.Trim();
            if (!StringHelper.IsSafeSqlString(sign))
            {
                response.Error = ErrorCode.ERR_UnSafeSqlString;
                reply();
                return;
            }

            if (sign.Length > 30)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                response.Message = "个性签名过长!";
                reply();
                return;
            }

            unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Sign = sign;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
