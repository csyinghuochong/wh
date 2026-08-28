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
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();
                DBSaveComponent dbSaveComponent = unit.GetComponent<DBSaveComponent>();
                int occtwo = roleInfo.OccTwo;
                if (occtwo != 0)
                {
                   
                }
              
                if (!LDSkill_BattleCategory.Instance.Contain(request.SkillID))
                {
                    Log.Error($"C2M_SkillCmd 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                BagComponentServer bag = null;
                ItemLocType itemLoc = ItemLocType.ItemLocBag;
                if (request.ItemId > 0)
                { 
                    bag = unit.GetComponent<BagComponentServer>();
                    if (!LDItemCategory.Instance.Contain(request.ItemId))
                    {
                        Console.WriteLine($"request.SkillID item:  {request.ItemId}");
                        Log.Error($"C2M_SkillCmd 2");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    LDItem ldItem = LDItemCategory.Instance.Get(request.ItemId);
                    // 60=药水(Param1=技能ID
                    bool skillCastItem = ldItem.ItemType == ItemTypeEnum.SubType_Potion_60;
                    if (!skillCastItem)
                    {
                        Console.WriteLine($"request.SkillID error:  {request.SkillID}");
                        Log.Error($"C2M_SkillCmd 3");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    if (ldItem.ItemType == ItemTypeEnum.SubType_Potion_60
                        && ldItem.GetTypeParam1() != request.SkillID)
                    {
                        Log.Error($"C2M_SkillCmd potion skill mismatch item={request.ItemId} skill={request.SkillID} param1={ldItem.ItemTypeParam1}");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    itemLoc = ItemNewHelper.GetToItemLocType(new RewardItem
                    {
                        ItemType = ItemBigType.Type_Item,
                        ItemID = request.ItemId,
                        ItemNum = 1,
                    });
                    if (bag.GetItemNumber(ItemBigType.Type_Item, request.ItemId, itemLoc) <= 0)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                }

                MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();        
                LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(request.SkillID);
                if (mapComponent.MapTypeEnum != MapTypeEnum.RunRace && !CommonHelper.IsInnerNet())
                {
                }
                dbSaveComponent.NoFindPath = 0;
                unit.GetComponent<MountComponentServer>().Dismount();

                M2C_SkillCmd m2C_SkillCmd = skillManagerComponent.OnUseSkill(request, true);

                if (m2C_SkillCmd!= null && m2C_SkillCmd.Error == ErrorCode.ERR_Success)
                {
                    if (request.ItemId > 0)
                    {
                        bag.OnCostItemData($"{request.ItemId};1", itemLoc, ItemGetWay.GM);
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