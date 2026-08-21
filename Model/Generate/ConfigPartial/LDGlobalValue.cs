using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    
    public struct DayMonsters
    {
        public int MonsterId;
        public float GaiLv;
        public int TotalNumber;
    }

    public struct DayJingLing
    {
        public List<int> MonsterId;
        public List<int> Weights;
        public float GaiLv;
        public int TotalNumber;
    }

    
    public partial class LDGlobalValueCategory
    {
        private Dictionary<string, LDGlobalValue> keyDict = new Dictionary<string, LDGlobalValue>(StringComparer.Ordinal);

        public int JianDingFuQulity = 0;


        public int[] BagInitCapacity = new int[(int)ItemLocType.ItemLocMax + 1];
      
        public int GemStoreInitCapacity = 0;
        public int GemStoreMaxCapacity = 0;

        public int OnLineLimit = 0;

        public int AccountBagMax = 0;

        /// <summary>默认已开 1 页仓库。</summary>
        public int DefaultCangKuNumber = 1;


        public int MaxLevel = 100;
        
        public int TempValue = 0;

        public List<DayMonsters> DayMonsterList = new List<DayMonsters>();

        public List<DayJingLing> DayJingLingList = new List<DayJingLing>();

        public Dictionary<int, int> ZhuaPuItem = new Dictionary<int, int>();
        ////上面的全部废弃掉////

        
        public List<int> Add_Point_Level_UP_Fixed = new List<int>();

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
            this.BagInitCapacity[(int)ItemLocType.ItemLocBag] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120021);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagTreasure] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120022);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagMaterial] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120023);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagConsume] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_120024);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagLife] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200251);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagHome] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200252);
            this.BagInitCapacity[(int)ItemLocType.ItemLocBagHome2] = this.GetInt(GlobalValueKey.Global_Bag_Capacity_1200253);
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
            
            Add_Point_Level_UP_Free_ByLevel = GlobalValueLevelPointParser.ParseToLevelTable(
                rawValue,
                this.MaxLevel,
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