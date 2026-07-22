using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;


namespace ET
{
    
    /// <summary>
    /// Component公用。 system 分开
    /// </summary>
    
    public class BagComponentServer : Entity, IAwake, ITransfer, IUnitCache

    {
        

        /// <summary>
        /// 附加格子
        /// </summary>
        public List<int> AdditionalCellNum = new List<int>();

        /// <summary>
        /// 激活的时装
        /// </summary>
        public List<int> FashionActiveIds = new List<int>();

        /// <summary>
        /// 穿戴的时装
        /// </summary>
        public List<int> FashionEquipList = new List<int>();


        public List<BagInfo> EquipList = new List<BagInfo>();
        public List<BagInfo> BagItemList =new List<BagInfo>();  //背包
        public List<BagInfo> TreasureList = new List<BagInfo>();//奇珍
        public List<BagInfo> MaterialList = new List<BagInfo>();//材料
        public List<BagInfo> ConsumeList = new List<BagInfo>();//消耗
        public List<BagInfo> LifeList = new List<BagInfo>();
        public List<BagInfo> HomeList = new List<BagInfo>();

        public List<BagInfo> Warehouse1 = new List<BagInfo>();
        

        [BsonIgnore]
        public M2C_RoleBagUpdate message = new M2C_RoleBagUpdate() {  };


        public List<BagInfo>[] AllItemList;


        public bool RealAddItem;
    }
}