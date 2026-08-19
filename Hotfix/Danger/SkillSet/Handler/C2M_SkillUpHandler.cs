using System;
using System.Collections.Generic;

namespace ET
{
    //技能升级
    [ActorMessageHandler]
    public class C2M_SkillUpHandler : AMActorLocationRpcHandler<Unit, C2M_SkillUp, M2C_SkillUp>
    {

		protected override async ETTask Run(Unit unit, C2M_SkillUp request, M2C_SkillUp response, Action reply)
		{
			SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
			SkillPro skillPro = skillSetComponentServer.GetBySkillID(request.SkillID);
			if (skillPro == null)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
			int maxLv = LDSkill_Battle_LvCategory.Instance.GetSkillMaxLv(request.SkillID);
			if (skillPro.Level >= maxLv)
			{
                response.Error = ErrorCode.ERR_SkillMaxLevel;
                reply();
                return;
            }

			int nextLv = skillPro.Level + 1;
			LDSkill_Battle_Lv nextCfg = LDSkill_Battle_LvCategory.Instance.GetLDSkillLv(request.SkillID, nextLv);
			if (nextCfg == null)
			{
				response.Error = ErrorCode.ERR_Parameter;
				reply();
				return;
			}

			RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
			if (roleInfo.Lv < nextCfg.Learn_Lv)
			{
				response.Error = ErrorCode.ERR_LevelNoEnough;
				reply();
				return;
			}

			List<RewardItem> costItems = ItemNewHelper.GetRewardItems(nextCfg.Cost);
			BagComponentServer bag = unit.GetComponent<BagComponentServer>();
			if (costItems.Count > 0 && !bag.CheckNeedItem(costItems))
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			if (costItems.Count > 0 && !bag.OnCostItemData(costItems, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			skillPro.Level++;
			Function_Fight.UnitUpdateProperty_Base( unit,true, true );

			reply();
			await ETTask.CompletedTask;
		}

	}
}