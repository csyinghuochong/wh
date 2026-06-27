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
			if (skillSetComponentServer.GetBySkillID(request.SkillID) == null)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

			List<SkillPro> SkillList = skillSetComponentServer.SkillList;
			LDSkill skillconf = LDSkillCategory.Instance.Get(request.SkillID);
			

            RoleInfoComponentServer unitInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();

		            
			for (int i = SkillList.Count - 1; i >= 0; i--)
			{
				if (SkillList[i].SkillID == request.SkillID)
				{
					SkillList[i].Level++;
					break;
				}
			}
			
			//unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneySub(UserDataType.Gold, (costGoldValue*-1).ToString(), true, ItemGetWay.CostItem);
			//unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.Sp, (costSPValue * -1).ToString());

			Function_Fight.UnitUpdateProperty_Base( unit,true, true );
			//测试跑马灯
			//string text = "";
			//if (RandomHelper.RandFloat01() < 0.5f)
			//	text = "测试一个长字符串的适配！！测试一个长字符串的适配！！";
			//else
			//	text = "";
			//M2C_HorseNoticeInfo m2C_HorseNoticeInfo = new M2C_HorseNoticeInfo() { NoticeText = skillconf.SkillName + " 升级了. " + text };
			//MessageHelper.Broadcast(unit, m2C_HorseNoticeInfo);

			////测试邮件
			//long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(unit.DomainZone(), Enum.GetName(SceneType.EMail)).InstanceId;
			//E2M_EMailSendResponse g_SendChatRequest = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
			//	(mailServerId, new M2E_EMailSendRequest() {  Id = unit.GetComponent<UnitInfoComponent>().UserID });

			reply();
			await ETTask.CompletedTask;
		}

	}
}