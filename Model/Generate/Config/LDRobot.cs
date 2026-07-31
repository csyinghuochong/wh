using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDRobotCategory : ProtoObject, IMerge
    {
        public static LDRobotCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDRobot> dict = new Dictionary<int, LDRobot>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDRobot> list = new List<LDRobot>();
		
        public LDRobotCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDRobotCategory s = o as LDRobotCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDRobot config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDRobot)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDRobot Get(int id)
        {
            this.dict.TryGetValue(id, out LDRobot item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDRobot)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDRobot> GetAll()
        {
            return this.dict;
        }

        public LDRobot GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDRobot: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
