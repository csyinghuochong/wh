using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 角色静态属性组装（不含战斗 Buff 层）。
    /// 输出：分项存储ID → 累加后的存储值，交给 NumericComponent.ApplyAttributeDictionary。
    ///
    /// 流水线：
    /// 1) 职业初始 + 装备
    /// 2) 六维加点（含称号六维） → 战斗分项
    /// 3) 体 → 生命上限固定值
    /// 4) 称号非六维属性
    /// 5) 坐骑列表按骑乘/非骑乘比例叠加
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
            MergeTitleAttributes(unit, pointValues, dic);
            MergePointConvertAttrs(pointValues, dic);
            MergeBodyHpFixed(roleInfo, roleInfo.Lv, pointValues, dic);
            MergeMountAttributes(unit, dic);

            return dic;
        }

        /// <summary>MountComponent 坐骑列表：骑乘中的坐骑用骑乘比例，其余用非骑乘比例。</summary>
        static void MergeMountAttributes(Unit unit, Dictionary<int, long> dic)
        {
            MountComponentServer mountComponent = unit.GetComponent<MountComponentServer>();
            if (mountComponent == null)
            {
                return;
            }

            MountHelper.MergeMountListAttributes(mountComponent.GetAllMounts(), dic);
        }

        /// <summary>TitleList 全部有效称号：六维点并入 pointValues，其余属性写入 dic。</summary>
        static void MergeTitleAttributes(Unit unit, int[] pointValues, Dictionary<int, long> dic)
        {
            TitleComponentServer titleComponent = unit.GetComponent<TitleComponentServer>();
            if (titleComponent == null)
            {
                return;
            }

            List<AttributeItem> items = titleComponent.GetTitlePro();
            if (items == null || items.Count == 0)
            {
                return;
            }

            List<AttributeItem> combatItems = new List<AttributeItem>();
            for (int i = 0; i < items.Count; i++)
            {
                AttributeItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                int pointIndex = GetPointIndex(item.AttributeID);
                if (pointIndex >= 0)
                {
                    pointValues[pointIndex] += (int)item.AttributeValue;
                    continue;
                }

                combatItems.Add(item);
            }

            NumericConvert.MergeAttributes(combatItems, dic);
        }

        static int GetPointIndex(int attributeId)
        {
            for (int i = 0; i < RoleAddPointHelper.PointNumericTypes.Length; i++)
            {
                if (RoleAddPointHelper.PointNumericTypes[i] == attributeId)
                {
                    return i;
                }
            }

            return -1;
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
