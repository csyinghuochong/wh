namespace ET
{
    public static class UpgrageLevelHelper
    {

        public static void CheckInitPoint(this Unit unit, int newLevel)
        {
            if (newLevel != 1)
            {
                return;
            }

            //没有给默认属性点
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int point_1 = numericComponent.GetAsInt(NumericType.Point_Ti_1);
            int point_2 = numericComponent.GetAsInt(NumericType.Point_Li_2);
            int point_3 = numericComponent.GetAsInt(NumericType.Point_Zhi_3);
            int point_4 = numericComponent.GetAsInt(NumericType.Point_Nian_4);
            int point_5 = numericComponent.GetAsInt(NumericType.Point_Min_5);
            int point_6 = numericComponent.GetAsInt(NumericType.Point_Xun_6);
            if (point_1 == 0  && point_1 == 0  && point_3 == 0
                && point_4 == 0  && point_5 == 0 && point_6 == 0)
            {
                RoleAddPointHelper.AddPointsOnLevelUp(unit, 1);
            }
        }

        public static void OnUpgrageLevel(this Unit unit, int newLevel, int oldLevel)
        {
            for (int lv = oldLevel + 1; lv <= newLevel; lv++)
            {
                RoleAddPointHelper.AddPointsOnLevelUp(unit, lv);
            }

            long maxHp = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.HP_Max_10);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.HP_Current_8, maxHp, true);

            unit.GetComponent<TaskComponentServer>().OnUpdateLevel(newLevel);
            unit.GetComponent<ChengJiuComponentServer>().OnUpdateLevel(newLevel);
        }

        public static void OnUpgrageLevel(this Unit unit, int newLevel)
        {
            int oldLevel = newLevel - 1;
            if (oldLevel < 1)
            {
                oldLevel = 1;
            }

            unit.OnUpgrageLevel(newLevel, oldLevel);
        }
    }
}
