using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ET
{
    public static class BuffHandlerSystem
    {

        public static void OnBaseBuffInit(this BuffHandler self, BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto)
        {
            self.PassTime = 0;
            self.IsTrigger = false;
            self.BuffData = buffData;
            self.TheUnitFrom = theUnitFrom;
            self.TheUnitBelongto = theUnitBelongto;
            self.BuffState = BuffState.Running;
            self.BeginTime = TimeHelper.ServerNow();
            self.MLdSkillConf = LDSkillCategory.Instance.Get(buffData.SkillId);
            self.MBuff = LDSkillBuffCategory.Instance.Get(buffData.BuffId);
            self.DelayTime = self.MBuff.BuffDelayTime;
            self.BuffEndTime = CheckBuffTime(theUnitBelongto, self.MBuff) + 1000 * (int)self.GetTianfuProAdd((int)BuffAttributeEnum.AddBuffTime) + TimeHelper.ServerNow();
            self.BuffEndTime = buffData.BuffEndTime > 0 ? buffData.BuffEndTime : self.BuffEndTime;
            self.InterValTime = self.MBuff.BuffLoopTime * 1000;
            self.InterValTimeBegin = TimeHelper.ServerNow();
            self.NowBuffValue = 0f;
        }

        /// <summary>
        /// 返回毫秒
        /// </summary>
        /// <param name="theUnitBelongto"></param>
        /// <param name="skillBuffConfig"></param>
        /// <returns></returns>
        public static int CheckBuffTime(Unit theUnitBelongto, LDSkillBuff ldSkillBuff)
        {
            int buffTime = ldSkillBuff.BuffTime;
            if ( (ldSkillBuff.BuffType == 2 && ldSkillBuff.buffParameterType == 7)
                ||  ldSkillBuff.BuffScript.Equals("RoleBuff_Bounce"))
            {
                //韧性缩短眩晕时间
                NumericComponent numericComponent = theUnitBelongto.GetComponent<NumericComponent>();
                float addResPro = numericComponent.GetAsFloat(NumericType.Numeric_Error);

                //最多抵抗一半
                if (addResPro >= 0.5f)
                {
                    addResPro = 0.5f;
                }

                buffTime = (int)((float)buffTime * (1f - addResPro));
            }
            return buffTime;
        }

        public static float GetTianfuProAdd(this BuffHandler self, int key)
        {
            SkillSetComponentServer skillSetComponentServer = self.TheUnitFrom.GetComponent<SkillSetComponentServer>();
            if (skillSetComponentServer == null)
                return 0f;

            float addValue = 0f;
            Dictionary<int, float> keyValuePairs = skillSetComponentServer.GetBuffPropertyAdd(self.MBuff.Id);
            if (keyValuePairs == null)
                return addValue;
            keyValuePairs.TryGetValue(key, out addValue);
            return addValue;
        }
    }
}
