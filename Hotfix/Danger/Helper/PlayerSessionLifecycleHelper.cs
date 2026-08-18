using System;

namespace ET
{
    /// <summary>
    /// 会话生命周期：进游戏编排 / 登录 / 重登 / 离线 / 断线。
    /// DBSave 仍负责 UpdateCacheDB / Remove。
    /// </summary>
    public static class PlayerSessionLifecycleHelper
    {
        /// <summary>
        /// 进游戏总入口：跨天、各系统 OnLogin 扇出。
        /// </summary>
        public static void OnLogin(this Unit unit, string remoteip)
        {
            long currentTime = TimeHelper.ServerNow();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            roleInfoComponentServer.OnLogin(remoteip);

            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            PlayerDailyResetHelper.LoginCheckCrossDay(unit, currentTime);

            unit.GetComponent<BagComponentServer>().OnLogin(roleInfo.RobotId, roleInfo.Occ, roleInfo.OccTwo);
            unit.GetComponent<TaskComponentServer>().OnLogin();
            unit.GetComponent<DBSaveComponent>().OnLogin();
            unit.GetComponent<RechargeComponentServer>().OnLogin();
            unit.GetComponent<PetComponentServer>().OnLogin();
            unit.GetComponent<ActivityComponentServer>().OnLogin(roleInfo.Lv);
            unit.GetComponent<TitleComponentServer>().OnCheckTitle(false);
            unit.GetComponent<ChengJiuComponentServer>().OnLogin();
            unit.GetComponent<JiaYuanComponentServer>().OnLogin();
            unit.GetComponent<SkillSetComponentServer>().OnLogin(roleInfo.Occ);
            // RoleDailyData 全量由客户端 LoginHelper 请求，此处只做迁移准备
            unit.GetComponent<RoleDailyDataComponentServer>().OnLogin();
            unit.ResetLoginNumeric();
            roleInfoComponentServer.LastLoginTime = currentTime;
        }

        public static void CheckNumeric(this Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            int assigned = RoleAddPointHelper.SumCurrentFreePoints(numericComponent);
            int totalPoint = RoleAddPointHelper.GetTotalPointAtLevel(roleInfoComponentServer.RoleInfo.Lv);
            if (!unit.IsRobot() && assigned > totalPoint)
            {
                Log.Warning($"属性点异常: {unit.DomainZone()} {unit.Id} assigned={assigned} total={totalPoint}");
            }
        }

        public static void ResetLoginNumeric(this Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.Set((int)NumericType.Now_Dead, 0, false);
            numericComponent.Set((int)NumericType.Now_Damage, 0, false);
            numericComponent.Set((int)NumericType.TeamId, 0, false);
            numericComponent.Set((int)NumericType.HP_Current_8, numericComponent.GetAsLong((int)NumericType.HP_Max_10), false);
            numericComponent.Set((int)NumericType.Now_Weapon, unit.GetComponent<BagComponentServer>().GetWuqiItemId(), false);
            numericComponent.Set(NumericType.ZeroClock, 0, false);

            int yuekatimes = numericComponent.GetAsInt(NumericType.YueKaRemainTimes);
            if (yuekatimes > 0)
            {
                numericComponent.ApplyValue(NumericType.YueKaEndTime, yuekatimes, false);
            }
        }

        public static void OnReturn(this Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.SetValueNoSync(NumericType.Now_Dead, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Damage, 0);
            numericComponent.SetValueNoSync(NumericType.BossBelongID, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_HP, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_MaxHP, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_DamgeCostPro, 0);
            if (numericComponent.GetAsLong(NumericType.Now_Dead) <= 0)
            {
                long maxHp = numericComponent.GetAsLong(NumericType.HP_Max_10);
                numericComponent.SetValueNoSync(NumericType.HP_Current_8, maxHp);
            }
        }

        public static void OnResetPoint(this Unit unit)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (!RoleAddPointHelper.CanResetPoint(roleInfoComponentServer.RoleInfo.Lv))
            {
                return;
            }

            RoleAddPointHelper.RecalculateAllPoints(unit);
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
        }

        public static void OnLogin(DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            AntiCheatAuditHelper.LogSession(unit, roleInfo, "登录");
            if (!unit.IsRobot())
            {
                self.LogTest();
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ApplyValue(NumericType.LastGameTime, TimeHelper.ServerNow(), false);
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.Game;
        }

        public static void OnRelogin(DBSaveComponent self, long gateSessionId)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            AntiCheatAuditHelper.LogSession(unit, roleInfo, "二次登陆");
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.Game;
        }

        public static void OnOffLine(DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
           
            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            string oaid = dataCollationComponent.OAID;
            string lastgametime = TimeHelper.DateTimeNow().ToString();
            numericComponent.ApplyValue(NumericType.LastGameTime, TimeHelper.ServerNow(), false);
            roleInfoComponentServer.OnOffLine();
            dataCollationComponent.OnOffLine(lastgametime);
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.None;

            AntiCheatAuditHelper.LogSession(unit, roleInfoComponentServer, "离线");
            if (!unit.IsRobot())
            {
                self.UpdateCacheDB();
                DBHelper.UpdateLastGameTime(oaid,
                    lastgametime,
                    roleInfoComponentServer.RoleInfo.AccInfoID,
                    roleInfoComponentServer.RemoteAddress,
                    roleInfoComponentServer.RoleInfo.Lv,
                    self.OnLineTime).Coroutine();
            }
        }

        public static int OnDisconnect(DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            Scene scene = unit.DomainScene();
            int sceneTypeEnum = scene.GetComponent<MapComponent>().MapTypeEnum;
            if (sceneTypeEnum == MapTypeEnum.MainCityScene)
            {
                unit.RecordPostion(sceneTypeEnum, CommonHelper.MainCityID());
            }

            TransferHelper.BeforeTransfer(unit, 2);
            if (!unit.IsRobot())
            {
                self.LogTest();
                self.UpdateCacheDB();
                AntiCheatAuditHelper.LogSession(unit, roleInfo, "移除");
            }

            long unitId = unit.Id;
            unit.GetParent<UnitComponent>().Remove(unitId);
            Game.EventSystem.Publish(new EventType.PlayerDisconnect() { DomainScene = scene, UnitId = unitId });
            return ErrorCode.ERR_Success;
        }
    }
}
