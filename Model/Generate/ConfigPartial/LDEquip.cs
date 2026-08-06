using System;
using System.Collections.Generic;

namespace ET
{
    public partial class LDEquipCategory
    {
        public Dictionary<int, Dictionary<int, int>> Enhance_AttributeList = new Dictionary<int, Dictionary<int, int>>();

        private readonly Dictionary<int, List<AttributeRandom>> EquipAttribute = new Dictionary<int, List<AttributeRandom>>();

        public override void AfterEndInit()
        {
            ParseEquipAttri();
            ParseEnhance_AttributeList();
        }

        private void ParseEquipAttri()
        {
            EquipAttribute.Clear();
            foreach (LDEquip ldEquip in this.GetAll().Values)
            {
                // 23_500~600|24_1000~1200|66_50~60|70_50~60|18_30
                if (string.IsNullOrEmpty(ldEquip.Attribute))
                {
                    continue;
                }

                List<AttributeRandom> equipAttribute = new List<AttributeRandom>();
                string[] attributeList = ldEquip.Attribute.Split('|');
                for (int i = 0; i < attributeList.Length; i++)
                {
                    string attributeInfo = attributeList[i];
                    if (attributeInfo.Length < 2)
                    {
                        continue;
                    }

                    string[] attributeInfolist = attributeInfo.Split('_');
                    if (attributeInfolist.Length < 2
                        || !int.TryParse(attributeInfolist[0], out int attriId))
                    {
                        continue;
                    }

                    string[] attributeValue = attributeInfolist[1].Split('~');
                    if (attributeValue.Length == 1
                        && int.TryParse(attributeValue[0], out int fixedValue))
                    {
                        equipAttribute.Add(new AttributeRandom
                        {
                            AttributeID = attriId,
                            AttributeValueMin = fixedValue,
                            AttributeValueMax = fixedValue,
                        });
                    }
                    else if (attributeValue.Length == 2
                             && int.TryParse(attributeValue[0], out int minValue)
                             && int.TryParse(attributeValue[1], out int maxValue))
                    {
                        equipAttribute.Add(new AttributeRandom
                        {
                            AttributeID = attriId,
                            AttributeValueMin = minValue,
                            AttributeValueMax = maxValue,
                        });
                    }
                }

                EquipAttribute.Add(ldEquip.Id, equipAttribute);
            }
        }

        private void ParseEnhance_AttributeList()
        {
            Enhance_AttributeList.Clear();
            foreach (LDEquip ldEquip in this.GetAll().Values)
            {
                if (string.IsNullOrEmpty(ldEquip.Enhance_Attribute))
                {
                    continue;
                }

                // 23~3|24~6（兼容旧表 23_3）
                Dictionary<int, int> map = new Dictionary<int, int>();
                string[] items = ldEquip.Enhance_Attribute.Split('|');
                for (int i = 0; i < items.Length; i++)
                {
                    string s = items[i];
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }

                    char sep = s.IndexOf('~') >= 0 ? '~' : '_';
                    string[] p = s.Split(sep);
                    if (p.Length == 2 && int.TryParse(p[0], out int id) && int.TryParse(p[1], out int v))
                    {
                        map[id] = v;
                    }
                }

                if (map.Count > 0)
                {
                    Enhance_AttributeList[ldEquip.Id] = map;
                }
            }
        }

