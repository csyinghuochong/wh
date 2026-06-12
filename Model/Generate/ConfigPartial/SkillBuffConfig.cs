using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public partial class LDSkillBuffCategory
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
        
        public override void AfterEndInit()
        {
            foreach (LDSkillBuff skillBuffConfig in this.GetAll().Values)
            {
                try
                {
                    if (skillBuffConfig.BuffType != 6 &&  !string.IsNullOrEmpty(skillBuffConfig.buffParameterValue2))
                    {
                        List<int> buffIds = new List<int>();
                        string[] ids = skillBuffConfig.buffParameterValue2.Split(',');
                        foreach (string id in ids)
                        {
                            if (!int.TryParse(id, out int buffId))
                            {
                                Log.Error($"int.TryParse error: {id} skillBuffId:{skillBuffConfig.Id}");
                                continue;
                            }
                            buffIds.Add(buffId);
                        }
                        this.RelieveBuffList.Add(skillBuffConfig.Id, buffIds);
                    }
                }
                catch (Exception ex) 
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}