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

    //红点数据管理， 驱动表现
    public static class ReddotComponentServerSystem
    {

        /// <summary>
        /// 显示红点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="reddotType"></param>
        public static void AddReddont(this ReddotComponentServer self, int reddotType)
        {
            bool have = false;
            for (int i = self.ReddontList.Count - 1; i >= 0; i--)
            {
                if (self.ReddontList[i].KeyId == reddotType)
                {
                    have = true;
                    break;
                }
            }
            if (!have)
            {
                self.ReddontList.Add(new KeyValuePair() { KeyId = reddotType, Value = "1" });
            }

#if !NOT_UNITY
            EventType.ReddotChange.Instance.ZoneScene = self.ZoneScene();
            EventType.ReddotChange.Instance.ReddotType = reddotType;
            EventType.ReddotChange.Instance.Number = 1;
            EventSystem.Instance.PublishClass(EventType.ReddotChange.Instance);
#endif
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

        /// <summary>
        /// 移除红点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="reddotType"></param>
        /// <returns></returns>
        public static void RemoveReddont(this ReddotComponentServer self, int reddotType)
        {
            for (int i = self.ReddontList.Count - 1; i >=0; i--)
            {
                if (self.ReddontList[i].KeyId == reddotType)
                {
                    self.ReddontList.RemoveAt(i);
                    break;
                }
            }

#if !NOT_UNITY
            EventType.ReddotChange.Instance.ZoneScene = self.ZoneScene();
            EventType.ReddotChange.Instance.ReddotType = reddotType;
            EventType.ReddotChange.Instance.Number = 0;
            EventSystem.Instance.PublishClass(EventType.ReddotChange.Instance);
#endif
        }

#if !NOT_UNITY
        public static void UpdateReddont(this ReddotComponent self, int reddotType)
        {
            bool showReddot = false;
            Unit unit = UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene());

            switch (reddotType)
            {
                case BelongReddot.Belong_110011:
                    int pointRemain = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.PointRemain);
                    showReddot = pointRemain > 0;
                    break;
            }
            
            if (showReddot)
            {
                self.AddReddont(reddotType);
            }
            else
            {
                self.RemoveReddont(reddotType);
            }
        }
#endif

    }
}
