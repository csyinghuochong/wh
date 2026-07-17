using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    //[MessageHandler(AppType.Gate)]
    public static class Function_Fight
    {

        /// <summary>
        /// 全量重算角色静态战斗属性（职业+装备+加点），保留战斗 Buff 层。
        /// </summary>
        public static void UnitUpdateProperty_Base(Unit unit, bool notice, bool rank)
        {
            if (unit.SceneType == MapTypeEnum.RunRace)
            {
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();

            // notice 时先快照一级属性，Reset 后再对比，避免无变化也推客户端
            Dictionary<int, long> beforeBaseAttrs = notice ? SnapshotBaseAttrs(numeric) : null;

            // 1. 清静态分项/一级结果（保留加点、运行时状态、战斗 Buff）
            numeric.ResetProperty();

            // 2. 组装分项字典 → 写入并重算一级属性
            Dictionary<int, long> staticAttrs = UnitStaticAttrBuilder.Build(unit);
            numeric.ApplyAttributeDictionary(staticAttrs, false);

            // 3. 只推有变化的一级属性
            if (notice)
            {
                SendChangedBaseAttributeListUpdate(unit, numeric, beforeBaseAttrs);
            }

            // 4. 刷新战力
            UpdateCombat(unit, numeric, notice);
        }

        /// <summary>快照 ForwardMap 一级属性存储值（Reset 前调用）。</summary>
        private static Dictionary<int, long> SnapshotBaseAttrs(NumericComponent numeric)
        {
            Dictionary<int, long> snap = new Dictionary<int, long>(AttrConfigManager.ForwardMap.Count);
            foreach (int baseAttr in AttrConfigManager.ForwardMap.Keys)
            {
                snap[baseAttr] = numeric.GetStoredValue(baseAttr);
            }

            return snap;
        }

        /// <summary>
        /// 对比快照，只同步有变化的一级属性存储值；全无变化则不发包。
        /// 必须发 GetStoredValue（NumericDic 原值：固定原样、千分比已是 ×1000），客户端 SetValueNoSync 直接写入。
        /// </summary>
        private static void SendChangedBaseAttributeListUpdate(Unit unit, NumericComponent numeric, Dictionary<int, long> before)
        {
            List<int> ks = new List<int>();
            List<long> vs = new List<long>();
            foreach (int baseAttr in AttrConfigManager.ForwardMap.Keys)
            {
                long now = numeric.GetStoredValue(baseAttr);
                if (before == null || !before.TryGetValue(baseAttr, out long old) || old != now)
                {
                    ks.Add(baseAttr);
                    vs.Add(now);
                }
            }

            if (ks.Count == 0)
            {
                return;
            }

            MessageHelper.SendToClient(unit, new M2C_UnitNumericListUpdate
            {
                UnitID = unit.Id,
                Ks = ks,
                Vs = vs,
            });
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
