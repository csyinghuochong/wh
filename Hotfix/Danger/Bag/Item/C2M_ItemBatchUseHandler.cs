using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemBatchUseHandler : AMActorLocationRpcHandler<Unit, C2M_ItemBatchUseRequest, M2C_ItemBatchUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemBatchUseRequest request, M2C_ItemBatchUseResponse response, Action reply)
        {
            if (unit.Type != UnitType.Player || request.Items == null || request.Items.Count == 0 || request.Items.Count > 1000)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            // 按提交顺序逐项执行，失败项保持原状并继续；每项及时推送，避免后续使用或发奖再次改变同一格子。
            foreach (ItemUseInfo item in request.Items)
            {
                if (item == null)
                {
                    if (response.Error == ErrorCode.ERR_Success)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }
                    continue;
                }

                M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
                MailInfo overflowMail = new MailInfo();
                int error = await ItemUseHelper.UseBagItem(unit, item.OperateBagID, item.Loc, item.UseNum,
                    item.OperatePar, bagUpdate, overflowMail);
                if (unit.IsDisposed)
                {
                    return;
                }
                if (response.Error == ErrorCode.ERR_Success && error != ErrorCode.ERR_Success)
                {
                    response.Error = error;
                }
                if (error == ErrorCode.ERR_Success && overflowMail.ItemList.Count > 0)
                {
                    response.OperatePar = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_Over_Bag_Volume).ToString();
                }
                if (bagUpdate.BagInfoDelete.Count > 0 || bagUpdate.BagInfoUpdate.Count > 0)
                {
                    MessageHelper.SendToClient(unit, bagUpdate);
                    DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, unit.GetComponent<BagComponentServer>()).Coroutine();
                }
            }

            reply();
        }
    }
}
