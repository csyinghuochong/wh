using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_PetHeCheng, M2C_PetHeCheng>
	{
		protected override async ETTask Run(Unit unit, C2M_PetHeCheng request, M2C_PetHeCheng response, Action reply)
		{
			//读取数据库
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
			ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
			TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

			PetInfo petinfo_1 = petComponentServer.GetPetInfo(request.PetInfoId1);
            PetInfo petinfo_2 = petComponentServer.GetPetInfo(request.PetInfoId2);
			if (petinfo_1 == null || petinfo_2 == null)
			{
				response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}
            if (petinfo_1.PetStatus == 1 || petinfo_2.PetStatus == 1)
            {
                response.Error = ErrorCode.ERR_Pet_Hint_4;
                reply();
                return;
            }
			//错误码
			//判定是否出战
			//if (PetStatus_1 == 1 || PetStatus_2 == 2)
			//{
			//	response.Error = 1;
			//}
			//改变第一个宠物的数据	

			//宠物合成次数
			int petHeChengNumber = dataCollationComponent.PetHeCheng;

			PetHeChengResult heChengResult = petComponentServer.TryHeChengPets(petinfo_1, petinfo_2, petHeChengNumber);
            PetInfo petinfo_update = heChengResult.UpdatePet;
            PetInfo petinfo_delete = heChengResult.DeletePet;
			petinfo_update.ConfigId = heChengResult.PetID;
			petinfo_update.PetLv = heChengResult.PetLv;
			petinfo_update.PetExp = heChengResult.PetExp;
			petinfo_update.IfBaby = heChengResult.IfBaby;
			petinfo_update.AddPropretyNum = heChengResult.AddPropretyNum;
			petinfo_update.AddPropretyValue = heChengResult.AddPropretyValue;
			petinfo_update.PetPingFen = 0;
			petinfo_update.ZiZhi_Hp = heChengResult.ZiZhi_Hp;
			petinfo_update.ZiZhi_Act = heChengResult.ZiZhi_Act;
			petinfo_update.ZiZhi_MageAct = heChengResult.ZiZhi_MageAct;
			petinfo_update.ZiZhi_Def = heChengResult.ZiZhi_Def;
			petinfo_update.ZiZhi_Adf = heChengResult.ZiZhi_Adf;
			petinfo_update.ZiZhi_ActSpeed = heChengResult.ZiZhi_ActSpeed;
			petinfo_update.ZiZhi_ChengZhang = heChengResult.ZiZhi_ChengZhang;
			petinfo_update.PetSkill = heChengResult.SavePetSkillID;
			LDPet petconf = LDPetCategory.Instance.Get(heChengResult.PetID);
			petinfo_update.PetName = petconf.Name.ToString();
            petinfo_update.LockSkill.Clear();
            petComponentServer.OnResetPoint(petinfo_update);
			petComponentServer.RemovePet(petinfo_delete.Id, 1);
			chengJiuComponentServer.OnPetHeCheng(petinfo_update);
			taskComponentServer.OnPetHeCheng(petinfo_update);
			dataCollationComponent.PetHeCheng++;

            petComponentServer.OnPetScoreChanged();
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            response.DeletePetInfoId = petinfo_delete.Id;
			response.rolePetInfo = petinfo_update;
			reply();
			await ETTask.CompletedTask;
		}

		public int GetMultiple()
		{
			return 10000;
		}

		public float GetRandomZeroTOne()
		{
			return RandomHelper.RandomNumber(0, 10) * 0.1f;
		}

		//随机分配指定点数
		public string PetAddPropertyFenPei(int sumNum)
		{
			//取4个随机值
			float ran_1 = RandomHelper.RandomNumber(0, 5) * 0.1f;
			float ran_2 = RandomHelper.RandomNumber(0, 5) * 0.1f;
			int ran_ss = Mathf.FloorToInt((1 - ran_1 - ran_2) * 10);
			float ran_3 = RandomHelper.RandomNumber(0, ran_ss) * 0.1f;
			float ran_4 = 1 - ran_1 - ran_2 - ran_3;
			int add_1 = (int)(sumNum * ran_1);
			int add_2 = (int)(sumNum * ran_2);
			int add_3 = (int)(sumNum * ran_3);
			int add_4 = (int)(sumNum * ran_4);

			return add_1 + "_" + add_2 + "_" + add_3 + "_" + add_4;
		}

		public float Pet_HeCheng_ZiZhi(float zizhiValue_1, float zizhiValue_2, float maxZiZhi = 99999, string ziZhiType = "0")
		{
			/*
			float zizhi_1 = 0.04f;
			float zizhi_2 = 0.75f;
			float zizhi_3 = 0.25f;
			float zizhi_4 = 1.1f;

			if (ziZhiType == "1")
			{
				Random example = new Random();
				float number = example.Next(1, 10) * 0.1f;
				//5%概率满资质
				if (number <= zizhi_1)
				{
					return Mathf.Max(zizhiValue_1, zizhiValue_2);
				}
			}
			*/
			//获取随机资质
			/*
			Random example2 = new Random();
			float number2 = example2.Next(1, 10) * 0.1f;
			float zhizhiValue = Mathf.Min(zizhiValue_1, zizhiValue_2) * zizhi_2 + ((Mathf.Min(zizhiValue_1, zizhiValue_2) * zizhi_3 + Mathf.Max(zizhiValue_1, zizhiValue_2) - Mathf.Min(zizhiValue_1, zizhiValue_2))) * number2 * zizhi_4;
			*/

			float ZiZhimin = Mathf.Min(zizhiValue_1, zizhiValue_2);
			float ZiZhimax = Mathf.Max(zizhiValue_1, zizhiValue_2);

			ZiZhimin = ZiZhimin * 0.95f;
			ZiZhimax = ZiZhimax * 1.05f;

			float chaValuie = ZiZhimax - ZiZhimin;

			float zhizhiValue = ZiZhimin + chaValuie * RandomHelper.RandFloat01();

			//限制最高资质
			if (zhizhiValue > maxZiZhi)
			{
				zhizhiValue = maxZiZhi;
			}

            return (float)Math.Round(zhizhiValue, 2) ;
		}
	}
}