using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_FirstWinSelfRewardHandler : AMActorLocationRpcHandler<Unit, C2M_FirstWinSelfRewardRequest, M2C_FirstWinSelfRewardResponse>
    {
		protected override async ETTask Run(Unit unit, C2M_FirstWinSelfRewardRequest request, M2C_FirstWinSelfRewardResponse response, Action reply)
		{            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

			BagComponentServer bag = unit.GetComponent<BagComponentServer>();
			RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
			if (!FirstWinConfigCategory.Instance.Contain(request.FirstWinId))
			{
				response.Error = ErrorCode.ERR_NetWorkError;
				reply();
				return;
			}

			
			string rewardlist = string.Empty;
			FirstWinConfig firstWinConfig = FirstWinConfigCategory.Instance.Get(request.FirstWinId);
			switch (request.Difficulty)
			{
				case 1:
					rewardlist = firstWinConfig.Self_RewardList_1;
					break;
				case 2:
					rewardlist = firstWinConfig.Self_RewardList_2;
					break;
				case 3:
					rewardlist = firstWinConfig.Self_RewardList_3;
					break;
				default:
					rewardlist = firstWinConfig.Self_RewardList_1;
					break;
			}
			string[] rewarditemlist = rewardlist.Split('@');
			if (bag.GetBagLeftCell() < rewarditemlist.Length)
			{
				response.Error = ErrorCode.ERR_BagIsFull;
				reply();
				return;
			}

			int errorcode = roleInfo.OnGetFirstWinSelf(request.FirstWinId, request.Difficulty);
			if (errorcode != ErrorCode.ERR_Success)
			{
				response.Error = errorcode;
				reply();
				return;
			}

			bag.OnAddItemData(rewardlist, $"{ItemGetWay.FirstWin}_{TimeHelper.ServerNow()}");
			response.FirstWinInfos = roleInfo.RoleInfo.FirstWinSelf;
			reply();
			await ETTask.CompletedTask;
		#endif
}
	}
}
