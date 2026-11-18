using System;
using System.Linq;

namespace ET
{
    [Timer(TimerType.MonsterBodyTimer)]
    public class MonsterBodyTimer : ATimer<MonsterBodyComponent>
    {
        public override void Run(MonsterBodyComponent self)
        {
            try
            {
                self.CheckExpire();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    [ObjectSystem]
    public class MonsterBodyComponentAwakeSystem : AwakeSystem<MonsterBodyComponent>
    {
        public override void Awake(MonsterBodyComponent self)
        {
            self.Viewers.Clear();
            self.ResetLifetime();
            // 暂停寿命期间仍检查查看者是否已离线或换图，移动距离不影响已打开的界面。
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(1000, TimerType.MonsterBodyTimer, self);
        }
    }

    [ObjectSystem]
    public class MonsterBodyComponentDestroySystem : DestroySystem<MonsterBodyComponent>
    {
        public override void Destroy(MonsterBodyComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
            self.Viewers.Clear();
        }
    }

    public static class MonsterBodyComponentSystem
    {
        /// <summary>读取 GlobalValue 配置的秒数，转换为毫秒。</summary>
        public static long GetLifetime(bool hasDrops)
        {
            string key = hasDrops ? GlobalValueKey.Global_Monster_Body_Item_Refresh : GlobalValueKey.Global_Monster_Body_NotItem_Refresh;
            return LDGlobalValueCategory.Instance.GetInt(key) * 1000L;
        }

        public static void ResetLifetime(this MonsterBodyComponent self)
        {
            bool hasDrops = self.GetParent<Unit>().GetComponent<UnitInfoComponent>().Drops.Count > 0;
            if (!hasDrops) self.Viewers.Clear();
            self.ExpireTime = TimeHelper.ServerNow() + GetLifetime(hasDrops);
        }

        /// <summary>重复打开/关闭不叠加；最后一位查看者关闭后重新计时。</summary>
        public static void SetViewing(this MonsterBodyComponent self, Unit player, bool isOpen)
        {
            if (isOpen)
            {
                self.Viewers[player.Id] = player.GetComponent<UnitGateComponent>().GateSessionActorId;
                return;
            }
            if (!self.Viewers.Remove(player.Id) || self.Viewers.Count > 0) return;
            self.ResetLifetime();
        }

        public static void CheckExpire(this MonsterBodyComponent self)
        {
            Unit monster = self.GetParent<Unit>();
            if (monster.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 1)
            {
                monster.RemoveComponent<MonsterBodyComponent>();
                return;
            }
            UnitComponent units = monster.GetParent<UnitComponent>();
            bool hadViewers = self.Viewers.Count > 0;
            foreach (long id in self.Viewers.Keys.ToArray())
            {
                Unit player = units.Get(id);
                if (player == null || player.Type != UnitType.Player
                    || player.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
                {
                    self.Viewers.Remove(id);
                    continue;
                }
                UnitGateComponent gate = player.GetComponent<UnitGateComponent>();
                if (gate.PlayerState != PlayerState.Game || gate.GateSessionActorId != self.Viewers[id]) self.Viewers.Remove(id);
            }
            if (hadViewers && self.Viewers.Count == 0) self.ResetLifetime();
            if (self.Viewers.Count > 0 || TimeHelper.ServerNow() < self.ExpireTime || !DllHelper.BattleCheck) return;
            // 尸体销毁时一并清理其持有的掉落记录。
            units.Remove(monster.Id);
        }
    }
}
