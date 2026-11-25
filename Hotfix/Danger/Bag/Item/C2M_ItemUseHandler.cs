using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemUseHandler : AMActorLocationRpcHandler<Unit, C2M_ItemUseRequest, M2C_ItemUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemUseRequest request, M2C_ItemUseResponse response, Action reply)
        {
            M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
            // 接收本次礼包奖励的溢出邮件；UseItem先尝试入包，放不下的奖励由其写入附件并保存邮件。
            MailInfo overflowMail = new MailInfo();
            response.Error = await ItemUseHelper.UseBagItem(unit, request.OperateBagID, request.Loc, request.UseNum,
                request.OperatePar, bagUpdate, overflowMail);
            if (unit.IsDisposed)
            {
                return;
            }
            // 使用成功且有奖励转邮件时，Error仍为0；OperatePar携带Word_Prompt.Id字符串，供客户端显示补充提示。
            // 没有溢出则保持OperatePar为空；使用失败由Error返回失败提示，不走此分支。
            if (response.Error == ErrorCode.ERR_Success && overflowMail.ItemList.Count > 0)
            {
                response.OperatePar = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_Over_Bag_Volume).ToString();
            }
            if (bagUpdate.BagInfoDelete.Count > 0 || bagUpdate.BagInfoUpdate.Count > 0)
            {
                MessageHelper.SendToClient(unit, bagUpdate);
                DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, unit.GetComponent<BagComponentServer>()).Coroutine();
            }
            reply();
            await ETTask.CompletedTask;
        }
    }

    [ActorMessageHandler]
    public class C2M_ItemTradeConfirmHandler : AMActorLocationRpcHandler<Unit, C2M_ItemTradeConfirmRequest, M2C_ItemTradeConfirmResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemTradeConfirmRequest request, M2C_ItemTradeConfirmResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (unit.Type != UnitType.Player || request.BagInfoID <= 0
                    || (!ItemNewHelper.IsValideBagLoc(request.Loc) && request.Loc != (int)ItemLocType.ItemLocEquip))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            BagInfo item = bag.GetItemByLoc((ItemLocType)request.Loc, request.BagInfoID);
            if (item == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }
            if (item.GetTradeStatus() != ItemTradeStatus.DungeonTradable)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            item.SetTradeStatus(ItemTradeStatus.NonTradable);
            M2C_RoleBagUpdate update = new M2C_RoleBagUpdate();
            update.BagInfoUpdate.Add(item);
            MessageHelper.SendToClient(unit, update);
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, bag).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
