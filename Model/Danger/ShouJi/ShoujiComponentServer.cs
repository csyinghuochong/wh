using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
namespace ET
{

    public class ShoujiComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        /// <summary>
        /// 收集大厅
        /// </summary>
        public List<ShouJiChapterInfo> ShouJiChapterInfos = new List<ShouJiChapterInfo>();

        /// <summary>
        /// 珍宝
        /// </summary>
        public List<IntLongPair> TreasureInfo = new List<IntLongPair>();


        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> ChapterStar = new Dictionary<int, int>();
    }
}
