using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    //[MessageHandler(AppType.Gate)]
    public static class Function_Fight
    {

        public  static M2C_UnitNumericListUpdate m2C_UnitNumericListUpdate = new M2C_UnitNumericListUpdate();
        
        //暴击等级等属性转换成实际暴击率的方法
        public static float LvProChange(long value, int lv)
        {
            float proValue = (float)value / (float)(7500 + lv * 250);
            if (proValue < 0)
            {
                proValue = 0;
            }
            if (proValue > 0.75f)
            {
                proValue = 0.75f;
            }
            return proValue;
        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的攻击加成
        public static float GetFightValueActProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.75f)
            {
                addPro = 0.75f;
            }

            return addPro;

        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的命中和攻击
        public static float GetFightValueCriAndHitProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.2f)
            {
                addPro = 0.2f;
            }

            return addPro;

        }

        //字典是引用,进来的值会发生改变
        public static void AddUpdateProDicList(int typeID, long typeValue, Dictionary<int, long> dic)
        {
            AttrConfigManager.MergeAttributeValue(typeID, typeValue, dic);
        }

        //是否是一级属性
        public static bool ifNumTypeOnePro(int numericType)
        {

            if (numericType < (int)NumericType.Max)
            {
                numericType = numericType * 100;
            }
            int nowValue = (int)numericType / 100;
            if (nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 大恶魔  ...血量提升30倍,攻击提升200%，移动速度变为10，自身会变成恶魔模型
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public static void UnitUpdateProperty_DemonBig(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 100000, notice);

            ///可以修改属性乘法 属性附属乘法.     
            //numericComponent.Set(NumericType.Numeric_Error, 0, notice);
        }

        /// <summary>
        /// 小恶魔
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public static void UnitUpdateProperty_DemonLittle(Unit unit, bool notice)
        {
          
        }

        /// <summary>
        /// 幽灵
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public  static void UnitUpdateProperty_DemonGhost(Unit unit, bool notice)
        {
           
        }


        /// <summary>
        /// 奔跑大赛属性
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public  static void UnitUpdateProperty_RunRace(Unit unit, bool notice)
        {
          
        }

        /// <summary>
        /// 更新基础的属性
        /// </summary>
        public static void UnitUpdateProperty_Base(Unit unit, bool notice, bool rank)
        {
            if (unit.SceneType == MapTypeEnum.RunRace)
            {
                return;
            }

            RoleInfoComponentServer unitInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = unitInfoComponentServer.RoleInfo;
            int roleLv = roleInfo.Lv;

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ResetProperty();

            Dictionary<int, long> updateProDicList = new Dictionary<int, long>();

            // 职业初始属性
            List<AttributeItem> attributeList = new List<AttributeItem>();
            attributeList.AddRange(LDOccupationCategory.Instance.GetOccInitAttribute(roleInfo.Occ));

            // 装备属性
            unit.GetComponent<BagComponentServer>().GetEquipAttribute(attributeList);
            NumericInitHelper.MergeAttributes(attributeList, updateProDicList);

            // 属性点：初始点 + 等级固定点 + 已分配自由点 → 战斗属性
            int[] initPoints = RoleAddPointHelper.GetInitPoints();
            int[] fixedPointByLevel = RoleAddPointHelper.GetCumulativeFixedPointsByLevel(roleLv);
            int[] pointValues = new int[RoleAddPointHelper.PointNumericTypes.Length];
            for (int i = 0; i < RoleAddPointHelper.PointNumericTypes.Length; i++)
            {
                pointValues[i] = initPoints[i]
                    + fixedPointByLevel[i]
                    + numericComponent.GetAsInt(RoleAddPointHelper.PointNumericTypes[i]);
            }

            Dictionary<int, double> pointConvertAttrs = RolePointConvertHelper.CalcAllConvertAttributes(pointValues);
            foreach (KeyValuePair<int, double> kv in pointConvertAttrs)
            {
                AttrConfigManager.MergeAttributeValue(kv.Key, kv.Value, updateProDicList);
            }

            // 体（Point_Ti）→ 标准生命 × 职业 Hp_Param → 生命上限
            int occupationId = RoleAddPointHelper.GetOccupationId(roleInfo);
            int bodyPoints = RolePointConvertHelper.GetBodyPointCount(pointValues);
            double roleHpFixed = RolePointConvertHelper.CalcRoleHpFixed(roleLv, bodyPoints, occupationId);
            AttrConfigManager.MergeAttributeValue(NumericType.HP_Fixed_11, roleHpFixed, updateProDicList);

            // 批量写入分项属性，每个基础属性只重算一次
            numericComponent.ApplyAttributeDictionary(updateProDicList, false);

            if (notice)
            {
                SendBaseAttributeListUpdate(unit, numericComponent);
            }

            UpdateCombat(unit, numericComponent, notice);


            float hpcur = numericComponent.GetAsFloat(NumericType.HP_Max_10);


            if (rank)
            {
                unit.GetComponent<RoleInfoComponentServer>().UpdateRankInfo();
            }
        }

        /// <summary>
        /// 同步 ForwardMap 中所有基础属性的最终值到客户端。
        /// </summary>
        private static void SendBaseAttributeListUpdate(Unit unit, NumericComponent numericComponent)
        {
            List<int> ks = new List<int>();
            List<long> vs = new List<long>();
            foreach (int baseAttr in AttrConfigManager.ForwardMap.Keys)
            {
                ks.Add(baseAttr);
                vs.Add(numericComponent.GetAsLong(baseAttr));
            }

            m2C_UnitNumericListUpdate.UnitID = unit.Id;
            m2C_UnitNumericListUpdate.Ks = ks;
            m2C_UnitNumericListUpdate.Vs = vs;
            MessageHelper.SendToClient(unit, m2C_UnitNumericListUpdate);
        }
        
        public  static void UpdateCombat(Unit unit, NumericComponent numericComponent, bool notice)
        {
            //战力计算

            int zhanliValue = RandomHelper.RandomNumber(100, 200);
            //更新战力
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.Combat, zhanliValue.ToString(), notice);

            if (zhanliValue < 0 || zhanliValue > 500000)
            {
                Log.Error($"战力异常: {unit.DomainZone()}  {unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name}  {zhanliValue}");
            }

        }
    }


}
