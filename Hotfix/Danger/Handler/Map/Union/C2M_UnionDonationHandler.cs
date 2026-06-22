using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionDonationHandler : AMActorLocationRpcHandler<Unit, C2M_UnionDonationRequest, M2C_UnionDonationResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionDonationRequest request, M2C_UnionDonationResponse response, Action reply)
        {
            //获取家族等级
            long unionid = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionid == 0)
            {
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Donation, unit.Id))
            {
                if (request.Type == 0) // 金币捐献
                {
                    if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.UnionDonationNumber) >= 5)
                    {
                        response.Error = ErrorCode.ERR_TimesIsNot;
                        reply();
                        return;
                    }

                    long selfgold = unit.GetComponent<RoleInfoComponent>().RoleInfo.Gold;
                    U2M_UnionOperationResponse responseUnionEnter = (U2M_UnionOperationResponse)await ActorMessageSenderComponent.Instance.Call(
                        DBHelper.GetUnionServerId(unit.DomainZone()),
                        new M2U_UnionOperationRequest() { OperateType = 3, UnitId = unit.Id, UnionId = unionid, Par = selfgold.ToString() });


                    if (responseUnionEnter.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = responseUnionEnter.Error;
                        reply();
                        return;
                    }

                    int unionID = int.Parse(responseUnionEnter.Par);
                    LDUnion ldUnionCof = LDUnionCategory.Instance.Get(unionID);
                    unit.GetComponent<NumericComponent>().ApplyChange(unit, NumericType.UnionDonationNumber, 1, 0);
                  
                    /*roleInfoComponent.UpdateRoleMoneySub(UserDataType.Gold, (ldUnionCof.DonateGold * -1).ToString(), true, ItemGetWay.Donation);
                    int randNumExp = RandomHelper.RandomNumber(ldUnionCof.DonateExp[0], ldUnionCof.DonateExp[1] + 1);
                    int randNumGongXian = RandomHelper.RandomNumber(ldUnionCof.DonateReward[0], ldUnionCof.DonateReward[1] + 1);
                    int randUnionGold = RandomHelper.RandomNumber(ldUnionCof.AddUnionGold[0], ldUnionCof.AddUnionGold[1] + 1);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionContri, randNumGongXian.ToString(), true, ItemGetWay.Donation);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionExp, randNumExp.ToString(), true, ItemGetWay.Donation);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionGold, randUnionGold.ToString(), true, ItemGetWay.Donation);*/
                }
                else if (request.Type == 1) // 钻石捐献
                {
                    if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.UnionDiamondDonationNumber) >= 10)
                    {
                        response.Error = ErrorCode.ERR_TimesIsNot;
                        reply();
                        return;
                    }

                    long selfDiamond = unit.GetComponent<RoleInfoComponent>().RoleInfo.Diamond;
                    U2M_UnionOperationResponse responseUnionEnter = (U2M_UnionOperationResponse)await ActorMessageSenderComponent.Instance.Call(
                        DBHelper.GetUnionServerId(unit.DomainZone()),
                        new M2U_UnionOperationRequest() { OperateType = 4, UnitId = unit.Id, UnionId = unionid, Par = selfDiamond.ToString() });

                    if (responseUnionEnter.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = responseUnionEnter.Error;
                        reply();
                        return;
                    }

                    int unionID = int.Parse(responseUnionEnter.Par);
                    LDUnion ldUnionCof = LDUnionCategory.Instance.Get(unionID);
                    unit.GetComponent<NumericComponent>().ApplyChange(unit, NumericType.UnionDiamondDonationNumber, 1, 0);
                    // 花费250钻石，暂时写死，M2U_UnionOperationRequest也是
                    RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();

                    /*roleInfoComponent.UpdateRoleMoneySub(UserDataType.Diamond, (ldUnionCof.DonateDiamond * -1).ToString(), true, ItemGetWay.Donation);
                    int randNumExp = RandomHelper.RandomNumber(ldUnionCof.DonateExp[0], ldUnionCof.DonateExp[1] + 1);
                    int randNumGongXian = RandomHelper.RandomNumber(ldUnionCof.DonateReward[0], ldUnionCof.DonateReward[1] + 1);
                    int randUnionGold = RandomHelper.RandomNumber(ldUnionCof.AddUnionGold[0], ldUnionCof.AddUnionGold[1] + 1);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionContri, randNumGongXian.ToString(), true, ItemGetWay.Donation);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionExp, randNumExp.ToString(), true, ItemGetWay.Donation);
                    roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.UnionGold, randUnionGold.ToString(), true, ItemGetWay.Donation);*/
                }
            }



            reply();
            await ETTask.CompletedTask;
        }
    }
}
