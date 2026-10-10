using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    
  
    public partial class LDGlobalValueCategory
    {
        private Dictionary<string, LDGlobalValue> keyDict = new Dictionary<string, LDGlobalValue>(StringComparer.Ordinal);


        public Dictionary<int, int> BagInitCapacity = new Dictionary<int, int>();
      
        public int GemStoreInitCapacity = 0;
        public int GemStoreMaxCapacity = 0;


        /// <summary>默认已开 1 页仓库。</summary>
        public int DefaultCangKuNumber = 1;


        public int MaxLevel = 100;
        

        ////上面的全部废弃掉////

        
        public List<int> Add_Point_Level_UP_Fixed = new List<int>();

        /// <summary>新人等级：该等级及以下不受世界等级影响。</summary>
        public int NewRoleLv;

        /// <summary>每个邮件页签的最大数量。</summary>
        public int MailMaxNum = 100;

        /// <summary>拥挤统计窗口，秒。表未配或小于等于 0 时按 24 小时。</summary>
        public int ServerCrowdTimeSeconds = 24 * 3600;

        /// <summary>拥挤统计窗口毫秒。由秒换算。</summary>
        public long ServerCrowdWindowMs = 24L * TimeHelper.Hour;

        /// <summary>拥挤人数线。0|100|200 取 100。未配为 0，按流畅。</summary>
        public int ServerCrowdBusy;

        /// <summary>爆满人数线。0|100|200 取 200。</summary>
        public int ServerCrowdFull;

        /// <summary>升级自由点：下标=角色等级，值=升到该级本次获得的自由点（1 级为 0）。</summary>
        public int[] Add_Point_Level_UP_Free_ByLevel = Array.Empty<int>();

        public override void AfterEndInit()
        {
            this.keyDict.Clear();
            foreach (LDGlobalValue ldGlobal in this.GetAll().Values)
            {
                if (string.IsNullOrEmpty(ldGlobal.Key))
                {
                    continue;
                }

                if (this.keyDict.ContainsKey(ldGlobal.Key))
                {
                    Log.Error($"LDGlobalValue Key 重复: {ldGlobal.Key}, id={ldGlobal.Id}");
                    continue;
                }

                this.keyDict.Add(ldGlobal.Key, ldGlobal);
            }

            ParseAddPoint();
            ParseBaseData();
        }

        private void ParseBaseData()
        {
            this.NewRoleLv = this.ContainKey(GlobalValueKey.Global_New_Role_Lv)
                ? this.GetInt(GlobalValueKey.Global_New_Role_Lv)
                : 0;

            this.MailMaxNum = 100;
            if (this.ContainKey(GlobalValueKey.Global_Mail_Max_Num))
            {
                int maxNum = this.GetInt(GlobalValueKey.Global_Mail_Max_Num);
                if (maxNum > 0)
                {
                    this.MailMaxNum = maxNum;
                }
            }

            this.BagInitCapacity.Clear();
            this.BagInitCapacity[(int)ItemLocType.ItemLocBag] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120021);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagTreasure] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120022);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagMaterial] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120023);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagConsume] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120024);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagLife] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200251);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagHome] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200252);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagHome2] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200253);
            this.ParseServerCrowd();
        }

        private void ParseServerCrowd()
        {
            int seconds = 24 * 3600;
            if (this.ContainKey(GlobalValueKey.Global_Server_Crowd_Time))
            {
                int value = this.GetInt(GlobalValueKey.Global_Server_Crowd_Time);
                if (value > 0)
                {
                    seconds = value;
                }
            }

            this.ServerCrowdTimeSeconds = seconds;
            this.ServerCrowdWindowMs = seconds * 1000L;
            this.ServerCrowdBusy = 0;
            this.ServerCrowdFull = 0;
            if (!this.ContainKey(GlobalValueKey.Global_Server_Crowd_Role))
            {
                return;
            }

            // 0|100|200：[0,100) 流畅，[100,200) 拥挤，[200,+∞) 爆满
            int[] values = this.GetIntArray(GlobalValueKey.Global_Server_Crowd_Role);
            Array.Sort(values);
            if (values.Length >= 3)
            {
                this.ServerCrowdBusy = values[values.Length - 2];
                this.ServerCrowdFull = values[values.Length - 1];
                return;
            }

            if (values.Length == 2)
            {
                this.ServerCrowdBusy = values[0];
                this.ServerCrowdFull = values[1];
                return;
            }

            if (values.Length == 1)
            {
                this.ServerCrowdFull = values[0];
            }
        }

        private void ParseAddPoint()
        {
            Add_Point_Level_UP_Fixed.Clear();

            if (this.ContainKey(GlobalValueKey.Global_Add_Point_Level_UP_Fixed))
            {
                Add_Point_Level_UP_Fixed.AddRange(this.GetIntArray(GlobalValueKey.Global_Add_Point_Level_UP_Fixed));
            }

            if (!this.ContainKey(GlobalValueKey.Global_Add_Point_Level_UP_Free))
            {
                Add_Point_Level_UP_Free_ByLevel = Array.Empty<int>();
                return;
            }

            string rawValue = this.GetByKey(GlobalValueKey.Global_Add_Point_Level_UP_Free).Value;
            int maxLevel = GlobalValueLevelPointParser.GetMaxLevelInRaw(rawValue);
            Add_Point_Level_UP_Free_ByLevel = GlobalValueLevelPointParser.ParseToLevelTable(
                rawValue,
                maxLevel,
                GlobalValueKey.Global_Add_Point_Level_UP_Free);
        }

        /// <summary>升到指定等级时本次获得的自由属性点（1 级为 0，3 级为 4）。</summary>
        public int GetFreePointByLevel(int level)
        {
            return GlobalValueLevelPointParser.GetPointsByLevel(Add_Point_Level_UP_Free_ByLevel, level);
        }

        /// <summary>指定等级累计已获得的自由属性点（1 级为 0，3 级为 8）。</summary>
        public int GetTotalFreePointByLevel(int level)
        {
            return GlobalValueLevelPointParser.GetTotalPointsByLevel(Add_Point_Level_UP_Free_ByLevel, level);
        }

        public int GetBagInitCapacity(int loc)
        {
            return this.BagInitCapacity.TryGetValue(loc, out int cap) ? cap : 0;
        }

        public LDGlobalValue GetByKey(string key)
        {
            if (!this.keyDict.TryGetValue(key, out LDGlobalValue item))
            {
                throw new Exception($"配置找不到，配置表名: {nameof(LDGlobalValue)}，配置Key: {key}");
            }

            return item;
        }

        public bool ContainKey(string key)
        {
            return this.keyDict.ContainsKey(key);
        }

        public int GetInt(string key)
        {
            string value = NormalizeGlobalValue(this.GetByKey(key).Value);
            if (!int.TryParse(value, out int result))
            {
                throw new Exception($"LDGlobalValue GetInt 解析失败，Key: {key}, Value: {this.GetByKey(key).Value}");
            }

            return result;
        }

        public int[] GetIntArray(string key, char separator = '|')
        {
            string value = this.GetByKey(key).Value;
            string[] parts = value.Split(separator);
            int[] result = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(NormalizeGlobalValue(parts[i]), out result[i]))
                {
                    throw new Exception($"LDGlobalValue GetIntArray 解析失败，Key: {key}, Value: {value}");
                }
            }

            return result;
        }

        /// <summary>导表单值常带花括号，如 {100}。</summary>
        private static string NormalizeGlobalValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = value.Trim();
            if (value.Length >= 2 && value[0] == '{' && value[value.Length - 1] == '}')
            {
                return value.Substring(1, value.Length - 2).Trim();
            }

            return value;
        }
    }
}