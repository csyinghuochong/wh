using System;
using System.Collections.Generic;

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
        public static List<AttributeItem> GetTitlePro(this TitleComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();
            long serverTime = TimeHelper.ServerNow();
            for (int i = 0; i < self.TitleList.Count; i++)
            {
                IntLongPair titleEntry = self.TitleList[i];
                if (titleEntry.Value != -1 && titleEntry.Value < serverTime)
                {
                    continue;
                }

                List<AttributeItem> attrs = LDTitleCategory.Instance.GetTitleAttri(titleEntry.KeyId);
                if (attrs == null || attrs.Count == 0)
                {
                    continue;
                }

                proList.AddRange(attrs);
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

            Function_Fight.UnitUpdateProperty_Base(unit, notice, true);
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
            self.TitleList.Add(new IntLongPair() { KeyId = titleId, Value = endTime });
        }
    }
}
