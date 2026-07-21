using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_SkillCmdHandler : AMActorLocationRpcHandler<Unit, C2M_SkillCmd, M2C_SkillCmd>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillCmd request, M2C_SkillCmd response, Action reply)
        {
            try
            {
                int juexingid = 0;
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                NumericComponent numeric = unit.GetComponent<NumericComponent>();
                SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();
                DBSaveComponent dbSaveComponent = unit.GetComponent<DBSaveComponent>();
                int occtwo = roleInfo.OccTwo;
                if (occtwo != 0)
                {
                   
                }
                if (juexingid == request.SkillID)
                {
                    if (numeric.GetAsLong(NumericType.JueXingAnger) < 500 && !CommonHelper.IsInnerNet())
                    {
                        response.Error = ErrorCode.Error_AngleNotEnough;
                        reply();
                        return;
                    }
                }

                if (!LDSkillCategory.Instance.Contain(request.SkillID))
                {
                    Log.Error($"C2M_SkillCmd 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                BagComponentServer bag = null;
                ChengJiuComponentServer chengJiu = null;
                TaskComponentServer task = null;
                if (request.ItemId > 0)
                { 
                    bag = unit.GetComponent<BagComponentServer>();
                    if(bag.GetItemNumber(ItemBigType.Type_Item, request.ItemId) <= 0)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                    if (!LDItemCategory.Instance.Contain(request.ItemId))
                    {
                        Console.WriteLine($"request.SkillID item:  {request.ItemId}");
                        Log.Error($"C2M_SkillCmd 2");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    LDItem ldItem =LDItemCategory.Instance.Get(request.ItemId);
                    if (ldItem.ItemType != 101 && ldItem.ItemType != 110)
                    {
                        Console.WriteLine($"request.SkillID error:  {request.SkillID}");
                        Log.Error($"C2M_SkillCmd 3");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                }

                MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();        
                LDSkill ldSkill = LDSkillCategory.Instance.Get(request.SkillID);
                if (mapComponent.MapTypeEnum != MapTypeEnum.RunRace && !CommonHelper.IsInnerNet())
                {
                }
                dbSaveComponent.NoFindPath = 0;
                numeric.ApplyValue(NumericType.HorseRide, 0, true, true);

                M2C_SkillCmd m2C_SkillCmd = skillManagerComponent.OnUseSkill(request, true);

                if (m2C_SkillCmd!= null && m2C_SkillCmd.Error == ErrorCode.ERR_Success)
                {
                    if (request.ItemId > 0)
                    {
                        bag.OnCostItemData($"{request.ItemId};1",ItemLocType.ItemLocBag, ItemGetWay.GM);

                        if (CommonConfig.ChengJiuLianJin.Contains(request.ItemId))
                        {
                            chengJiu ??= unit.GetComponent<ChengJiuComponentServer>();
                            task ??= unit.GetComponent<TaskComponentServer>();
                            chengJiu.TriggerEvent(ChengJiuTargetEnum.BattleUseItem_214, 0, 1);
                            task.TriggerTaskEvent(TastConditionType.BattleUseItem_30, 0, 1);  
                        }
                    }
                    if (juexingid == request.SkillID)
                    {
                        numeric.ApplyValue(NumericType.JueXingAnger, 0);
                    }
                }
                
                response.Error = m2C_SkillCmd!= null ? m2C_SkillCmd.Error : ErrorCode.ERR_UseSkillError;
                response.Message = m2C_SkillCmd != null ? m2C_SkillCmd.Message: string.Empty;
                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }

    }
}