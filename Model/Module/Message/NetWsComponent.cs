namespace ET
{
    /// <summary>
    /// 外网 WebSocket 监听（小游戏等）。与 NetKcpComponent(TCP) 并列，互不影响。
    /// </summary>
    public class NetWsComponent: Entity, IAwake<string, int>, IDestroy
    {
        public AService Service;

        public int SessionStreamDispatcherType { get; set; }
    }
}
