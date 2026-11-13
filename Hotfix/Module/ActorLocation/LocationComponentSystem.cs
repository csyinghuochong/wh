namespace ET
{
    [ObjectSystem]
    public class LockInfoAwakeSystem: AwakeSystem<LockInfo, long, CoroutineLock>
    {
        public override void Awake(LockInfo self, long lockInstanceId, CoroutineLock coroutineLock)
        {
            self.CoroutineLock = coroutineLock;
            self.LockInstanceId = lockInstanceId;
        }
    }
    
    [ObjectSystem]
    public class LockInfoDestroySystem: DestroySystem<LockInfo>
    {
        public override void Destroy(LockInfo self)
        {
            self.CoroutineLock.Dispose();
            self.LockInstanceId = 0;
        }
    }
    
    public static class LocationComponentSystem
    {
        public static async ETTask Add(this LocationComponent self, long key, long instanceId)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
            {
                self.locations[key] = instanceId;
                Log.Info($"location add key: {key} instanceId: {instanceId}");
            }
        }

        public static async ETTask Remove(this LocationComponent self, long key)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
            {
                self.locations.Remove(key);
                Log.Info($"location remove key: {key}");
            }
        }

        public static async ETTask Lock(this LocationComponent self, long key, long instanceId, int time = 0)
        {
            // 生命周期由下面的 Location 超时管理，避免协程锁先超时放行查询。
            CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key, 0);
            self.locations.TryGetValue(key, out long originalInstanceId);

            LockInfo lockInfo = self.AddChild<LockInfo, long, CoroutineLock>(instanceId, coroutineLock);
            self.lockInfos.Add(key, lockInfo);

            Log.Info($"location lock key: {key} instanceId: {instanceId}");

            if (time > 0)
            {
                async ETTask TimeWaitAsync()
                {
                    long lockInfoInstanceId = lockInfo.InstanceId;
                    await TimerComponent.Instance.WaitAsync(time);
                    if (lockInfo.InstanceId != lockInfoInstanceId)
                    {
                        return;
                    }
                    Log.Info($"location timeout unlock key: {key} lockId: {instanceId} newInstanceId: {originalInstanceId}");
                    self.UnLock(key, instanceId, originalInstanceId);
                }
                TimeWaitAsync().Coroutine();
            }
        }

        public static bool UnLock(this LocationComponent self, long key, long oldInstanceId, long newInstanceId)
        {
            if (!self.lockInfos.TryGetValue(key, out LockInfo lockInfo))
            {
                // 回包丢失后的重复解锁允许返回成功，但不能覆盖已经变化的路由。
                return self.locations.TryGetValue(key, out long currentInstanceId) && currentInstanceId == newInstanceId;
            }

            if (oldInstanceId != lockInfo.LockInstanceId)
            {
                return false;
            }

            Log.Info($"location unlock key: {key} instanceId: {oldInstanceId} newInstanceId: {newInstanceId}");

            self.locations[key] = newInstanceId;

            self.lockInfos.Remove(key);

            // 解锁
            lockInfo.Dispose();
            return true;
        }

        public static async ETTask<long> Get(this LocationComponent self, long key)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key))
            {
                self.locations.TryGetValue(key, out long instanceId);
                Log.Info($"location get key: {key} instanceId: {instanceId}");
                return instanceId;
            }
        }
    }
}
