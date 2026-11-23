using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    internal class M2E_GMEMailSendHandler : AMActorRpcHandler<Scene, M2E_GMEMailSendRequest, E2M_GMEMailSendResponse>
    {
        protected override async ETTask Run(Scene scene, M2E_GMEMailSendRequest request, E2M_GMEMailSendResponse response, Action reply)
        {
            Log.Warning($"M2E_GMEMailSendRequest:{request.UserName}");
            int zone = (int)request.ActorId;
            if (request.ActorId != zone || zone <= 0 || !StartZoneConfigCategory.Instance.Contain(zone)
                || StartZoneConfigCategory.Instance.Get(zone).PhysicZone != scene.DomainZone()
                || request.MailType <= 0 || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrEmpty(request.Title)
                || !MailHelp.TryGetGMMailItems(request.Itemlist, out List<BagInfo> items))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            if (request.UserName == "0")
            {
                response.Error = await scene.GetComponent<MailSceneComponent>().OnServerMail(request, items);
                reply();
                return;
            }

            DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
            List<RoleInfoComponentServer> accountInfoList = await dbComponent.Query<RoleInfoComponentServer>(zone, d => d.RoleInfo.Name == request.UserName);
            if (accountInfoList.Count == 0)
            {
                response.Message = $"找不到:{request.UserName}";
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Login_Unregister);
                reply();
                return;
            }

            RoleInfoComponentServer roleInfoComponent = accountInfoList[0];
            if (roleInfoComponent.RoleInfo.RobotId > 0)
            {
                reply();
                return;
            }

            long mailOwnerId = roleInfoComponent.Id;
            List<RechargeComponentServer> rechargeInfoList = await dbComponent.Query<RechargeComponentServer>(zone, d => d.Id == mailOwnerId);
            long rechargeNum = rechargeInfoList.Count > 0 ? rechargeInfoList[0].GetTotalRechargeNum() : 0;
            if (!MailHelp.CheckSendMail(request.MailType, request.Title, rechargeNum, roleInfoComponent))
            {
                reply();
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.ConfigId = 0;
            mailInfo.ParamList.Add("福利发放");
            mailInfo.ParamList.Add("福利发放");
            mailInfo.ItemList = items;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            response.Error = await MailHelp.SendUserMail(zone, mailOwnerId, mailInfo);
            if (response.Error == LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Login_Unregister))
            {
                response.Message = $"找不到:{request.UserName}";
            }

            reply();
        }
    }
}
