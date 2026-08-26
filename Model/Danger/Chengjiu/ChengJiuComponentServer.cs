using System.Collections.Generic;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif

namespace ET
{

    public enum ChengJiuTypeEnum : int
    { 
        None = 0,
        GuanKa = 1,
        TanSuo = 2,
        ShouJi = 3,
        Number = 4,
    }

    public enum SpiritTypeEnum : int
    {
        None = 0,
        GuanKa = 1,
        TanSuo = 2,
        ShouJi = 3,
        Number = 4,
    }

    //1.每天随机给东西 参数:掉落ID
    //2.拾取地上的金币
    //3.拾取地上的金币和道具
    //4.附带技能 参数:技能ID(取消当前精灵要取消对应的技能Buff)
    //5.激活提升属性 参数: 属性
    //6.每次击败怪物额外附加一个掉落ID 参数: 掉落ID
    //7.打开对应系统功能 参数: 功能ID
    public static class JingLingFunctionType
    {
        public const int RandomDrop  = 1;
        public const int PickGold = 2;

        public const int AddSkill = 4;
        public const int AddProperty = 5;
        public const int ExtraDrop = 6;
        public const int OpenFunction = 7;
    }


    public class ChengJiuComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
#if SERVER
        public long JingLingUnitId = 0;
        public List<ChengJiuInfo> ChengJiuProgessList = new List<ChengJiuInfo>();

        [BsonIgnore]
        public int ChengJiuEventBatchDepth;

        [BsonIgnore]
        public Dictionary<(int, int), int> ChengJiuEventCoalesceAdd = new Dictionary<(int, int), int>();

        [BsonIgnore]
        public Dictionary<(int, int), int> ChengJiuEventCoalesceSet = new Dictionary<(int, int), int>();
#else
        public Dictionary<int, ChengJiuInfo> ChengJiuProgessList = new Dictionary<int, ChengJiuInfo>();
#endif
        public int TotalChengJiuPoint = 0;
        public List<int> AlreadReceivedId = new List<int>();
        public List<int> ChengJiuCompleteList = new List<int>();
        public List<int> JingLingList = new List<int>();
        public List<MagickaSlotInfo> MagickaSlotIdList = new List<MagickaSlotInfo>();
        public int JingLingId = 0;
        public int RandomDrop = 0;
    }
}
