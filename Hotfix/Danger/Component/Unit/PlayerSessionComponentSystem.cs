namespace ET
{
    [ObjectSystem]
    public class PlayerSessionComponentAwakeSystem : AwakeSystem<PlayerSessionComponent>
    {
        public override void Awake(PlayerSessionComponent self)
        {
        }
    }

    public static class PlayerSessionComponentSystem
    {
        public static void CheckNumeric(this PlayerSessionComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (roleInfoComponentServer == null)
            {
                return;
            }

            int assigned = RoleAddPointHelper.SumCurrentFreePoints(numericComponent);
            int totalPoint = RoleAddPointHelper.GetTotalPointAtLevel(roleInfoComponentServer.RoleInfo.Lv);
            if (!unit.IsRobot() && assigned > totalPoint)
            {
                Log.Warning($"属性点异常: {unit.DomainZone()} {unit.Id} assigned={assigned} total={totalPoint}");
            }
        }

        public static void OnLogin(this PlayerSessionComponent self, int robotId)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.Set((int)NumericType.Now_Dead, 0, false);
            numericComponent.Set((int)NumericType.Now_Damage, 0, false);
            numericComponent.Set((int)NumericType.TeamId, 0, false);
            numericComponent.Set((int)NumericType.HP_Current_8, numericComponent.GetAsLong((int)NumericType.HP_Max_10), false);
            numericComponent.Set((int)NumericType.Now_Weapon, unit.GetComponent<BagComponentServer>().GetWuqiItemId(), false);
            numericComponent.Set(NumericType.JueXingAnger, 0, false);
            numericComponent.Set(NumericType.RunRaceRankId, 0, false);
            numericComponent.Set(NumericType.ZeroClock, 0, false);

            int yuekatimes = numericComponent.GetAsInt(NumericType.YueKaRemainTimes);
            if (yuekatimes > 0)
            {
                numericComponent.ApplyValue(NumericType.YueKaEndTime, yuekatimes, false);
            }
        }

        public static void OnReturn(this PlayerSessionComponent self)
        {
            Unit unit = self.GetParent<Unit>();
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

        public static void OnResetPoint(this PlayerSessionComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (!RoleAddPointHelper.CanResetPoint(roleInfoComponentServer.RoleInfo.Lv))
            {
                return;
            }

            RoleAddPointHelper.RecalculateAllPoints(unit);
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
        }
    }
}
