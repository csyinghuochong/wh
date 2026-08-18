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
        /// 进游戏总入口：组件补齐、跨天、各系统 OnLogin 扇出。
        /// </summary>
        public static void OnLogin(this Unit unit, string remoteip)
        {
            UnitComponentEnsureHelper.EnsurePlayerComponents(unit);

            long currentTime = TimeHelper.ServerNow();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            roleInfoComponentServer.OnLogin(remoteip);

            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            PlayerDailyResetHelper.RunLoginCrossDay(unit, currentTime);
            roleInfoComponentServer.LastLoginTime = currentTime;

            unit.GetComponent<BagComponentServer>().OnLogin(roleInfo.RobotId, roleInfo.Occ, roleInfo.OccTwo);
            unit.GetComponent<TaskComponentServer>().OnLogin();
            unit.GetComponent<PlayerSessionComponent>()?.OnLogin(roleInfo.RobotId);
            unit.GetComponent<DBSaveComponent>().OnLogin();
            unit.GetComponent<RechargeComponentServer>().OnLogin();
            unit.GetComponent<PetComponentServer>().OnLogin();
            unit.GetComponent<ActivityComponentServer>().OnLogin(roleInfo.Lv);
            unit.GetComponent<TitleComponentServer>().OnCheckTitle(false);
            unit.GetComponent<ChengJiuComponentServer>().OnLogin();
            unit.GetComponent<JiaYuanComponentServer>().OnLogin();
            unit.GetComponent<SkillSetComponentServer>().OnLogin(roleInfo.Occ);
            // RoleDailyData 全量由客户端 LoginHelper 请求，此处只做迁移准备
            unit.GetComponent<RoleDailyDataComponentServer>()?.OnLogin();
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
