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
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            int level = roleInfo.Lv;
			int sp = roleInfo.Sp;
			switch (request.OperationType)
			{
				case 1:
                    int needGold = int.Parse(LDGlobalValueCategory.Instance.Get(20).Value);
                    if (roleInfo.Gold < needGold)
                    {
                        response.Error = ErrorCode.ERR_GoldNotEnoughError;
                        reply();
                        return;
                    }

                    roleInfoComponentServer.UpdateRoleMoneySub(UserDataType.Gold, (needGold * -1).ToString());
					roleInfoComponentServer.UpdateRoleData(UserDataType.Sp, (level - sp).ToString());
					skillSetComponentServer.OnSkillReset(true);
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

                    if (roleInfo.OccTwo != 0)
                    {
                        skillSetComponentServer.OnChangeJueXing(roleInfo.OccTwo, toOcc);
                        roleInfo.OccTwoOld.Add(roleInfo.OccTwo);
                    }

                    sp = skillSetComponentServer.OnOccReset();
					roleInfoComponentServer.UpdateRoleData(UserDataType.Sp, sp.ToString());
                    bagComponentServer.OnCostItemData(ChangeOccItem, ItemLocType.ItemLocBag, ItemGetWay.SkillMake);
                    
                    skillSetComponentServer.OnChangeOccTwoRequest(toOcc);
                    skillSetComponentServer.AsyncUpdateSkillSet().Coroutine();
                    break;
                case 3:
                    numeric.ApplyValue(NumericType.SkillMakePlan2, 1);
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
