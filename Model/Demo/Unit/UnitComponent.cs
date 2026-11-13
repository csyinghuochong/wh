using System.Collections.Generic;

namespace ET
{
	public class UnitComponent: Entity, IAwake,IDestroy
	{
		public int SceneType;

		public List<long> AoI = new List<long>();

		public List<Unit> Units = new List<Unit>();

		public Dictionary<long, UnitTransferBuffer> UnitComponents = new Dictionary<long, UnitTransferBuffer>();

		public List<long> AllPlayers = new List<long>();	
    }
}
