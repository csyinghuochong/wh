using System;

namespace ET
{
    /// <summary>
    /// 角色属性点：固定加点、自由加点、自动分配与洗点。
    /// 属性顺序：力量|敏捷|智力|体质|耐力，对应 NumericType.Point_Strength ~ Point_Stamina。
    /// </summary>
    public static class RoleAddPointHelper
    {
        public static readonly int[] PointNumericTypes =
        {
            NumericType.Point_Strength,
            NumericType.Point_Agility,
            NumericType.Point_Intelligence,
            NumericType.Point_Constitution,
            NumericType.Point_Stamina,
            NumericType.Point_Haste,
        };

        public static int GetAutoLevel()
        {
            return LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Add_Point_Auto_Level);
        }

        public static int GetFreePointPerLevel()
        {
            return LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Add_Point_Level_UP_Free);
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

        /// 每升 1 级增加固定 + 自由部分。</summary>
        public static int GetTotalPointAtLevel(int level)
        {
            if (level <= 0)
            {
                return 0;
            }

            int levelUpCount = level;
            int fixedSum = 0;
            foreach (int point in GetFixedPointPerLevel())
            {
                fixedSum += point;
            }

            return levelUpCount * (fixedSum + GetFreePointPerLevel());
        }

        public static int GetRemainPoint(int lv, int[] assignedPoints)
        {
            int gettotalPoint = GetTotalPointAtLevel(lv);
            int getsumPoints = SumPoints(assignedPoints);
            return gettotalPoint - getsumPoints;
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

        public static int SumCurrentPoints(NumericComponent numericComponent)
        {
            int sum = 0;
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                sum += numericComponent.GetAsInt(PointNumericTypes[i]);
            }

            return sum;
        }

        public static void ReadCurrentPoints(NumericComponent numericComponent, int[] points)
        {
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                points[i] = numericComponent.GetAsInt(PointNumericTypes[i]);
            }
        }
        

        public static int[] GetFixedPointByLevel(int newLevel)
        {
            int[] fixedPoints = GetFixedPointPerLevel();
            for (int i = 0;i < fixedPoints.Length; i++)
            {
                fixedPoints[i] *= newLevel;
            }

            return fixedPoints;
        }

        /// <summary>升级时增加 1 级的属性点。</summary>
        public static void AddPointsOnLevelUp(Unit unit, int newLevel)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            /*int[] fixedPoints = GetFixedPointPerLevel();
            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                if (fixedPoints[i] == 0)
                {
                    continue;
                }

                numericComponent.ApplyValue(PointNumericTypes[i], numericComponent.GetAsInt(PointNumericTypes[i]) + fixedPoints[i], true);
            }*/

            int freePoints = GetFreePointPerLevel();
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

        /// <summary>按当前等级重算全部属性点（洗点 / 数据校验修复）。</summary>
        public static void RecalculateAllPoints(Unit unit)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int level = roleInfo.Lv;
            int autoLevel = GetAutoLevel();
            int[] fixedPoints = GetFixedPointPerLevel();
            int freePoints = GetFreePointPerLevel();
            int[] defaultPoints = GetDefaultFreeDistribution(roleInfo);

            for (int i = 0; i < PointNumericTypes.Length; i++)
            {
                numericComponent.ApplyValue(PointNumericTypes[i], 0, false);
            }

            numericComponent.ApplyValue(NumericType.PointRemain, 0, false);

            for (int lv = 1; lv < level; lv++)
            {
                int afterLevel = lv + 1;
                for (int i = 0; i < PointNumericTypes.Length; i++)
                {
                    if (fixedPoints[i] != 0)
                    {
                        numericComponent.ApplyValue(PointNumericTypes[i], numericComponent.GetAsInt(PointNumericTypes[i]) + fixedPoints[i], false);
                    }
                }

                if (freePoints <= 0)
                {
                    continue;
                }

                if (afterLevel < autoLevel)
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
