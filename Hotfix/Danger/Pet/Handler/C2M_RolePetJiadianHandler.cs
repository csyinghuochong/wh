using System;
using System.Collections.Generic;
using System.Text;

namespace ET
{
	//玩家宠物
	[ActorMessageHandler]
	public class C2M_RolePetJiadianHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetJiadian, M2C_RolePetJiadian>
	{
		protected override async ETTask Run(Unit unit, C2M_RolePetJiadian request, M2C_RolePetJiadian response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			//读取数据库
			RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
			if (rolePetInfo == null)
			{
				reply();
				return;
			}

			int allValue = 0;
			int maxPoint = (rolePetInfo.PetLv - 1) * 5;
			StringBuilder addPropretyValueBuilder = new StringBuilder();
			addPropretyValueBuilder.Append(request.AddPropretyValue[0]);
			allValue += request.AddPropretyValue[0];
			for (int i = 1; i < request.AddPropretyValue.Count; i++)
			{
				allValue += request.AddPropretyValue[i];
				addPropretyValueBuilder.Append('_').Append(request.AddPropretyValue[i]);
			}
			rolePetInfo.AddPropretyValue = addPropretyValueBuilder.ToString();
			rolePetInfo.AddPropretyNum = maxPoint - allValue;
			if (allValue > maxPoint 
				|| rolePetInfo.AddPropretyNum < 0 
                || request.AddPropretyValue[0] > maxPoint
				|| request.AddPropretyValue[1] > maxPoint 
				|| request.AddPropretyValue[2] > maxPoint
				|| request.AddPropretyValue[3] > maxPoint)
			{
				rolePetInfo.AddPropretyValue = CommonConfig.DefaultProprety;
                rolePetInfo.AddPropretyNum = (rolePetInfo.PetLv - 1) * 5;
			}
			petComponentServer.UpdatePetAttribute(rolePetInfo, true);
			response.RolePetInfo = rolePetInfo;	

			reply();
			await ETTask.CompletedTask;
		}
	}
}