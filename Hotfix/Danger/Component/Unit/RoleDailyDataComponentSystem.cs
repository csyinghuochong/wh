using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class RoleDailyDataComponentAwakeSystem : AwakeSystem<RoleDailyDataComponent>
    {
        public override void Awake(RoleDailyDataComponent self)
        {
        }
    }

    public static class RoleDailyDataComponentSystem
    {
        private static readonly int[] DailyResetNumericTypes =
        {
            NumericType.HongBao,
            NumericType.Now_XiLian,
            NumericType.PetChouKa,
            NumericType.YueKaAward,
            NumericType.XiuLian_ExpNumber,
            NumericType.XiuLian_CoinNumber,
            NumericType.XiuLian_ExpTime,
            NumericType.XiuLian_CoinTime,
            NumericType.TiLiKillNumber,
            NumericType.ChouKa,
            NumericType.ExpToGoldTimes,
            NumericType.RechargeSign,
            NumericType.TeamDungeonTimes,
            NumericType.TeamDungeonXieZhu,
            NumericType.BattleTodayKill,
            NumericType.FubenTimesReset,
            NumericType.FenShangSet,
            NumericType.ArenaNumber,
            NumericType.LocalDungeonTime,
            NumericType.TreasureTask,
            NumericType.JiaYuanExchangeZiJin,
            NumericType.JiaYuanExchangeExp,
            NumericType.JiaYuanVisitRefresh,
            NumericType.JiaYuanGatherOther,
            NumericType.JiaYuanPickOther,
            NumericType.UnionDonationNumber,
            NumericType.UnionDiamondDonationNumber,
            NumericType.RaceDonationNumber,
            NumericType.JiaYuanPurchaseRefresh,
            NumericType.TowerOfSealArrived,
            NumericType.TowerOfSealFinished,
            NumericType.RunRaceRankId,
            NumericType.HappyCellIndex,
            NumericType.HappyMoveNumber,
            NumericType.PetMineBattle,
            NumericType.PetMineLogin,
            NumericType.CostTiLi,
            NumericType.DrawIndex,
            NumericType.DrawReward,
            NumericType.PetMineReset,
            NumericType.V1ChouKaNumber,
            NumericType.V1RechageNumber,
            NumericType.PetExploreNumber,
            NumericType.PetHeXinExploreNumber,
        };

        /// <summary>零点 / 跨天：重置所有日清 Numeric 字段。</summary>
        public static void OnZeroClockUpdate(this RoleDailyDataComponent self, bool notice = false)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            for (int i = 0; i < DailyResetNumericTypes.Length; i++)
            {
                numericComponent.ApplyValue(DailyResetNumericTypes[i], 0, notice);
            }

            int yuekatimes = numericComponent.GetAsInt(NumericType.YueKaRemainTimes);
            numericComponent.ApplyValue(NumericType.YueKaEndTime, yuekatimes, notice);

            int lirun = (int)(numericComponent.GetAsInt(NumericType.InvestTotal) * 0.25f);
            numericComponent.ApplyValue(
                NumericType.InvestTotal,
                numericComponent.GetAsInt(NumericType.InvestTotal) + lirun,
                notice);
        }
    }
}
