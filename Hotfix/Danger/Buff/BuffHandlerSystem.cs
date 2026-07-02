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
            self.MBuff = LDSkill_BuffCategory.Instance.Get(buffData.BuffId);
            //self.DelayTime = self.MBuff.BuffDelayTime;
            self.BuffEndTime = CheckBuffTime(theUnitBelongto, self.MBuff) + 1000 * (int)self.GetTianfuProAdd((int)BuffAttributeEnum.AddBuffTime) + TimeHelper.ServerNow();
            self.BuffEndTime = buffData.BuffEndTime > 0 ? buffData.BuffEndTime : self.BuffEndTime;
            //self.InterValTime = self.MBuff.BuffLoopTime * 1000;
            self.InterValTimeBegin = TimeHelper.ServerNow();
            self.NowBuffValue = 0f;
        }

        /// <summary>
        /// 返回毫秒
        /// </summary>
        /// <param name="theUnitBelongto"></param>
        /// <param name="skillBuffConfig"></param>
        /// <returns></returns>
        public static int CheckBuffTime(Unit theUnitBelongto, LDSkill_Buff ldSkillBuff)
        {
           
            return 0;
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
