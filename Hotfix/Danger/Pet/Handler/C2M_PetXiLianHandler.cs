using System;
using System.Collections.Generic;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetXiLianHandler : AMActorLocationRpcHandler<Unit, C2M_PetXiLian, M2C_PetXiLian>
	{
		protected override async ETTask Run(Unit unit, C2M_PetXiLian request, M2C_PetXiLian response, Action reply)
		{
			//读取数据库
			PetComponentServer pet = unit.GetComponent<PetComponentServer>();
			BagComponentServer bag = unit.GetComponent<BagComponentServer>();
			ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
			TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
			PetInfo petInfo = pet.GetPetInfo(request.PetInfoId);
			BagInfo bagInfo = bag.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoID);

			bool ifCost = false;

            //扣除相关道具
            if (ifCost)
			{
				//扣除道具
				bag.OnCostItemData($"{bagInfo.ItemID};1", ItemLocType.ItemLocBag, ItemGetWay.PetHeXinExplore);		
				chengJiuComponentServer.OnPetXiLian(petInfo);		//激活成就
				taskComponentServer.OnPetXiLian(petInfo);                    //激活任务

            }
            pet.OnPetScoreChanged();

            reply();
			await ETTask.CompletedTask;
		}

		//宠物打技能书
		private bool Pet_AddSkill( Unit unit, PetInfo petinfo, int addSkillID)
		{
			//判断当前技能是否有重复的
			if (petinfo.PetSkill.Contains(addSkillID))
			{
				return false; 
			}

			//学习规则是随机顶掉当前宠物的一个技能
			if (petinfo.PetSkill.Count > 1)
			{
				bool delStatus = false;
				//2技能打3技能
				if (petinfo.PetSkill.Count < 3)
				{
					if (RandomHelper.RandFloat01() < 0.4f) {
						//不删技能
						delStatus = true;
					}
				}

				//3技能打4技能
				if (petinfo.PetSkill.Count == 3)
				{
					if (RandomHelper.RandFloat01() < 0.2f)
					{
						//不删技能
						delStatus = true;
					}
				}

				//随机获取替换的技能ID序号
				if (!delStatus)
				{
                    //int tihuanNum = RandomHelper.RandomNumber(0, petinfo.PetSkill.Count);
                    //petinfo.PetSkill.RemoveAt(tihuanNum);
                    List<int> canRemoveSkil = new List<int>();
                    HashSet<int> lockSkillSet = new HashSet<int>(petinfo.LockSkill);
                    for (int i = 0; i < petinfo.PetSkill.Count; i++)
					{
						if (!lockSkillSet.Contains(petinfo.PetSkill[i]))
						{
							canRemoveSkil.Add(petinfo.PetSkill[i]);
						}
					}
					//从没有锁定的技能随机删除一个
					if (canRemoveSkil.Count > 0)
					{
						int tihuanNum = RandomHelper.RandomNumber(0, canRemoveSkil.Count);
						int removeSkill = canRemoveSkil[tihuanNum];

                        petinfo.PetSkill.Remove(removeSkill);

                        for (int i = 0; i < petinfo.LockSkill.Count; i++)
                        {
                            //Console.WriteLine($"锁定技能: {unit.Id}  {petinfo.LockSkill[i]}  ");
                        }

                        //Console.WriteLine($"移除技能: {unit.Id}  {removeSkill} ");
                        //Console.WriteLine($"添加技能: {unit.Id}  {addSkillID} ");
                    }
					else
					{
						Log.Error($"技能全锁定： {unit.Id} {petinfo.Id}");
					}

				}
			}

			petinfo.PetSkill.Add(addSkillID);

   //         for (int i = 0; i < petinfo.PetSkill.Count; i++)
   //         {
   //             Console.WriteLine($"最终技能: {unit.Id}   {petinfo.PetSkill[i]}  {TimeHelper.ServerNow()}");
   //         }

			//int lockskill = petinfo.LockSkill.Count > 0 ? petinfo.LockSkill[0] : 0;
			//if (lockskill > 0 && !petinfo.PetSkill.Contains(lockskill))
			//{
   //             Console.WriteLine($"技能锁定Error  {unit.Id}  {lockskill}");
			//}
			//Console.WriteLine("");

            return true;
		}

		//宠物自身洗炼
		private int Pet_XilianSelf(PetInfo petinfo)
		{
			
			int ErrorCore = -1;
			/*
			int petID = petinfo.ConfigId;
			PetConfig petConfig = PetConfigCategory.Instance.Get(petID);

			int petType = petConfig.PetType;
			if (petType != 0)
			{
				//Game_PublicClassVar.Get_function_UI.GameGirdHint_Front("洗炼变异宠物请使用更强大的道具！");
				ErrorCore = 1;
			}

			
			if (petID != 30000001 && petID != 30000002)
			{
				//3%概率变成变异宠物
				if (RandomHelper.RandomNumber(0, 10) * 0.1f <= 0.03f)
				{
					int petBianYiID = 0; // petConfig.PetBianYiID;
					if (petBianYiID != 0)
					{
						petID = petBianYiID;
					}
					//获取玩家名称
					//string roseName = "";
					//广播【"恭喜玩家" + roseName + "洗炼宠物时一不小心打翻了药坛子,宠物不小心变了一个颜色!"】
				}
			}
			*/
			//Function_AI.GetInstance().Pet_Create(petinfo, 1);

			return ErrorCore;
		}

		//宝宝属性点重置
		private bool Pet_AddProprety(PetInfo petinfo)
		{
			//判定目标是否为宝宝
			bool ifBaby = petinfo.IfBaby;
			if (ifBaby == false)
			{
				//langStrHint = Game_PublicClassVar.Get_gameSettingLanguge.LoadLocalizationHint("hint_229");
				//Game_PublicClassVar.Get_function_UI.GameGirdHint_Front(langStrHint);
				return false;
			}

			//读取宠物技能点数
			string addPropretyValue = petinfo.AddPropretyValue;
			int addPropretyNum = petinfo.AddPropretyNum;
			int petLv = petinfo.PetLv;
			string[] addPropretyValueList = addPropretyValue.Split(';');
			int nowNum = 0;
			for (int i = 0; i < addPropretyValueList.Length; i++)
			{
				nowNum = nowNum + int.Parse(addPropretyValueList[i]);
			}

			int nowChongZhiNumOne = 15 + (petLv - 1) * 1;
			if (nowNum >= nowChongZhiNumOne * 4)
			{
				nowNum = nowNum - nowChongZhiNumOne * 4;
			}
			else
			{
				//宠物属性使用失败,当前加点总数必须大于一定值。
				//langStrHint = Game_PublicClassVar.Get_gameSettingLanguge.LoadLocalizationHint("hint_230");
				//Game_PublicClassVar.Get_function_UI.GameGirdHint_Front(langStrHint);
				//Game_PublicClassVar.Get_function_UI.GameGirdHint_Front("重置失败,点数不符！");
				return false;
			}

			//消耗当前的道具,刷新对应的栏位显示
			//销毁道具
			//if (Game_PublicClassVar.Get_function_Rose.CostBagItem(XiLianNeedItemID, int.Parse(XiLianNeedItemNum)))
			bool ifCostStatus = true; // await Function_Role.GetInstance().Bag_CostItem(player.UserId, (int)(request.OperateBagID), (int)(request.OperateBagNum));
			if (ifCostStatus)
			{
				nowNum = nowNum + addPropretyNum;
				petinfo.AddPropretyValue = nowChongZhiNumOne + "_" + nowChongZhiNumOne + "_" + nowChongZhiNumOne + "_" + nowChongZhiNumOne;
				petinfo.AddPropretyNum = nowNum;
				//Game_PublicClassVar.Get_function_DataSet.DataSet_WriteData("AddPropretyValue", nowChongZhiNumOne + ";" + nowChongZhiNumOne + ";" + nowChongZhiNumOne + ";" + nowChongZhiNumOne, "ID", petSpaceID, "RosePet");
				//Game_PublicClassVar.Get_function_DataSet.DataSet_WriteData("AddPropretyNum", nowNum.ToString(), "ID", petSpaceID, "RosePet");
				//Game_PublicClassVar.Get_function_DataSet.DataSet_SetXml("RosePet");
				//langStrHint = Game_PublicClassVar.Get_gameSettingLanguge.LoadLocalizationHint("hint_231");
				//Game_PublicClassVar.Get_function_UI.GameGirdHint_Front(langStrHint);
				//nowXiLianNum = nowXiLianNum + 1;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}