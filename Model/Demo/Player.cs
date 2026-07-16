namespace ET
{
	public enum PlayerState : int
	{
		None = 0,
		Disconnect,
		Gate,
		Game,
	}

	public sealed class Player : Entity, IAwake<string>, IAwake<long,long>
	{ 
		public long AccountId { get; set; }


		/// <summary>
		/// 玩家离线就会置空，还没有移除的情况下也会置空
		/// </summary>
		public Session ClientSession { get; set; }

		public long UnitId { get; set; }		//unitid  ==  userid

		public string Name { get; set; }

		public PlayerState PlayerState { get; set; }

		public long DBCacheId { get; set; }

		public long ChatInfoInstanceId { get; set; }

		/// <summary>战区聊天 ChatInfoUnit InstanceId；未入战区为 0</summary>
		public long WarChatInfoInstanceId { get; set; }

		public long MailServerID { get; set; }

		public long ReChargeServerID { get; set; }

		public long PopularizeServerID { get; set; }

		public long RankServerID { get; set; }

		/// <summary>战区 Rank ActorId；未入战区为 0</summary>
		public long WarRankServerID { get; set; }

		public long PaiMaiServerID { get; set; }

		public long ActivityServerID { get; set; }
		
		public long TeamServerID { get; set; }

		public long FriendServerID { get; set; }

		public long UnionServerID { get; set; }

		public long SoloServerID { get; set; }

		public string RemoteAddress { get; set; }

	}
}