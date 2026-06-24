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
            if (numericComponent.GetAsInt(NumericType.Point_Strength) == 0
                && numericComponent.GetAsInt(NumericType.Point_Agility) == 0
                && numericComponent.GetAsInt(NumericType.Point_Intelligence) == 0
                && numericComponent.GetAsInt(NumericType.Point_Constitution) == 0
                && numericComponent.GetAsInt(NumericType.Point_Stamina) == 0)
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

            long maxHp = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.HP_Max);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.HP_Current, maxHp, true);

            unit.GetComponent<TaskComponent>().OnUpdateLevel(newLevel);
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
