namespace ET
{
    public static class UpgrageLevelHelper
    {
        public static void OnUpgrageLevel(this Unit unit, int newLevel, int oldLevel)
        {
            for (int lv = oldLevel + 1; lv <= newLevel; lv++)
            {
                RoleAddPointHelper.AddPointsOnLevelUp(unit, lv);
            }

            long maxHp = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.HP_Max);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.HP_Current, maxHp, false);

            unit.GetComponent<TaskComponent>().OnUpdateLevel(newLevel);
            unit.GetComponent<ChengJiuComponent>().OnUpdateLevel(newLevel);
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
