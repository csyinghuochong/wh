using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_YueKaOpenHandler : AMActorLocationRpcHandler<Unit, C2M_YueKaOpenRequest, M2C_YueKaOpenResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_YueKaOpenRequest request, M2C_YueKaOpenResponse response, Action reply)
        {
            int cost = int.Parse( LDGlobalValueCategory.Instance.Get(37).Value );
            //判定是否是月卡用户
            if (unit.IsYueKaStates())
            {
                response.Error = ErrorCode.ERR_RoleYueKaRepeat;
                reply();
                return;
            }
            //判定是否钻石足够
            if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Diamond < cost)
            {
                response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                reply();
                return;
            }

            //开启月卡
            unit.UpdateYueKaTimes();

            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneySub(UserDataType.Diamond, (cost * -1).ToString(), true, ItemGetWay.CostItem);

            long addPilao = int.Parse(LDGlobalValueCategory.Instance.Get(26).Value) - int.Parse(LDGlobalValueCategory.Instance.Get(10).Value);
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.PiLao, addPilao.ToString());
            //Log.Warning($"[增加疲劳] {unit.DomainZone()}  {unit.Id}   {0}  {addPilao}");
            reply();
            await ETTask.CompletedTask;
        }

    }
}
