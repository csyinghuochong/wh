using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class ShouJiChapterInfoComponentAwakeSystem : AwakeSystem<ShouJiChapterInfoComponent>
    {
        public override void Awake(ShouJiChapterInfoComponent self)
        {
            ShouJiChapterInfoComponent.Instance = self;
            self.Load();
        }
    }

    [ObjectSystem]
    public class ShouJiChapterInfoComponentLoadSystem : LoadSystem<ShouJiChapterInfoComponent>
    {
        public override void Load(ShouJiChapterInfoComponent self)
        {
            self.Load();
        }
    }

    [ObjectSystem]
    public class ShouJiChapterInfoComponentDestroySystem : DestroySystem<ShouJiChapterInfoComponent>
    {
        public override void Destroy(ShouJiChapterInfoComponent self)
        {
            ShouJiChapterInfoComponent.Instance = null;
        }
    }

    public static class ShouJiChapterInfoComponentSystem
    {
        public static void Load(this ShouJiChapterInfoComponent self)
        { 
        }
    }
}
