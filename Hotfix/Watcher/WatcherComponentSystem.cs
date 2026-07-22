using System;
using System.Collections;
using System.Diagnostics;

namespace ET
{
    public class WatcherComponentAwakeSystem: AwakeSystem<WatcherComponent>
    {
        public override void Awake(WatcherComponent self)
        {
            WatcherComponent.Instance = self;
        }
    }
    
    public class WatcherComponentDestroySystem: DestroySystem<WatcherComponent>
    {
        public override void Destroy(WatcherComponent self)
        {
            WatcherComponent.Instance = null;
        }
    }
    
    public static class WatcherComponentSystem
    {

        public static async ETTask CheckLoginServer(this WatcherComponent self)
        {
            await ETTask.CompletedTask;
        }

        public static void Stop(this WatcherComponent self)
        {
            for (int i = self.Processes.Count - 1; i >= 0; i--)
            {
                Log.Info($"close process: {self.Processes[i].Id} {self.Processes[i].ProcessName} ");
                self.Processes[i].Kill();
            }
        }

        public static void Start(this WatcherComponent self, int createScenes = 0)
        {
            string[] localIP = NetworkHelper.GetAddressIPs();
            var processConfigs = StartProcessConfigCategory.Instance.GetAll();
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values)
            {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP))
                {
                    continue;
                }
                Process process = WatcherHelper.StartProcess(startProcessConfig.Id, createScenes);
                self.Processes.Add(startProcessConfig.Id, process);
            }
        }


    }
}