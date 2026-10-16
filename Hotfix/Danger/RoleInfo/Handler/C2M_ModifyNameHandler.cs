using System;
using System.Collections.Generic;

namespace ET
{
    //修改名字
    [ActorMessageHandler]
    public class C2M_ModifyNameHandler : AMActorLocationRpcHandler<Unit, C2M_ModifyNameRequest, M2C_ModifyNameResponse>
    {
     

        protected override async ETTask Run(Unit unit, C2M_ModifyNameRequest request, M2C_ModifyNameResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            Log.Warning($"C2M_ModifyNameRequest:  {unit.DomainZone()} {unit.Id}");
            if (!StringHelper.IsSafeSqlString(request.NewName))
            {
                response.Error = ErrorCode.ERR_UnSafeSqlString;
                reply();
                return;
            }
            
            if (string.IsNullOrEmpty(request.NewName))
            {
                response.Error = ErrorCode.ERR_CreateRoleName;
                response.Message = "角色名字过短!";
                reply();
                return;
            }

            if (request.NewName.Contains(" "))
            {
                Log.Error($"C2M_ModifyNameRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            request.NewName = request.NewName.Trim();   

            int maxNameLength = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Role_Name_Character_Length);
            if (request.NewName.Length > maxNameLength)
            {
                response.Error = ErrorCode.ERR_CreateRoleName;
                response.Message = "角色名字过长!";
                reply();
                return;
            }

            List<RoleInfoComponentServer> result = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(UnitZoneHelper.GetHomeZone(unit), _account => _account.UserName == request.NewName);
            if (result.Count > 0)
            {
                response.Error = ErrorCode.ERR_RoleNameRepeat;
                reply();
                return;
            }

            List<RewardItem> costs = ItemNewHelper.GetRewardItems(ReadGlobalCost(GlobalValueKey.Global_Role_Name_Reset_Consume));
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (costs.Count > 0 && !bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            roleInfo.UserName = request.NewName;
            roleInfo.UpdateRoleData(UserDataType.Name, request.NewName);
            M2C_RoleDataBroadcast m2C_BroadcastRoleData = new M2C_RoleDataBroadcast();
            m2C_BroadcastRoleData.UnitId = unit.Id;
            m2C_BroadcastRoleData.UpdateType = (int)UserDataType.Name;
            m2C_BroadcastRoleData.UpdateTypeValue = request.NewName;
            MessageHelper.Broadcast(unit, m2C_BroadcastRoleData);

            reply();
            await ETTask.CompletedTask;
        }

        static string ReadGlobalCost(string key)
        {
            string raw = LDGlobalValueCategory.Instance.GetByKey(key).Value;
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            raw = raw.Trim();
            if (raw.Length >= 2 && raw[0] == '{' && raw[raw.Length - 1] == '}')
            {
                return raw.Substring(1, raw.Length - 2).Trim();
            }

            return raw;
        }
    }
}
