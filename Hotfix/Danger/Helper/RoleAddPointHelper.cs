using System;

namespace ET
{
    /// <summary>
    /// 角色属性点（GlobalValue 驱动，Numeric 只存自由分配部分）：
    /// 1. Add_Point_Init — 创角初始点（如每属性 10）
    /// 2. Add_Point_Level_UP_Fixed — 每升 1 级每属性固定 +1，累计 = perLevel × level
    /// 3. Add_Point_Level_UP_Free — 每升 1 级获得自由点；&lt; AutoLevel 按职业 Add_Point_Default 自动分配，否则进 PointRemain
    /// 展示/战斗总点 = Init + Fixed(level) + 已分配自由点
    /// </summary>
    public static class RoleAddPointHelper
    {
        public static readonly int[] PointNumericTypes =
        {
            NumericType.Point_Ti_1,      // 体
            NumericType.Point_Li_2,       // 力
            NumericType.Point_Zhi_3,  // 智
            NumericType.Point_Nian_4,  // 念
            NumericType.Point_Min_5,       // 敏
            NumericType.Point_Xun_6,         // 迅
        };

        public static int GetAutoLevel()
        {
            return LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Add_Point_Auto_Level);
        }

        public static int GetFreePointByLevel(int level)
        {
            return LDGlobalValueCategory.Instance.GetFreePointByLevel(level);
        }

        public static int GetTotalFreePointByLevel(int level)
        {
            return LDGlobalValueCategory.Instance.GetTotalFreePointByLevel(level);
        }

        /// <summary>创角初始属性点（Add_Point_Init）。</summary>
        public static int[] GetInitPoints()
        {
            int[] initPoints = LDGlobalValueCategory.Instance.GetIntArray(GlobalValueKey.Add_Point_Init);
            if (initPoints.Length != PointNumericTypes.Length)
            {
                throw new Exception($"GlobalValue {GlobalValueKey.Add_Point_Init} 长度应为 {PointNumericTypes.Length}");
            }

            return initPoints;
        }

        public static int[] GetFixedPointPerLevel()
        {
            int[] fixedPoints = LDGlobalValueCategory.Instance.GetIntArray(GlobalValueKey.Add_Point_Level_UP_Fixed);
            if (fixedPoints.Length != PointNumericTypes.Length)
            {
                throw new Exception($"GlobalValue {GlobalValueKey.Add_Point_Level_UP_Fixed} 长度应为 {PointNumericTypes.Length}");
            }

            return fixedPoints;
        }

        /// <summary>当前等级累计固定属性点（0→1、1→2… 各加 perLevel，level 1 = 1 点）。</summary>
        public static int[] GetCumulativeFixedPointsByLevel(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            int[] perLevel = GetFixedPointPerLevel();
            int[] cumulative = new int[perLevel.Length];
            for (int i = 0; i < perLevel.Length; i++)
            {
                cumulative[i] = perLevel[i] * level;
            }

            return cumulative;
        }

        public static int GetOccupationId(RoleInfo roleInfo)
        {
            return roleInfo.OccTwo > 0 ? roleInfo.OccTwo : roleInfo.Occ;
        }

        public static int[] GetDefaultFreeDistribution(RoleInfo roleInfo)
        {
            int occupationId = GetOccupationId(roleInfo);
            int[] defaultPoints = LDOccupationCategory.Instance.Get(occupationId).Add_Point_Default;
            if (defaultPoints == null || defaultPoints.Length != PointNumericTypes.Length)
            {
                throw new Exception($"Occupation {occupationId} Add_Point_Default 配置无效");
            }

            return defaultPoints;
        }

        public static bool CanManualAddPoint(int level)
        {
            return level >= GetAutoLevel();
        }

        public static bool CanResetPoint(int level)
        {
            return level >= GetAutoLevel();
        }

        /// <summary>指定等级累计可获得的自由属性点（不含初始点与固定点）。</summary>
        public static int GetTotalPointAtLevel(int level)
        {
            return GetTotalFreePointByLevel(level);
        }

        /// <summary>界面/协议用的展示点数 = 初始 + 累计固定 + Numeric 中已分配自由点。</summary>
        public static int GetDisplayPoint(NumericComponent numericComponent, int level, int statIndex)
        {
            return GetInitPoints()[statIndex]
                + GetCumulativeFixedPointsByLevel(level)[statIndex]
                + numericComponent.GetAsInt(PointNumericTypes[statIndex]);
        }

