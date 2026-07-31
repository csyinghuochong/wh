using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public partial class LDEquipCategory
    {

        public Dictionary<int, Dictionary<int,int>> Enhance_AttributeList = new Dictionary<int, Dictionary<int, int>>();

        private Dictionary<int, List<AttributeRandom>> EquipAttribute = new Dictionary<int,List<AttributeRandom>> { };

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
                List<AttributeRandom> equipAttribute = new List<AttributeRandom>();


                if (ldEquip.Id == 1115780)
                {
                    Console.WriteLine("ldEquip.Id == 1115780");
                }

                //23_500~600|24_1000~1200|66_50~60|70_50~60|18_30"

                if (string.IsNullOrEmpty(ldEquip.Attribute))
                {
                    continue;
                }

                string[] attributeList = ldEquip.Attribute.Split("|");
                for (int i = 0; i < attributeList.Length; i++)
                {
                    string attributeInfo = attributeList[i];

                    if (attributeInfo.Length < 2)
                    {
                        continue;
                    }

                    string[] attributeInfolist = attributeInfo.Split("~");
                    int attriId = int.Parse(attributeInfolist[0]);

                    string[] attributeValue = attributeInfolist[1].Split("~");

                    if (attributeValue.Length == 1)
                    {
                        equipAttribute.Add(new AttributeRandom()
                        {
                            AttributeID = attriId,
                            AttributeValueMin = int.Parse(attributeValue[0]),
                            AttributeValueMax = int.Parse(attributeValue[0]),
                        });
                    }
                    if (attributeValue.Length == 2)
                    {
                        equipAttribute.Add(new AttributeRandom()
                        {
                            AttributeID = attriId,
                            AttributeValueMin = int.Parse(attributeValue[0]),
                            AttributeValueMax = int.Parse(attributeValue[1]),
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

                //23_5|24_10|66_2|67_2|1_1|2_1|3_1|4_1|6_1|132_1|133_1|135_1|136_1

                string[] attributeList = ldEquip.Enhance_Attribute.Split("|");

                Dictionary<int, int> attributeItems = new Dictionary<int, int>();

                for (int i = 0; i < attributeList.Length; i++)
                {
                    string[] attributeItem = attributeList[i].Split("_");


                    if (attributeItem.Length != 2)
                    {
                        continue;
                    }

                    attributeItems.Add(int.Parse(attributeItem[0]), int.Parse(attributeItem[1]));
                }

                Enhance_AttributeList.Add(ldEquip.Id, attributeItems);

            }
        }


        public List<AttributeItem> GetRandomAttribute(int equipId)
        {
            List<AttributeItem> hideProLists1 = new List<AttributeItem>();

            //1025030|2025030|3025030    
            //以1025030 举例。   最后三位 030  是最大值  中间三位025最小值   前1-3位是属性id  对应NumericType的数值
            //1025030|2025030|3025030    
            //以1025030 举例。   最后三位 030  是最大值  中间三位025最小值   前1-3位是属性id  对应NumericType的数值
            //Att_Rand_Add 切分后的整数 可能是7位 可能是9位     后六位是最小最大值 前1到三位是id
            //第N次抽取：从 Att_Rand_Add1~N 合并池中随机，并过滤已抽出的属性id
            //Att_Rand_1~6 为必中：第N次只从对应池抽，并过滤已抽出的属性id
            //要注意的： Att_Rand_Add2 Att_Rand_Add3 Att_Rand_Add4 等可能为空

            LDEquip lDEquip = this.Get(equipId);
            List<int> usedAttrIds = new List<int>();

            // ---------- 必中属性 Att_Rand_1~6 ----------
            int[][] mustPools =
            {
                lDEquip.Att_Rand_1,
                lDEquip.Att_Rand_2,
                lDEquip.Att_Rand_3,
                lDEquip.Att_Rand_4,
                lDEquip.Att_Rand_5,
                lDEquip.Att_Rand_6,
            };
            for (int i = 0; i < mustPools.Length; i++)
            {
                int[] pool = mustPools[i];
                if (pool == null || pool.Length == 0)
                    continue;

                List<int> candidates = new List<int>();
                for (int j = 0; j < pool.Length; j++)
                {
                    string s = pool[j].ToString();
                    if (s.Length < 7)
                        continue;
                    int attrId = int.Parse(s.Substring(0, s.Length - 6));
                    if (usedAttrIds.Contains(attrId))
                        continue;
                    candidates.Add(pool[j]);
                }
                if (candidates.Count == 0)
                    continue;

                int fullId = candidates[RandomHelper.RandomNumber(0, candidates.Count)];
                string fullStr = fullId.ToString();
                string last6 = fullStr.Substring(fullStr.Length - 6, 6);
                int pickAttrId = int.Parse(fullStr.Substring(0, fullStr.Length - 6));
                int minValue = int.Parse(last6.Substring(0, 3));
                int maxValue = int.Parse(last6.Substring(3, 3));

                hideProLists1.Add(new AttributeItem()
                {
                    AttributeID = pickAttrId,
                    AttributeValue = RandomHelper.RandomNumber(minValue, maxValue + 1),
                });
                usedAttrIds.Add(pickAttrId);
            }

            // ---------- 随机附加 Att_Rand_Add ----------
            int attRandomNum = lDEquip.Att_Rand_Add_Num;
            int[][] addPools =
            {
                lDEquip.Att_Rand_Add1,
                lDEquip.Att_Rand_Add2,
                lDEquip.Att_Rand_Add3,
                lDEquip.Att_Rand_Add4,
                lDEquip.Att_Rand_Add5,
                lDEquip.Att_Rand_Add6,
            };
            for (int i = 0; i < attRandomNum; i++)
            {
                List<int> candidates = new List<int>();
                List<int> candidateAttrIds = new List<int>();
                // 第 i+1 次：合并 Att_Rand_Add1 ~ Att_Rand_Add(i+1)
                for (int p = 0; p <= i && p < addPools.Length; p++)
                {
                    int[] pool = addPools[p];
                    if (pool == null || pool.Length == 0)
                        continue;
                    for (int j = 0; j < pool.Length; j++)
                    {
                        string s = pool[j].ToString();
                        if (s.Length < 7)
                            continue;
                        int attrId = int.Parse(s.Substring(0, s.Length - 6));
                        if (usedAttrIds.Contains(attrId))
                            continue;
                        if (candidateAttrIds.Contains(attrId))
                            continue;
                        candidates.Add(pool[j]);
                        candidateAttrIds.Add(attrId);
                    }
                }
                if (candidates.Count == 0)
                    continue;

                int fullId = candidates[RandomHelper.RandomNumber(0, candidates.Count)];
                string fullStr = fullId.ToString();
                string last6 = fullStr.Substring(fullStr.Length - 6, 6);
                int pickAttrId = int.Parse(fullStr.Substring(0, fullStr.Length - 6));
                int minValue = int.Parse(last6.Substring(0, 3));
                int maxValue = int.Parse(last6.Substring(3, 3));

                hideProLists1.Add(new AttributeItem()
                {
                    AttributeID = pickAttrId,
                    AttributeValue = RandomHelper.RandomNumber(minValue, maxValue + 1),
                });
                usedAttrIds.Add(pickAttrId);
            }

            return hideProLists1;
        }


        public List<AttributeItem> GetEquipAttribute(int equipId)
        {
            List<AttributeItem> hideProLists1 = new List<AttributeItem>();

            hideProLists1.AddRange(GetRandomAttribute(equipId));


            this.EquipAttribute.TryGetValue(equipId, out List<AttributeRandom> hideRandom);

            if (hideRandom == null)
            {
                return hideProLists1;
             }

            for (int i = 0; i < hideRandom.Count; i++)
            {
                int getValue = RandomHelper.RandomNumber((int)hideRandom[i].AttributeValueMin, (int)hideRandom[i].AttributeValueMax + 1 );
                hideProLists1.Add ( new AttributeItem()
                {
                    AttributeID = hideRandom[i].AttributeID,
                    AttributeValue = getValue,   
                }); 
            }


           
            return hideProLists1;
        }
    }
}
