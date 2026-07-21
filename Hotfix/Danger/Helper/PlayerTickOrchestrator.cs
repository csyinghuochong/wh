using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 玩家秒/分 Tick 编排：从 DBSave 主逻辑剥离，DBSave 只负责存盘与会话。
    /// </summary>
    public static class PlayerTickOrchestrator
    {
        public static void RunSecondTick(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            if (unitGateComponent != null && unitGateComponent.PlayerState != PlayerState.None)
            {
                unit.GetComponent<ActivityComponentServer>()?.Check();
            }
        }

        public static void RunMinuteTick(Unit unit, DBSaveComponent dbSave)
        {
            if (unit == null || unit.IsDisposed || dbSave == null)
            {
                return;
            }

            int saveInterval = RandomHelper.RandomNumber(20, 30);
            if (dbSave.DBInterval == -1 || dbSave.DBInterval >= saveInterval)
            {
                dbSave.DBInterval = 0;
                dbSave.UpdateCacheDB();
            }
            dbSave.DBInterval++;
            dbSave.OnLineTime++;

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            taskComponentServer?.Check();
            unit.GetComponent<RoleInfoComponentServer>()?.Check();
            unit.GetComponent<DataCollationComponent>()?.Check();
            unit.GetComponent<TitleComponentServer>()?.OnCheckTitle(true);
        }
    }
}
