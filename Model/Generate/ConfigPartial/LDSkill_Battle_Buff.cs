using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDSkill_Battle_BuffCategory
    {
        // 该buff可以解除的buff Id
        public Dictionary<int, List<int>> RelieveBuffList = new Dictionary<int, List<int>>();

        /// <summary>
        /// 获取该buff可以解除的状态
        /// </summary>
        /// <param name="buffId"></param>
        /// <returns></returns>
        public List<int> GetRelieveBuffs(int buffId)
        {
            List<int> relieveBuffs = new List<int>();
            this.RelieveBuffList.TryGetValue(buffId, out relieveBuffs);
            return relieveBuffs;
        }
        
    }
}