using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public  class C2Consign_ShopShowListHandler : AMActorRpcHandler<Scene, C2Consign_ShopShowListRequest, Consign2C_ShopShowListResponse>
    {
		//拍卖快捷列表购买道具
		protected override async ETTask Run(Scene scene, C2Consign_ShopShowListRequest request, Consign2C_ShopShowListResponse response, Action reply)
		{
			ConsignSceneComponent paimaiCompontent = scene.GetComponent<ConsignSceneComponent>();
			response.PaiMaiShopItemInfos = paimaiCompontent.dBPaiMainInfo_Shop.PaiMaiShopItemInfos;

			reply();
			await ETTask.CompletedTask;
		}

	}
}
