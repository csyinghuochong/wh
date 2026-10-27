namespace ET
{
    /// <summary>把 VIP 和当前称号推到本区、战区聊天服。</summary>
    public static class ChatShowHelper
    {
        public static void Sync(Unit unit)
        {
            SyncAsync(unit).Coroutine();
        }

        static async ETTask SyncAsync(Unit unit)
        {
            int vipLevel = unit.GetComponent<RechargeComponentServer>().RechargePro.VipLevel;
            int titleId = unit.GetComponent<TitleComponentServer>().TitleId;
            long chatServerId = DBHelper.GetChatServerId(unit);
            if (chatServerId != 0)
            {
                await MessageHelper.CallActor(chatServerId, new M2Chat_UpdateChatShow()
                {
                    UnitId = unit.Id,
                    VipLevel = vipLevel,
                    TitleId = titleId,
                });
            }

            if (unit.IsDisposed)
            {
                return;
            }

            long warChatServerId = DBHelper.GetWarChatServerId(unit);
            if (warChatServerId == 0)
            {
                return;
            }

            await MessageHelper.CallActor(warChatServerId, new M2Chat_UpdateChatShow()
            {
                UnitId = unit.Id,
                VipLevel = vipLevel,
                TitleId = titleId,
            });
        }
    }
}
