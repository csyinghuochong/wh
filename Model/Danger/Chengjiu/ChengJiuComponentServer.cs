using System.Collections.Generic;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif

namespace ET
{


    public class ChengJiuComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {

        public long JingLingUnitId = 0;
    

        [BsonIgnore]
        public Dictionary<(int, int), int> ChengJiuEventCoalesceAdd = new Dictionary<(int, int), int>();

        [BsonIgnore]
        public Dictionary<(int, int), int> ChengJiuEventCoalesceSet = new Dictionary<(int, int), int>();

        public int TotalChengJiuPoint = 0;
        public List<int> AlreadReceivedId = new List<int>();
        public List<int> ChengJiuCompleteList = new List<int>();
        public List<int> JingLingList = new List<int>();
        public int JingLingId = 0;
        public int RandomDrop = 0;
    }
}
