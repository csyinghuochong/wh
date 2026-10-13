

namespace ET
{

	public static class SessionPlayerComponentSystem
	{
		public class SessionPlayerComponentAwakeSystem : AwakeSystem<SessionPlayerComponent>
		{
			public override void Awake(SessionPlayerComponent self)
			{
				self.isLoginAgain = false;
				self.AccountId = 0;
				self.PlayerId = 0;
				self.PlayerInstanceId = 0;
			}
		}

		public class SessionPlayerComponentDestroySystem: DestroySystem<SessionPlayerComponent>
		{
			public override void Destroy(SessionPlayerComponent self)
			{
				Player player = Game.EventSystem.Get(self.PlayerInstanceId) as Player;
				if (player == null)
				{
					if (Log.IsDebugEnabled)
					{
						Log.Debug($"SessionPlayerComponent  player == null {self.AccountId}");
					}
					return;
				}

				Session session = self.GetParent<Session>();
				// 当前会话已经换成新连接。这次销毁的是被顶掉的旧连接，不要清掉新会话。
				if (player.ClientSession != null && player.ClientSession != session)
				{
					return;
				}

				if (Log.IsDebugEnabled)
				{
					Log.Debug($"SessionPlayerDestroy zone: {self.DomainZone()} self.isLoginAgain = {self.isLoginAgain}: player.Id:{player.Id} player.InstanceId:{player.InstanceId}");
				}
				ActorLocationSenderComponent.Instance.Send(self.PlayerId, new G2M_SessionDisconnect());
				player.RemoveComponent<PlayerOfflineOutTimeComponent>();
				player.AddComponent<PlayerOfflineOutTimeComponent>();	//n秒后会kickplayer
				player.ClientSession = null;

				self.AccountId = 0;
				self.PlayerId = 0;
				self.PlayerInstanceId = 0;
				self.isLoginAgain = false;
			}
		}

		public static Player GetMyPlayer(this SessionPlayerComponent self)
		{
			return self.Domain.GetComponent<PlayerComponent>().Get(self.AccountId);
		}

		public static Player GetPlayer(this SessionPlayerComponent self, long accountId)
		{
			return self.Domain.GetComponent<PlayerComponent>().Get(accountId);
		}
	}
}
