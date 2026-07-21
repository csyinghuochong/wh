using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ObjectSystem]
    public class TitleComponentSystemAwake: AwakeSystem<TitleComponentServer>
    {

        public override void Awake(TitleComponentServer self)
        {
            self.TitleList.Clear();
        }
    }

    public static class TitleComponentServerSystem
    {
        private static readonly Dictionary<int, List<AttributeItem>> TitleProCache = new Dictionary<int, List<AttributeItem>>();

        private static List<AttributeItem> GetCachedTitlePro(int titleId, string addProperty)
        {
            if (TitleProCache.TryGetValue(titleId, out List<AttributeItem> cached))
            {
                return cached;
            }

            List<AttributeItem> proList = new List<AttributeItem>();
            string[] attributeInfoList = addProperty.Split('@');
            for (int a = 0; a < attributeInfoList.Length; a++)
            {
                string[] attributeInfo = attributeInfoList[a].Split(';');
                int numericType = int.Parse(attributeInfo[0]);

                if (NumericHelp.GetNumericValueType(numericType) == 2)
                {
                    float fvalue = float.Parse(attributeInfo[1]);
                    proList.Add(new AttributeItem() { AttributeID = numericType, AttributeValue = NumericHelp.ToStoredValue(numericType, fvalue) });
                }
                else
                {
                    long lvalue = 0;
                    try
                    {
                        lvalue = long.Parse(attributeInfo[1]);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex.ToString() + $"报错称号: {titleId}");
                    }
                    proList.Add(new AttributeItem() { AttributeID = numericType, AttributeValue = lvalue });
                }
            }

            TitleProCache[titleId] = proList;
            return proList;
        }

        public static List<AttributeItem> GetTitlePro(this TitleComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();

            for (int i = self.TitleList.Count - 1; i >= 0; i--)
            {
                KeyValuePairInt titleEntry = self.TitleList[i];
                LDElf ldElf = LDElfCategory.Instance.Get(titleEntry.KeyId);
                proList.AddRange(GetCachedTitlePro(titleEntry.KeyId, ldElf.AddProperty));
            }
            return proList;
        }

        /// <summary>
        /// 移除过期称号
        /// </summary>
        /// <param name="self"></param>
        public static void OnCheckTitle(this TitleComponentServer self, bool notice)
        {
            bool update = false;
            long serverTime = TimeHelper.ServerNow();
            for (int i = self.TitleList.Count - 1; i >= 0; i--)
            {
                if (self.TitleList[i].Value == -1) //永久称号
                {
                    continue;
                }
                if (self.TitleList[i].Value < serverTime)
                {
                    update = true;
                    self.TitleList.RemoveAt(i);
                }
            }
            if (!update)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();
            if (notice)
            {
                self.TitleUpdateResult.TitleList = self.TitleList;
                MessageHelper.SendToClient(unit, self.TitleUpdateResult);
            }
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int title = numericComponent.GetAsInt(NumericType.TitleID);
            if (title > 0 && !self.IsHaveTitle(title))
            {
                numericComponent.ApplyValue(NumericType.TitleID, 0, notice);
            }
        }

        public static bool IsHaveTitle(this TitleComponentServer self, int titleId)
        {
            for (int i = 0; i < self.TitleList.Count; i++)
            {
                if (self.TitleList[i].KeyId == titleId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回-1为永久称号
        /// </summary>
        /// <param name="self"></param>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public static long GetTitlLeftTime(this TitleComponentServer self, int titleId)
        {
            for (int i = 0; i < self.TitleList.Count; i++)
            {
                if (self.TitleList[i].KeyId != titleId)
                {
                    continue;
                }
                if (self.TitleList[i].Value == -1)
                {
                    return -1;
                }
                long leftTime = self.TitleList[i].Value - TimeHelper.ServerNow();
                leftTime = Math.Max(leftTime, 0);
                return leftTime;
            }
            return 0;
        }

        public static void OnGmGaoJi(this TitleComponentServer self)
        {
            Dictionary<int, LDTitle> allTitle = LDTitleCategory.Instance.GetAll();
            foreach (var key in allTitle.Keys) 
            {
                self.OnActiveTile( key );
            }
        }

        public static void OnActiveTile(this TitleComponentServer self, int titleId)
        {
            for (int i = self.TitleList.Count - 1; i >= 0; i--)
            {
                if (self.TitleList[i].KeyId == titleId)
                {
                    self.TitleList.RemoveAt(i);
                }
            }

            LDTitle elf = LDTitleCategory.Instance.Get(titleId);
            long endTime = elf.ValidityTime == -1 ? -1 : TimeHelper.ServerNow() + elf.ValidityTime * 1000;
            self.TitleList.Add(new KeyValuePairInt() { KeyId = titleId, Value = endTime });
        }
    }
}
