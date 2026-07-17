using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 角色静态属性组装（不含战斗 Buff 层）。
    /// 输出：分项存储ID → 累加后的存储值，交给 NumericComponent.ApplyAttributeDictionary。
    ///
    /// 流水线：
    /// 1) 职业初始 + 装备
    /// 2) 六维加点 → 战斗分项
    /// 3) 体 → 生命上限固定值
    /// </summary>
    public static class UnitStaticAttrBuilder
    {
        public static Dictionary<int, long> Build(Unit unit)
        {
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            Dictionary<int, long> dic = new Dictionary<int, long>();

            MergeOccupationAndEquip(unit, roleInfo.Occ, dic);
            int[] pointValues = CalcTotalPointValues(numeric, roleInfo.Lv);
            MergePointConvertAttrs(pointValues, dic);
            MergeBodyHpFixed(roleInfo, roleInfo.Lv, pointValues, dic);

            return dic;
        }

        /// <summary>职业初始属性 + 当前装备属性。</summary>
        public static void MergeOccupationAndEquip(Unit unit, int occ, Dictionary<int, long> dic)
        {
            List<AttributeItem> items = new List<AttributeItem>();
            items.AddRange(LDOccupationCategory.Instance.GetOccInitAttribute(occ));
            unit.GetComponent<BagComponentServer>().GetEquipAttribute(items);
            NumericConvert.MergeAttributes(items, dic);
        }

        /// <summary>总点数 = 创角初始 + 等级固定 + 已分配自由点。</summary>
        public static int[] CalcTotalPointValues(NumericComponent numeric, int roleLv)
        {
            int[] initPoints = RoleAddPointHelper.GetInitPoints();
            int[] fixedByLevel = RoleAddPointHelper.GetCumulativeFixedPointsByLevel(roleLv);
            int[] pointValues = new int[RoleAddPointHelper.PointNumericTypes.Length];

            for (int i = 0; i < RoleAddPointHelper.PointNumericTypes.Length; i++)
            {
                int freeAssigned = numeric.GetAsInt(RoleAddPointHelper.PointNumericTypes[i]);
                pointValues[i] = initPoints[i] + fixedByLevel[i] + freeAssigned;
            }

            return pointValues;
        }

        /// <summary>六维加点转换成攻击/防御/命中等分项（不含体→生命）。</summary>
        public static void MergePointConvertAttrs(int[] pointValues, Dictionary<int, long> dic)
        {
            Dictionary<int, double> convertAttrs = RolePointConvertHelper.CalcAllConvertAttributes(pointValues);
            foreach (KeyValuePair<int, double> kv in convertAttrs)
            {
                AttrConfigManager.MergeAttributeValue(kv.Key, kv.Value, dic);
            }
        }

        /// <summary>体点数 → 生命上限固定值（HP_Fixed）。</summary>
        public static void MergeBodyHpFixed(RoleInfo roleInfo, int roleLv, int[] pointValues, Dictionary<int, long> dic)
        {
            int occupationId = RoleAddPointHelper.GetOccupationId(roleInfo);
            int bodyPoints = RolePointConvertHelper.GetBodyPointCount(pointValues);
            double roleHpFixed = RolePointConvertHelper.CalcRoleHpFixed(roleLv, bodyPoints, occupationId);
            AttrConfigManager.MergeAttributeValue(NumericType.HP_Fixed_11, roleHpFixed, dic);
        }
    }
}
