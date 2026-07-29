namespace ET
{
    [ObjectSystem]
    public class ReddotComponentAwakeSystem : AwakeSystem<ReddotComponentServer>
    {
        public override void Awake(ReddotComponentServer self)
        {
            self.ReddontList.Clear();
        }
    }

    /// <summary>服务端红点数据（持久化 / 登录下发）。不做表现冒泡。</summary>
    public static class ReddotComponentServerSystem
    {
        public static void AddReddont(this ReddotComponentServer self, int reddotType)
        {
            if (self.GetReddot(reddotType) > 0)
            {
                return;
            }

            self.ReddontList.Add(new KeyValuePair() { KeyId = reddotType, Value = "1" });
        }

        public static int GetReddot(this ReddotComponentServer self, int reddotType)
        {
            for (int i = self.ReddontList.Count - 1; i >= 0; i--)
            {
                if (self.ReddontList[i].KeyId == reddotType)
                {
                    return 1;
                }
            }

            return 0;
        }

        public static void RemoveReddont(this ReddotComponentServer self, int reddotType)
        {
            for (int i = self.ReddontList.Count - 1; i >= 0; i--)
            {
                if (self.ReddontList[i].KeyId == reddotType)
                {
                    self.ReddontList.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