        /// <summary>根据展示点数反算剩余可分配自由点。</summary>
        public static int GetRemainPoint(int level, int[] displayPoints)
        {
            if (displayPoints == null || displayPoints.Length != PointNumericTypes.Length)
            {
                throw new Exception($"displayPoints 长度应为 {PointNumericTypes.Length}");
            }

            int[] initPoints = GetInitPoints();
            int[] fixedPoints = GetCumulativeFixedPointsByLevel(level);
            int assignedFree = 0;
            for (int i = 0; i < displayPoints.Length; i++)
            {
                assignedFree += displayPoints[i] - initPoints[i] - fixedPoints[i];
            }

            return GetTotalFreePointByLevel(level) - assignedFree;
        }

        public static int SumPoints(int[] points)
        {
            int sum = 0;
            for (int i = 0; i < points.Length; i++)
            {
                sum += points[i];
            }

            return sum;
        }

        public static int SumCurrentFreePoints(NumericComponent numericComponent)
        {
            int sum = 0;
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                sum += numericComponent.GetAsInt(PointNumericTypes[i]);
            }

            sum += numericComponent.GetAsInt(NumericType.PointRemain);
            return sum;
        }

        public static void ReadCurrentPoints(NumericComponent numericComponent, int[] points)
        {
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                points[i] = numericComponent.GetAsInt(PointNumericTypes[i]);
            }
        }

        /// <summary>兼容旧调用：累计固定点。</summary>
        public static int[] GetFixedPointByLevel(int level)
        {
            return GetCumulativeFixedPointsByLevel(level);
        }

        /// <summary>升级时发放 1 级的自由点（固定点由公式实时计算，不入库）。</summary>
        public static void AddPointsOnLevelUp(Unit unit, int newLevel)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            int freePoints = GetFreePointByLevel(newLevel);
            if (freePoints <= 0)
            {
                return;
            }

            if (newLevel <= GetAutoLevel())
            {
                RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
                int[] defaultPoints = GetDefaultFreeDistribution(roleInfo);
                for (int i = 0; i < PointNumericTypes.Length; i++)
                {
                    if (defaultPoints[i] == 0)
                    {
                        continue;
                    }

                    numericComponent.ApplyValue(PointNumericTypes[i], numericComponent.GetAsInt(PointNumericTypes[i]) + defaultPoints[i], true);
                }
            }
            else
            {
                numericComponent.ApplyValue(NumericType.PointRemain, numericComponent.GetAsInt(NumericType.PointRemain) + freePoints, true);
            }
        }

        /// <summary>对 (oldLevel, newLevel] 每一级发放自由点。</summary>
        public static void AddPointsForLevelRange(Unit unit, int oldLevel, int newLevel)
        {
            for (int lv = oldLevel + 1; lv <= newLevel; lv++)
            {
                AddPointsOnLevelUp(unit, lv);
            }
        }

        /// <summary>1 级且六维全 0 时补发一级自由点（登录 CheckData 修复旧号）。</summary>
        public static void EnsureLevel1InitPoints(Unit unit, int level)
        {
            if (level != 1)
            {
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                if (numeric.GetAsInt(PointNumericTypes[i]) != 0)
                {
                    return;
                }
            }

            AddPointsOnLevelUp(unit, 1);
        }

        /// <summary>按当前等级重算全部自由属性点（洗点 / 数据校验修复）。</summary>
        public static void RecalculateAllPoints(Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int level = roleInfo.Lv;
            int autoLevel = GetAutoLevel();
            int[] defaultPoints = GetDefaultFreeDistribution(roleInfo);

            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                numericComponent.ApplyValue(PointNumericTypes[i], 0, false);
            }

            numericComponent.ApplyValue(NumericType.PointRemain, 0, false);

            for (int lv = 1; lv < level; lv++)
            {
                int afterLevel = lv + 1;
                int freePoints = GetFreePointByLevel(afterLevel);
                if (freePoints <= 0)
                {
                    continue;
                }

                if (afterLevel <= autoLevel)
                {
                    for (int i = 0; i < PointNumericTypes.Length; i++)
                    {
                        if (defaultPoints[i] != 0)
                        {
                            numericComponent.ApplyValue(PointNumericTypes[i], numericComponent.GetAsInt(PointNumericTypes[i]) + defaultPoints[i], false);
                        }
                    }
                }
                else
                {
                    numericComponent.ApplyValue(NumericType.PointRemain, numericComponent.GetAsInt(NumericType.PointRemain) + freePoints, false);
                }
            }
        }
    }
}
