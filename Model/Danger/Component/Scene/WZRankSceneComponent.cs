namespace ET
{
    /// <summary>战区排行场景组件（挂在 Zone=200x / SceneType=WZRank）</summary>
    public class WZRankSceneComponent : Entity, IAwake, IDestroy
    {
        public long Timer;

        public DBRankInfo DBRankInfo;
    }
}
