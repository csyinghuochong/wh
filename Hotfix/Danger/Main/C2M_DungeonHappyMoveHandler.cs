using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_DungeonHappyMoveHandler : AMActorLocationRpcHandler<Unit, C2M_DungeonHappyMoveRequest, M2C_DungeonHappyMoveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_DungeonHappyMoveRequest request, M2C_DungeonHappyMoveResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();

            if (request.OperatateType != 1 && request.OperatateType != 2 && request.OperatateType != 3)
            {
                Log.Error($"C2M_DungeonHappyMoveRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            RoleDailyDataComponentServer dailyData = unit.GetComponent<RoleDailyDataComponentServer>();
            RoleContextComponent roleContext = unit.GetComponent<RoleContextComponent>();
            if (request.OperatateType == 1)
            {
                if (dailyData.GetHappyMoveNumber() >= 5)
                {
                    response.Error = ErrorCode.ERR_TimesIsNot;
                    reply();
                    return;
                }

                //非免费时间则返回
                long happmoveTime = roleContext.HappyMoveTime;
                if (TimeHelper.ServerNow() < happmoveTime)
                {
                    response.Error = ErrorCode.ERR_HappyMove_CD;
                    reply();
                    return;
                }

                long mianfeicd = TimeHelper.Second * 5 ;
                roleContext.SetHappyMoveTime(TimeHelper.ServerNow() + mianfeicd);
                dailyData.AddHappyMoveNumber();
            }
            if (request.OperatateType == 2)
            {
                /*GlobalValue globalValue = GlobalValueCategory.Instance.Get(94);
                if (roleInfoComponent.RoleInfo.Gold < globalValue.Value2)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }
                roleInfoComponent.UpdateRoleData(UserDataType.Gold, (globalValue.Value2 * -1).ToString(), true, ItemGetWay.HappyMove);*/
            }
            if (request.OperatateType == 3)
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

            Scene domainScene = unit.DomainScene();
            for (int r = 10; r > 0; r--)
            {
                int newCell = RandomHelper.RandomNumber(0, HappyFubenConfig.PositionList.Count);

                bool haveorange = false;
                List<Unit> droplist = UnitHelper.GetUnitList(domainScene, UnitType.DropItem);
                for (int i = 0; i < droplist.Count; i++)
                {
                    DropComponent dropComponent = droplist[i].GetComponent<DropComponent>();
                    int itemid = dropComponent.ItemID;
                    if (LDItemCategory.Instance.Get(itemid).Quality >= 5)
                    {
                        haveorange = true;
                        break;
                    }
                }
                
                //遇到橙色道具真实随机率 30%在当前橙色格子
                if (haveorange && r > 1 && RandomHelper.RandFloat01() > 0.3f)
                {
                    continue;
                }

                numeric.ApplyValue(NumericType.UnitCellIndex, newCell + 1);
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