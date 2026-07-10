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
        /// （购买格子数量）
        /// </summary>
        public List<int> WarehouseAddedCell = new List<int>();

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


        public List<BagInfo> BagItemList =new List<BagInfo>();
        public List<BagInfo> BagItemPetHeXin = new List<BagInfo>();
        public List<BagInfo> EquipList = new List<BagInfo>();
        public List<BagInfo> GemList = new List<BagInfo>();
        public List<BagInfo> PetHeXinList = new List<BagInfo>();
        public List<BagInfo> Warehouse1 = new List<BagInfo>();
        public List<BagInfo> Warehouse2 = new List<BagInfo>();
        public List<BagInfo> Warehouse3 = new List<BagInfo>();
        public List<BagInfo> Warehouse4 = new List<BagInfo>();
        public List<BagInfo> JianYuanWareHouse1 = new List<BagInfo>();
        public List<BagInfo> JianYuanWareHouse2 = new List<BagInfo>();
        public List<BagInfo> JianYuanWareHouse3 = new List<BagInfo>();
        public List<BagInfo> JianYuanWareHouse4 = new List<BagInfo>();
        public List<BagInfo> JianYuanTreasureMapStorage1 = new List<BagInfo>();
        public List<BagInfo> JianYuanTreasureMapStorage2 = new List<BagInfo>();
        public List<BagInfo> ChouKaWarehouse = new List<BagInfo>();


        [BsonIgnore]
        public M2C_RoleBagUpdate message = new M2C_RoleBagUpdate() {  };


        public List<BagInfo>[] AllItemList;


        public bool RealAddItem;
    }
}