using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_HappyMoveHandler : AMActorLocationRpcHandler<Unit, C2M_HappyMoveRequest, M2C_HappyMoveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HappyMoveRequest request, M2C_HappyMoveResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();

            if (request.OperatateType != 1 && request.OperatateType != 3)
            {
                Log.Error($"C2M_HappyMoveRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.OperatateType == 1)
            {
                //非免费时间则返回
                RoleContextComponent roleContext = unit.GetComponent<RoleContextComponent>();
                long happmoveTime = roleContext.HappyMoveTime;
                if (TimeHelper.ServerNow()  < happmoveTime)
                {
                    response.Error = ErrorCode.ERR_HappyMove_CD;
                    reply();
                    return;
                }

                long mianfeicd = LDGlobalValueCategory.Instance.TempValue * 1000;
                roleContext.SetHappyMoveTime(TimeHelper.ServerNow() + mianfeicd);
            }
            if (request.OperatateType == 2)
            {
                LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(94);
                if (roleInfoComponentServer.RoleInfo.Gold < LDGlobalValueCategory.Instance.TempValue)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }
                //roleInfoComponent.UpdateRoleData( UserDataType.Gold, (globalValue.Value2 * -1).ToString(), true, ItemGetWay.HappyMove);
            }
            if (request.OperatateType  == 3)
            {
                /*GlobalValue globalValue = GlobalValueCategory.Instance.Get(95);
                if (roleInfoComponent.RoleInfo.Diamond < globalValue.Value2)
                {
                    response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                    reply();
                    return;
                }
                roleInfoComponent.UpdateRoleData(UserDataType.Diamond, (globalValue.Value2 * -1).ToString(), true, ItemGetWay.HappyMove);*/
            }

            for (int r = 10; r > 0; r--)
            {
                int newCell = RandomHelper.RandomNumber(0, HappyFubenConfig.PositionList.Count);

                bool haveorange = false;
                List<Unit> droplist = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.DropItem);
                for (int i = 0; i < droplist.Count; i++)
                {
                    DropComponent dropComponent = droplist[i].GetComponent<DropComponent>();
                   
                }

                //遇到橙色道具真实随机率 30%在当前橙色格子
                if (haveorange && r > 1 && RandomHelper.RandFloat01() > 0.3f)
                {
                    continue;
                }

                Vector3 vector3 = HappyFubenConfig.PositionList[newCell];
                unit.Position = vector3;
                break;
            }

            unit.Stop(-2);
            reply();
            await ETTask.CompletedTask;
        }
    }
}