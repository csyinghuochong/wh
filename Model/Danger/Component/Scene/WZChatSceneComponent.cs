using System.Collections.Generic;

namespace ET
{
    /// <summary>战区聊天场景组件（挂在 Zone=200x / SceneType=WZChat）</summary>
    public class WZChatSceneComponent : Entity, IAwake, IDestroy
    {
        public Dictionary<long, ChatInfoUnit> ChatInfoUnitsDict = new Dictionary<long, ChatInfoUnit>();

        /// <summary>战区频道最近消息（内存缓存）</summary>
        public List<ChatInfo> WarChatInfos = new List<ChatInfo>();
    }
}
