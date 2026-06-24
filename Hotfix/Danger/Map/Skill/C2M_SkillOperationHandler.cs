using System;

namespace ET
{
	//技能通用操作
    [ActorMessageHandler]
    public class C2M_SkillOperationHandler : AMActorLocationRpcHandler<Unit, C2M_SkillOperation, M2C_SkillOperation>
    {
		protected override async ETTask Run(Unit unit, C2M_SkillOperation request, M2C_SkillOperation response, Action reply)
		{
            //request.OperationType  = 1 重置技能点
            //request.OperationType  = 2 重置职业
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            int level = roleInfoComponent.RoleInfo.Lv;
			int sp = roleInfoComponent.RoleInfo.Sp;
			switch (request.OperationType)
			{
				case 1:
                    LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(20);
                    int needGold = int.Parse(ldGlobalValue.Value);
                    roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
                    if (roleInfoComponent.RoleInfo.Gold < needGold)
                    {
                        response.Error = ErrorCode.ERR_GoldNotEnoughError;
                        reply();
                        return;
                    }

                    roleInfoComponent.UpdateRoleMoneySub(UserDataType.Gold, (needGold * -1).ToString());
					roleInfoComponent.UpdateRoleData(UserDataType.Sp, (level - sp).ToString());
					unit.GetComponent<SkillSetComponent>().OnSkillReset(true);
					break;
				case 2:

                    int toOcc = 0;
                    try
                    {
                        toOcc = int.Parse(request.OperationValue);
                    }
                    catch (Exception ex)
                    { 
                        Log.Error(ex);
                        response.Error = ErrorCode.ERR_Parameter;
                        reply();
                        return;
                    }

                    if (!LDOccupation_TransferCategory.Instance.Contain(toOcc))
                    {
                        Log.Error($"C2M_ChangeOccTwoRequest.1");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }


                    string ChangeOccItem = "10000178;1";
                    BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  
                    if (!bagComponentServer.CheckNeedItem(ChangeOccItem))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    if (roleInfoComponent.RoleInfo.OccTwo != 0)
                    {
                        unit.GetComponent<SkillSetComponent>().OnChangeJueXing(roleInfoComponent.RoleInfo.OccTwo, toOcc);
                        roleInfoComponent.RoleInfo.OccTwoOld.Add(roleInfoComponent.RoleInfo.OccTwo);
                    }

                    sp = unit.GetComponent<SkillSetComponent>().OnOccReset();
					roleInfoComponent.UpdateRoleData(UserDataType.Sp, sp.ToString());
                    bagComponentServer.OnCostItemData(ChangeOccItem, ItemLocType.ItemLocBag, ItemGetWay.SkillMake);
                    
                    unit.GetComponent<SkillSetComponent>().OnChangeOccTwoRequest(toOcc);
                    unit.GetComponent<SkillSetComponent>().AsyncUpdateSkillSet().Coroutine();
                    break;
                case 3:
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.SkillMakePlan2, 1);
                    break;
                case 4:
                    //unit.GetComponent<NumericComponent>().ApplyValue(NumericType.GemWarehouseOpen, 1);
                    break;
			}

			reply();
			await ETTask.CompletedTask;
		}
	}
}
