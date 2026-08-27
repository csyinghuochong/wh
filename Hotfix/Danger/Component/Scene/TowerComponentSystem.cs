using System;
using System.Linq;

namespace ET
{

    [ObjectSystem]
    public class TowerComponentAwakeSystem : AwakeSystem<TowerComponent>
    {
        public override void Awake(TowerComponent self)
        {
            self.TowerId = 0;
            self.Timer = 0;
        }
    }

    [ObjectSystem]
    public class TowerComponentDestroySystem : DestroySystem<TowerComponent>
    {
        public override void Destroy(TowerComponent self)
        {
        }
    }

    public  static class TowerComponentSystem
    {
        private static string[] CachedTowerStartIds;

        private static void EnsureTowerStartIdsCache()
        {
            if (CachedTowerStartIds != null)
            {
                return;
            }
            CachedTowerStartIds = LDGlobalValueCategory.Instance.Get(65).Value.Split(';');
        }

        public static void OnKillEvent(this TowerComponent self, Unit defend)
        {
            if (defend.Id == self.MainUnit.Id)
            {
                self.OnTowerOver("PlayerDie");
                return;
            }
            if (defend.GetBattleCamp() == self.MainUnit.GetBattleCamp())
            {
                return;
            }
            if (SceneCreatureHelp.IsAllMonsterDead(self.DomainScene(), self.MainUnit))
            {
                self.OnTimer();
                return;
            }
        }

        public static void OnEmptyReward(this TowerComponent self)
        {
            M2C_FubenSettlement message = new M2C_FubenSettlement();
            message.BattleResult = 2;
            message.RewardExp = 0;
            message.RewardGold = 0;
            MessageHelper.SendToClient(self.MainUnit, message);
        }

        public static void OnTowerOver(this TowerComponent self, string way)
        {
           self.TowerId = 0;
        }

        public static void OnTimer(this TowerComponent self)
        {
            //奖励
        
        }

        public static async ETTask CreateMonster(this TowerComponent self, int towerId, bool init)
        {
            long instanceId = self.InstanceId;
            self.MainUnit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.TowerId, $"{MapTypeEnum.TowerDungeon};{towerId}");
            await TimerComponent.Instance.WaitAsync(2000);
            if (instanceId != self.InstanceId)
            {
                return;
            }
            if (self.MainUnit == null || self.MainUnit.IsDisposed)
            {
                return;
            }
            if (!init && self.TowerId == 0)
            {
                return;
            }
            Scene scene = self.DomainScene();
            /*TowerConfig towerConfig = TowerConfigCategory.Instance.Get(towerId);
            self.WaveTime = towerConfig.NextTime * 1000;
            FubenHelp.CreateMonsterList(scene, towerConfig.MonsterSet);
            */
        }

        public static void BeginTower(this TowerComponent self)
        {
            EnsureTowerStartIdsCache();
            string[] ids = CachedTowerStartIds;
            int index = self.FubenDifficulty - 1;

            if (index < 0)
            {
                index = 0;
            }
            if (index > 2)
            {
                index = 2;
            }
            self.CreateMonster(int.Parse(ids[index]),true).Coroutine();
        }
    }
}
