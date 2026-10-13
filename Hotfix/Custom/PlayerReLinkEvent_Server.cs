using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public class PlayerReLinkEvent_Server : AEvent<EventType.PlayerReLink>
    {
        protected override void Run(EventType.PlayerReLink args)
        {
            Session session = args.Session;
            Player player = args.Player;
            Session oldSession = player.ClientSession;

            if (session.GetComponent<MailBoxComponent>() == null)
            {
                session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
            }
            session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
            session.GetComponent<SessionPlayerComponent>().PlayerInstanceId = player.InstanceId;
            session.GetComponent<SessionPlayerComponent>().AccountId = args.AccountId;
            // 先绑新连接。旧连接断开时看到 ClientSession 已换人，不会把新会话清掉，也不会开始离线倒计时。
            player.ClientSession = session;

            if (oldSession == null || oldSession == session || oldSession.IsDisposed)
            {
                return;
            }

            // 同网关又登了一次，旧连接还挂在这个 Player 上。通知旧客户端被顶号，然后断开。
            SessionPlayerComponent oldPlayer = oldSession.GetComponent<SessionPlayerComponent>();
            if (oldPlayer != null)
            {
                oldPlayer.isLoginAgain = true;
            }
            oldSession.Send(new A2C_Disconnect() { Error = ErrorCode.ERR_OtherAccountLogin });
            oldSession.Disconnect().Coroutine();
        }
    }
}
