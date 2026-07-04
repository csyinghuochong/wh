using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Buff / 被动 修改战斗属性（战斗层 Numeric，非 HeroData）。
    /// </summary>
    public static class FightBuffNumericHelper
    {
        public static void BuffPropertyUpdate_Long(Unit unit, int numericType, long value)
        {
            if (unit == null || value == 0)
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            if (AttrConfigManager.TryGetBaseAttrsForFightChange(numericType, out List<int> baseAttrs))
            {
                foreach (int baseAttr in baseAttrs)
                {
                    numericComponent.ApplyFightFixedChange(null, baseAttr, value, 0);
                }

                return;
            }

            long newValue = numericComponent.GetAsLong(numericType) + value;
            numericComponent.Set(numericType, newValue);
        }

        public static void BuffPropertyUpdate_Float(Unit unit, int numericType, float value)
        {
            if (unit == null || value == 0f)
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            if (AttrConfigManager.TryGetBaseAttrsForFightChange(numericType, out List<int> baseAttrs))
            {
                foreach (int baseAttr in baseAttrs)
                {
                    numericComponent.ApplyFightPercentChange(null, baseAttr, value, 0);
                }

                return;
            }

            float newValue = numericComponent.GetAsFloat(numericType) + value;
            numericComponent.Set(numericType, newValue);
        }
    }
}