        /// <summary>
        /// 4~6 位：id + min(3)，max=min；7~9 位：id + min(3) + max(3)
        /// </summary>
        public static bool TryParsePackedAttr(int code, out int attrId, out int minValue, out int maxValue)
        {
            attrId = 0;
            minValue = 0;
            maxValue = 0;
            if (code <= 0)
            {
                return false;
            }

            string s = code.ToString();
            int len = s.Length;
            if (len < 4 || len > 9)
            {
                return false;
            }

            try
            {
                if (len <= 6)
                {
                    attrId = int.Parse(s.Substring(0, len - 3));
                    minValue = int.Parse(s.Substring(len - 3, 3));
                    maxValue = minValue;
                }
                else
                {
                    attrId = int.Parse(s.Substring(0, len - 6));
                    minValue = int.Parse(s.Substring(len - 6, 3));
                    maxValue = int.Parse(s.Substring(len - 3, 3));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 装备随机属性：
        /// 1) 必中 Att_Rand_1~6：每个池各抽 1 条（池空则跳过）
        /// 2) 附加 Att_Rand_Num 次：按组池抽，同属性 id 不重复
        ///    - ParamX1：组属性码（打包 id/min/max）
        ///    - ParamX2：该组最多抽几条，超过则该组不再进池（0=不限）
        ///    - ParamX3：从第几次附加才加入该组；Num=1 且 ParamX3=2 则永远抽不到
        /// </summary>
        public List<AttributeItem> GetRandomAttribute(int equipId)
        {
            List<AttributeItem> result = new List<AttributeItem>();
            LDEquip e = this.Get(equipId);
            // 已抽出的属性 id，必中/附加共用，避免同 id 抽两次
            HashSet<int> used = new HashSet<int>();

            // ---------- 必中：Att_Rand_1~6，每池抽 1 条 ----------
            int[][] mustPools =
            {
                e.Att_Rand_1, e.Att_Rand_2, e.Att_Rand_3,
                e.Att_Rand_4, e.Att_Rand_5, e.Att_Rand_6,
            };
            for (int p = 0; p < mustPools.Length; p++)
            {
                int[] pool = mustPools[p];
                if (pool == null || pool.Length == 0)
                {
                    continue;
                }

                // 本池候选：解析打包码，过滤已用 id
                List<int> ids = new List<int>();
                List<int> mins = new List<int>();
                List<int> maxs = new List<int>();
                for (int i = 0; i < pool.Length; i++)
                {
                    if (!TryParsePackedAttr(pool[i], out int id, out int min, out int max))
                    {
                        continue;
                    }

                    if (used.Contains(id) || ids.Contains(id))
                    {
                        continue;
                    }

                    ids.Add(id);
                    mins.Add(min);
                    maxs.Add(max);
                }

                if (ids.Count == 0)
                {
                    continue;
                }

                // 随机一条，值在 [min, max]
                int idx = RandomHelper.RandomNumber(0, ids.Count);
                int pickMin = mins[idx];
                int pickMax = maxs[idx] < pickMin ? pickMin : maxs[idx];
                result.Add(new AttributeItem
                {
                    AttributeID = ids[idx],
                    AttributeValue = RandomHelper.RandomNumber(pickMin, pickMax + 1),
                });
                used.Add(ids[idx]);
            }

            // ---------- 随机附加：共 Att_Rand_Num 次 ----------
            int addNum = e.Att_Rand_Num;
            if (addNum <= 0)
            {
                return result;
            }

            // 组1~5 的属性码池
            List<int>[] groupCodes =
            {
                new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(),
            };
            // 该组最多抽几条（0=不限）；抽满后本轮起不再进池
            int[] groupMax = { e.Att_Rand_Param12, e.Att_Rand_Param22, e.Att_Rand_Param32, e.Att_Rand_Param42, e.Att_Rand_Param52 };
            // 从第几次附加才加入该组（1=第一次就进池；2=第二次才进池）
            int[] groupJoin = { e.Att_Rand_Param13, e.Att_Rand_Param23, e.Att_Rand_Param33, e.Att_Rand_Param43, e.Att_Rand_Param53 };
            // 该组已抽出条数
            int[] groupPicked = { 0, 0, 0, 0, 0 };

            // 组1：表字段是 int[]；组2~5：字符串用 | 分隔
            if (e.Att_Rand_Param11 != null)
            {
                for (int i = 0; i < e.Att_Rand_Param11.Length; i++)
                {
                    if (e.Att_Rand_Param11[i] > 0)
                    {
                        groupCodes[0].Add(e.Att_Rand_Param11[i]);
                    }
                }
            }

            string[] strPools = { null, e.Att_Rand_Param21, e.Att_Rand_Param31, e.Att_Rand_Param41, e.Att_Rand_Param51 };
            for (int g = 1; g < 5; g++)
            {
                if (string.IsNullOrEmpty(strPools[g]))
                {
                    continue;
                }

                string[] parts = strPools[g].Split('|');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i], out int code) && code > 0)
                    {
                        groupCodes[g].Add(code);
                    }
                }
            }

            // 第 round 次附加：合并 JoinRound<=round 且未达上限的组，抽 1 条
            for (int round = 1; round <= addNum; round++)
            {
                List<int> ids = new List<int>();
                List<int> mins = new List<int>();
                List<int> maxs = new List<int>();
                List<int> fromGroup = new List<int>(); // 候选来自哪一组，抽中后给该组 Picked++

                for (int g = 0; g < 5; g++)
                {
                    // 未配置加入轮次，或还没到该组加入轮次
                    if (groupJoin[g] <= 0 || groupJoin[g] > round)
                    {
                        continue;
                    }

                    // 已达该组最大个数
                    if (groupMax[g] > 0 && groupPicked[g] >= groupMax[g])
                    {
                        continue;
                    }

                    List<int> codes = groupCodes[g];
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (!TryParsePackedAttr(codes[i], out int id, out int min, out int max))
                        {
                            continue;
                        }

                        if (used.Contains(id) || ids.Contains(id))
                        {
                            continue;
                        }

                        ids.Add(id);
                        mins.Add(min);
                        maxs.Add(max);
                        fromGroup.Add(g);
                    }
                }

                if (ids.Count == 0)
                {
                    continue;
                }

                int idx = RandomHelper.RandomNumber(0, ids.Count);
                int pickMin = mins[idx];
                int pickMax = maxs[idx] < pickMin ? pickMin : maxs[idx];
                result.Add(new AttributeItem
                {
                    AttributeID = ids[idx],
                    AttributeValue = RandomHelper.RandomNumber(pickMin, pickMax + 1),
                });
                used.Add(ids[idx]);
                groupPicked[fromGroup[idx]]++;
            }

            return result;
        }

        public List<AttributeItem> GetEquipAttribute(int equipId)
        {
            List<AttributeItem> result = new List<AttributeItem>();
            result.AddRange(GetRandomAttribute(equipId));

            if (!this.EquipAttribute.TryGetValue(equipId, out List<AttributeRandom> fixedList) || fixedList == null)
            {
                return result;
            }

            for (int i = 0; i < fixedList.Count; i++)
            {
                int min = (int)fixedList[i].AttributeValueMin;
                int max = (int)fixedList[i].AttributeValueMax;
                result.Add(new AttributeItem
                {
                    AttributeID = fixedList[i].AttributeID,
                    AttributeValue = RandomHelper.RandomNumber(min, max + 1),
                });
            }

            return result;
        }
    }
}
