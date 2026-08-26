using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 经济变更副作用：拍卖成就、金币任务/成就、等级/战力/家园等级推进。
    /// RoleInfo 只负责改数值与协议；进度通知走本 Helper，避免 RoleInfo↔Task/ChengJiu 搅在一起。
    /// </summary>
    public static class PlayerEconomyHelper
    {
        /// <summary>
        /// 货币增加后的成就侧通知（拍卖行卖出金币等）。
        /// </summary>
        public static void NotifyAfterMoneyAdd(Unit unit, long gold, int getWay)
        {
            if (unit == null || unit.IsDisposed || gold <= 0)
            {
                return;
            }

  
        }

        /// <summary>
        /// RoleData 数值落库后的任务/成就推进（金币、家园等级、等级、战力）。
        /// </summary>
        public static void NotifyRoleDataProgression(Unit unit, int type, RoleInfo roleInfo, long delta = 0)
        {
            if (unit == null || unit.IsDisposed || roleInfo == null)
            {
                return;
            }

            TaskComponentServer task = null;
            ChengJiuComponentServer chengJiu = null;

            switch (type)
            {
                case UserDataType.JiaYuanLv:
                    task = unit.GetComponent<TaskComponentServer>();
                    chengJiu = unit.GetComponent<ChengJiuComponentServer>();
                    int jiaYuanLv = unit.GetComponent<JiaYuanComponentServer>()?.JiaYuanLv ?? 1;
                    int jiaYuanShowLv = jiaYuanLv - 10000;
                    task?.OnJiaYuanLevel(jiaYuanShowLv);
                    chengJiu?.OnJiaYuanLevel(jiaYuanShowLv);
                    break;

                case UserDataType.Level:
                    task = unit.GetComponent<TaskComponentServer>();
                    chengJiu = unit.GetComponent<ChengJiuComponentServer>();
                    task?.OnUpdateLevel(roleInfo.Lv);
                    chengJiu?.OnUpdateLevel(roleInfo.Lv);
                    break;

                case UserDataType.Gold:
                    task = unit.GetComponent<TaskComponentServer>();
                    chengJiu = unit.GetComponent<ChengJiuComponentServer>();
                    chengJiu?.OnGetGold((int)delta);
                    task?.OnCostCoin((int)delta);
                    break;

                case UserDataType.Combat:
                    task = unit.GetComponent<TaskComponentServer>();
                    chengJiu = unit.GetComponent<ChengJiuComponentServer>();
                    chengJiu?.OnCombatToValue(roleInfo.Combat);
                    task?.OnCombatToValue(roleInfo.Combat);
                    break;
            }
        }

        /// <summary>
        /// 货币 GetWay 历史环缓冲（防作弊溯源）。
        /// </summary>
        public static void RecordMoneyGetWay(RoleInfo roleInfo, int type, int getWay)
        {
            if (roleInfo == null)
            {
                return;
            }
        }
    }
}
