using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;


namespace ET
{
    
    /// <summary>
    /// Component公用。 system 分开
    /// </summary>
    
    public class BagComponentServer : Entity, IAwake, ITransfer, IDeserialize, IUnitCache
    {

        /// <summary>
        /// 激活的时装
        /// </summary>
        public List<int> FashionActiveIds = new List<int>();

        /// <summary>
        /// 穿戴的时装
        /// </summary>
        public List<int> FashionEquipList = new List<int>();

        /// <summary>已开启的角色仓库页数。</summary>
        public int CangKuNumber;

        /// <summary>
        /// 已购买/增加的格子数量，key = ItemLocType
        /// </summary>
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> AddedCellNum = new Dictionary<int, int>();


        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, List<BagInfo>> AllItemList = new Dictionary<int, List<BagInfo>>();

        [BsonIgnore]
        public M2C_RoleBagUpdate message = new M2C_RoleBagUpdate() {  };

    }
}
