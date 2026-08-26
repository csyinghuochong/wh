using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 任务发奖 / 结算发奖：打断 Task↔Bag / Task↔Pet 直接互调。
    /// Task 只负责任务进度；发奖与宠物进度写在本 Helper。
    /// </summary>
    public static class TaskRewardHelper
    {
        public static void GrantRewards(Unit unit, List<RewardItem> rewardItems, string getWay)
        {
            if (unit == null || unit.IsDisposed || rewardItems == null || rewardItems.Count == 0)
            {
                return;
            }
            unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, getWay);
        }

        public static void GrantTaskCommitRewards(Unit unit, List<RewardItem> rewardItems)
        {
            GrantRewards(unit, rewardItems, $"{ItemGetWay.TaskReward}_{TimeHelper.ServerNow()}");
        }

        public static void SettlePetTianTiWin(Unit unit, List<RewardItem> rewardItems, string getWay)
        {
            GrantRewards(unit, rewardItems, getWay);
            unit.GetComponent<TaskComponentServer>()?.OnPetTianTiWin();
        }

        public static void SettlePetFubenWin(Unit unit, List<RewardItem> rewardItems, string getWay, int petFubenId, int star)
        {
            GrantRewards(unit, rewardItems, getWay);
            unit.GetComponent<PetComponentServer>()?.OnPassPetFuben(petFubenId, star);
            unit.GetComponent<TaskComponentServer>()?.OnPetFubenWin(petFubenId);
        }
    }

    /// <summary>
    /// 日/周活跃：DailyData 只改点数，任务进度由本 Helper 扇出，避免 DailyData↔Task 互调。
    /// </summary>
    public static class ActivePointHelper
    {
        public static void Add(Unit unit, int userDataType, int add, bool notice = true)
        {
            if (unit == null || unit.IsDisposed || add <= 0)
            {
                return;
            }

            unit.GetComponent<RoleDailyDataComponentServer>()?.AddActivePoint(userDataType, add, notice);
            unit.GetComponent<TaskComponentServer>()?.RefreshActivityTasksByActivePoint(userDataType, notice);
        }
    }

    /// <summary>
    /// 宠物获得后的任务/成就推进，避免 Pet 与 Task/ChengJiu 在多处各写一遍。
    /// </summary>
    public static class PetProgressionHelper
    {
        public static void NotifyPetAcquired(Unit unit, PetInfo newpet)
        {
            if (unit == null || unit.IsDisposed || newpet == null)
            {
                return;
            }

            unit.GetComponent<ChengJiuComponentServer>()?.OnGetPet(newpet);
            unit.GetComponent<TaskComponentServer>()?.OnGetPet(newpet);
        }
    }

    /// <summary>
    /// 副本结算入口：Scene 只调这里，不直接拼 Task+Bag+Pet。
    /// </summary>
    public static class DungeonSettlementHelper
    {
        public static void SettlePetTianTiWin(Unit unit, List<RewardItem> rewardItems, string getWay)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            TaskComponentServer task = unit.GetComponent<TaskComponentServer>();
            if (task == null)
            {
                TaskRewardHelper.GrantRewards(unit, rewardItems, getWay);
                return;
            }
            using (task.TaskEventBatch())
            {
                TaskRewardHelper.SettlePetTianTiWin(unit, rewardItems, getWay);
            }
        }

        public static void SettlePetFubenWin(Unit unit, List<RewardItem> rewardItems, string getWay, int petFubenId, int star)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            TaskComponentServer task = unit.GetComponent<TaskComponentServer>();
            if (task == null)
            {
                TaskRewardHelper.GrantRewards(unit, rewardItems, getWay);
                unit.GetComponent<PetComponentServer>()?.OnPassPetFuben(petFubenId, star);
                return;
            }
            using (task.TaskEventBatch())
            {
                TaskRewardHelper.SettlePetFubenWin(unit, rewardItems, getWay, petFubenId, star);
            }
        }

        public static void SettleTeamDungeon(Unit unit, int sceneId, int hurtRate, bool shenYuan)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }
            unit.GetComponent<TaskComponentServer>()?.OnTeamDungeonSettle(sceneId, hurtRate);
            unit.GetComponent<ChengJiuComponentServer>()?.OnTeamDungeonSettle(shenYuan);
        }
    }
}
