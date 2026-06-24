using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionXiuLianHandler : AMActorLocationRpcHandler<Unit, C2M_UnionXiuLianRequest, M2C_UnionXiuLianResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionXiuLianRequest request, M2C_UnionXiuLianResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            int numerType = UnionHelper.GetXiuLianId(request.Position, request.Type);
            if (numerType == 0)
            {
                reply();
                return;
            }

            long unionid = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionid == 0)
            {
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int xiulianid = numericComponent.GetAsInt(numerType);

            int position = request.Position;
            if (request.Type == 1)
            {
                position += 4;
            }await ETTask.CompletedTask;
        
            //if (xiulianid >= UnionQiangHuaConfigCategory.Instance.GetMaxId(position))
            {
                response.Error = ErrorCode.ERR_UnionXiuLianMax;
                reply();
                return;
            }
            /*
            UnionQiangHuaConfig unionQiangHuaConfig = UnionQiangHuaConfigCategory.Instance.Get(xiulianid);
            if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.UnionZiJin < unionQiangHuaConfig.CostGold)
            {
                response.Error = ErrorCode.ERR_HouBiNotEnough;
                reply();
                return;
            }
            
            if (!unit.GetComponent<BagComponentServer>().OnCostItemData(unionQiangHuaConfig.CostItem, ItemLocType.ItemLocBag, ItemGetWay.UnionXiuLian))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
           
            long selfgold = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Gold;
            U2M_UnionOperationResponse responseUnionEnter = (U2M_UnionOperationResponse)await ActorMessageSenderComponent.Instance.Call(
                       DBHelper.GetUnionServerId(unit.DomainZone()),
                       new M2U_UnionOperationRequest() { OperateType = 3, UnitId = unit.Id, UnionId = unionid, Par = selfgold.ToString() });
            int unionLevel = int.Parse(responseUnionEnter.Par);
            LDUnion ldUnion = LDUnionCategory.Instance.Get(unionLevel);

            //Console.WriteLine($"unionConfig:  {unionLevel}  {unionConfig.XiuLianLevel} {unionQiangHuaConfig.QiangHuaLv}");
            if (unionQiangHuaConfig.QiangHuaLv >= ldUnion.Id)
            {
                reply();
                return; 
            }


            unit.GetComponent<NumericComponent>().ApplyValue( numerType, xiulianid+1);
            //unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneySub( UserDataType.UnionContri,(unionQiangHuaConfig.CostGold * -1).ToString(), true, ItemGetWay.UnionXiuLian);

            //刷新角色属性
            Function_Fight.UnitUpdateProperty_Base(unit,true,true);
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();  
            for (int i = petComponentServer.RolePetInfos.Count - 1; i >= 0; i--)
            {
                petComponentServer.UpdatePetAttribute(petComponentServer.RolePetInfos[i], false);
            }

            reply();
            await ETTask.CompletedTask; */
        #endif
}
    }
}
