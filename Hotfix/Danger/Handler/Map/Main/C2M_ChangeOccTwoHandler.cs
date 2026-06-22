using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChangeOccTwoHandler : AMActorLocationRpcHandler<Unit, C2M_ChangeOccTwoRequest, M2C_ChangeOccTwoResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChangeOccTwoRequest request, M2C_ChangeOccTwoResponse response, Action reply)
        {
            //判断当前角色等级是否达到
            if (unit.GetComponent<RoleInfoComponent>().UserInfo.Lv < 18) 
            {
                response.Error = ErrorCode.ERR_Occ_Hint_1;
                reply();
                return;
            }

            int OccTwo = unit.GetComponent<RoleInfoComponent>().UserInfo.OccTwo;
            ////判断当前角色是否已经进行转职
            if (OccTwo != 0 )
            {
                response.Error = ErrorCode.ERR_Occ_Hint_2;
                reply();
                return;
            }

            if (!LDOccupation_TransferCategory.Instance.Contain(request.OccTwoID))
            {
                Log.Error($"C2M_ChangeOccTwoRequest.1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();

            unit.GetComponent<SkillSetComponent>().OnChangeJueXing(dataCollationComponent.OccTwoOld, request.OccTwoID);
            unit.GetComponent<SkillSetComponent>().OnChangeOccTwoRequest(request.OccTwoID);
            unit.GetComponent<TaskComponent>().OnChangeOccTwo();

            if (OccTwo == 0)
            {
                string userName = unit.GetComponent<RoleInfoComponent>().UserInfo.Name;
                string occtwoname = WordHelper.GetShowText(LDOccupation_TransferCategory.Instance.Get(request.OccTwoID).Name, 0);
                string occtwonameen = WordHelper.GetShowText(LDOccupation_TransferCategory.Instance.Get(request.OccTwoID).Name, 1);
                
                string noticeContent = $"{userName} 在主城转职大师处成功转职:<color=#C4FF00>{occtwoname}</color>";
                string noticeContentEn = $"{userName} at main city Job Change Master transfer successful:<color=#C4FF00>{occtwonameen}</color>";
                ServerMessageHelper.SendBroadMessage(unit.DomainZone(), NoticeType.Notice, noticeContent, noticeContentEn);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
