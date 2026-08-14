using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ExpToGoldHandler : AMActorLocationRpcHandler<Unit, C2M_ExpToGoldRequest, M2C_ExpToGoldResponse>
    {
        private static int expToGoldDropId;
        private static List<int> expToItemWeights;
        private static bool expToGoldCacheInit;

        private static void EnsureExpToGoldCache()
        {
            if (expToGoldCacheInit)
            {
                return;
            }

            expToGoldDropId = int.Parse(LDGlobalValueCategory.Instance.Get(81).Value.Split(';')[0]);
            expToItemWeights = new List<int>(CommonConfig.ExpToItemList.Count);
            for (int i = 0; i < CommonConfig.ExpToItemList.Count; i++)
            {
                expToItemWeights.Add(CommonConfig.ExpToItemList[i].KeyId);
            }

            expToGoldCacheInit = true;
        }

        protected override async ETTask Run(Unit unit, C2M_ExpToGoldRequest request, M2C_ExpToGoldResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
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
            if (bagComponentServer.IsBagFullByLoc((int)ItemLocType.ItemLocBag)) 
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            //低于20%经验无法兑换
            float costPro = 0.2f;
            if (request.OperateType == 2) {
                costPro = 0.3f;
            }
            LDExp_Lv ldExpCof = LDExp_LvCategory.Instance.Get(roleInfo.Lv);
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
                     roleInfoComponent.UpdateRoleData(UserDataType.Gold, sendGold.ToString(), true, 32);*/
                     //Log.Debug($"Gold:  {roleInfoComponent.Id} {sendGold} excharge");
                    break;
                case 2:
                    EnsureExpToGoldCache();
                    List<RewardItem> rewardItems = new List<RewardItem>();
                    DropHelper.DropIDToDropItem_2(expToGoldDropId, rewardItems);
                    bagComponentServer.OnAddItemData(rewardItems, String.Empty, $"{ItemGetWay.DuiHuan}_{TimeHelper.ServerNow()}");
                    break;
                case 0:
                    EnsureExpToGoldCache();
                    int index = RandomHelper.RandomByWeight(expToItemWeights);
                    bagComponentServer.OnAddItemData(CommonConfig.ExpToItemList[index].Value,  $"{ItemGetWay.DuiHuan}_{TimeHelper.ServerNow()}");
                    break;
                default:
                    break;
            }
            roleInfoComponentServer.UpdateRoleData(UserDataType.Exp, (costExp * -1).ToString());
            numericComponent.ApplyChange(null, NumericType.ExpToGoldTimes, 1, 0);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
