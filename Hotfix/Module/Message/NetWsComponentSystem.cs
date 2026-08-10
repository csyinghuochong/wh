using System.IO;
using System.Net;

namespace ET
{
    [ObjectSystem]
    public class NetWsComponentAwakeSystem: AwakeSystem<NetWsComponent, string, int>
    {
        public override void Awake(NetWsComponent self, string httpPrefix, int sessionStreamDispatcherType)
        {
            self.SessionStreamDispatcherType = sessionStreamDispatcherType;
            self.Service = new WService(NetThreadComponent.Instance.ThreadSynchronizationContext, new[] { httpPrefix });
            self.Service.ErrorCallback += (channelId, error) => self.OnError(channelId, error);
            self.Service.ReadCallback += (channelId, memory) => self.OnRead(channelId, memory);
            self.Service.AcceptCallback += (channelId, address) => self.OnAccept(channelId, address);

            NetThreadComponent.Instance.Add(self.Service);
            // 真正是否 listen 成功看后续: WService HttpListener start ok
            Log.Info($"NetWsComponent create WService prefix={httpPrefix}");
        }
    }

    [ObjectSystem]
    public class NetWsComponentDestroySystem: DestroySystem<NetWsComponent>
    {
        public override void Destroy(NetWsComponent self)
        {
            if (self.Service != null)
            {
                NetThreadComponent.Instance.Remove(self.Service);
                self.Service.Destroy();
                self.Service = null;
            }
        }
    }

    public static class NetWsComponentSystem
    {
        public static void OnRead(this NetWsComponent self, long channelId, MemoryStream memoryStream)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeHelper.ClientNow();
            SessionStreamDispatcher.Instance.Dispatch(self.SessionStreamDispatcherType, session, memoryStream);
        }

        public static void OnError(this NetWsComponent self, long channelId, int error)
        {
            Session session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.Error = error;
            session.Dispose();
        }

        public static void OnAccept(this NetWsComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            Session session = self.AddChildWithId<Session, AService>(channelId, self.Service);
            session.RemoteAddress = ipEndPoint;

            session.AddComponent<SessionAcceptTimeoutComponent>();
            session.AddComponent<SessionIdleCheckerComponent, int>(NetThreadComponent.checkInteral);
        }
    }
}
