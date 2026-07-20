using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ExpToGoldHandler : AMActorLocationRpcHandler<Unit, C2M_ExpToGoldRequest, M2C_ExpToGoldResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ExpToGoldRequest request, M2C_ExpToGoldResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            ServerInfo serverInfo = ConfigData.ServerInfoList[UnitZoneHelper.GetHomeZone(unit)];
            if (roleInfo.Lv < 70 &&  roleInfo.Lv < serverInfo.WorldLv)
            {
                response.Error = ErrorCode.ERR_LevelNoEnough;
                reply();
                return;
            }

            //满级经验兑换效验等级
            //GlobalValueConfig globalCof = GlobalValueConfigCategory.Instance.Get(41);
            //if (request.OperateType == 2)
            //{
            //    if (roleInfo.Level < globalCof.Value2)
            //    {
            //        response.Error = ErrorCode.ERR_ExpNoEnough;
            //        reply();
            //        return;
            //    }
            //}

            if (roleInfo.Lv != 70 && roleInfo.Lv != 75  )
            {
                response.Error = ErrorCode.ERR_LevelNoEnough;
                reply();
                return;
            }

            //背包已满
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.IsBagFull()) {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            //低于20%经验无法兑换
            float costPro = 0.2f;
            if (request.OperateType == 2) {
                costPro = 0.3f;
            }
            LDExp ldExpCof = LDExpCategory.Instance.Get(roleInfo.Lv);
            int costExp = (int)(ldExpCof.Exp_Role * costPro);
            if (roleInfo.Exp < costExp||costExp <= 0)
            {
                response.Error = ErrorCode.ERR_ExpNoEnough;
                reply();
                return;
            }

            switch (request.OperateType)
            {
                case 3:
                     /*int sendGold = (int)(10000 + expCof.Exp_Role * 10);
                     sendGold = (int)(10000 + expCof.Exp_Role * 10);
                     roleInfoComponent.UpdateRoleMoneyAdd(UserDataType.Gold, sendGold.ToString(), true, 32);*/
                     //Log.Debug($"Gold:  {roleInfoComponent.Id} {sendGold} excharge");
                    break;
                case 2:
                    string[] droplist = LDGlobalValueCategory.Instance.Get(81).Value.Split(';');
                    int dropid = int.Parse(droplist[0]);
                    List<RewardItem> rewardItems = new List<RewardItem>();
                    DropHelper.DropIDToDropItem_2(dropid, rewardItems);
                    bagComponentServer.OnAddItemData(rewardItems, String.Empty, $"{ItemGetWay.DuiHuan}_{TimeHelper.ServerNow()}");
                    break;
                case 0:
                    List<int> weights = ListComponent<int>.Create();
                    for (int i = 0; i < CommonConfig.ExpToItemList.Count; i++)
                    {
                        weights.Add(CommonConfig.ExpToItemList[i].KeyId);
                    }
                    int index = RandomHelper.RandomByWeight(weights);
                    bagComponentServer.OnAddItemData(CommonConfig.ExpToItemList[index].Value,  $"{ItemGetWay.DuiHuan}_{TimeHelper.ServerNow()}");
                    break;
                default:
                    break;
            }
            roleInfoComponentServer.UpdateRoleData(UserDataType.Exp, (costExp * -1).ToString());
            unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.ExpToGoldTimes, 1, 0);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
