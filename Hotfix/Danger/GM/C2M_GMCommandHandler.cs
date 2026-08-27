using MsgCryptTest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    //1#3#10001#1  添加道具id(10001)自动使用领悟生活技能          3#1  制造id(1)

    [ActorMessageHandler]
    public class C2M_GMCommandHandler : AMActorLocationHandler<Unit, C2M_GMCommandRequest>
    {
        /// <summary>
        /// 提取第二个#后面的整数
        /// 样例: //#testmail#12 → 12
        /// </summary>
        public static bool TryGetSecondHashNumber(string source, out int result)
        {
            result = 0;
            if (string.IsNullOrEmpty(source))
                return false;

            string[] arr = source.Split('#');
            // 至少要有两个#，数组长度≥2，第二个#后的内容在arr[1]
            if (arr.Length < 2)
                return false;

            return int.TryParse(arr[1].Trim(), out result);
        }


		protected override async ETTask Run(Unit unit, C2M_GMCommandRequest message)
		{
			try
			{
				string[] commands = message.GMMsg.Split('#');
				if (commands.Length == 0)
				{
					return;
				}

				if (message.GMMsg == "allmonster#")
				{
					foreach (LDMonster monsterConfig in LDMonsterCategory.Instance.GetAll().Values)
					{
						await TimerComponent.Instance.WaitAsync(1);
						Vector3 pos = unit.Position;
						Vector3 vector3 = new Vector3(pos.x + RandomHelper.RandFloat01() * 1, pos.y, pos.z + RandomHelper.RandFloat01() * 1);
						Unit monster = UnitFactory.CreateMonster(unit.DomainScene(), monsterConfig.Id, vector3,  new CreateMonsterInfo()
						{ 
							Camp  = CampEnum.CampMonster1
						});
					}
					return;
				}
				if (message.GMMsg .Contains ("testmail#"))
				{
                    //testmail#1
                    //testmail#2

                    //调用
                    if (TryGetSecondHashNumber(message.GMMsg, out int mailid))
                    {
                        Console.WriteLine(mailid);
                    }

                    MailInfo mailInfo = new MailInfo();
                    mailInfo.Status = 0;
					mailInfo.ConfigId = mailid;
                    Log.Error("MailInfo mailInfo = new MailInfo");
                    //mailInfo.Context = i + "_________" + i;
                    //mailInfo.Title = "系统通知";
                    mailInfo.MailId = IdGenerater.Instance.GenerateId();
                    mailInfo.Form = "官方xxx";
                    mailInfo.ValidTime = TimeHelper.ServerNow() + RandomHelper.RandomNumber(2000, 80000);
					mailInfo.ParamList.Add("AAAA");
                    mailInfo.ParamList.Add("BBBB");
                    mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1000100, ItemNum = 1 });
                    mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1000100, ItemNum = 1 });
                    await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(unit), unit.Id, mailInfo);
                }
               
                if (message.GMMsg == "killall#")
                {
					List<Unit> units = unit.GetParent<UnitComponent>().GetAll();
					
					for(int i = units.Count - 1; i >= 0; i--)
					{
						if (units[i].Type != UnitType.Monster)
						{
							continue;
						}

						units[i].GetComponent<NumericComponent>().ApplyChange(unit, NumericType.HP_Current_8, -1000000000, 0);
					}
					return;
				}
               
				if (message.GMMsg == "resetguide#")
				{
					unit.GetComponent<RoleInfoComponentServer>().RoleInfo.CompleteGuideIds.Clear();
					return;
				}
				
                if (message.GMMsg == "completetask#")
				{
					unit.GetComponent<TaskComponentServer>().GMCompletCurrentTask();
					return;
				}
                if (message.GMMsg == "resetweek#")
                {
                    unit.GetComponent<TaskComponentServer>().UpdateWeeklyTask(true);
                    return;
                }
                if (message.GMMsg == "petfight#")
                {
                    //出战要清掉之前的
					PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
                    UnitComponent unitComponent = unit.GetParent<UnitComponent>();
                    PetInfo fightpet = petComponentServer.GetFightPet();
                    if (fightpet != null)
                    {
                        fightpet.PetStatus = 0;
                        unitComponent.Remove(fightpet.Id);
                    }

                    PetInfo petinfo = petComponentServer.PetInfos.FirstOrDefault();
					if (petinfo == null)
					{
						return;
					}
                    Unit existingPetUnit = unitComponent.Get(petinfo.Id);
                    if (existingPetUnit == null)
                    {
                        petComponentServer.UpdatePetAttribute(petinfo, false);
                        UnitFactory.CreatePet(unit, petinfo);
                    }

                    petinfo.PetStatus = 1;
                    petComponentServer.FightPetId = petinfo.Id;
                    return;
                }
                switch (commands[0])
                {
					case "1":             //新增道具1#1#1001#1 1#2#1012#1 【添加道具/道具类型/道具id/道具数量】
						int itemType  =int.Parse(commands[1]);
						int itemId = int.Parse(commands[2]);
						int itemNumber = int.Parse(commands[3]);
						
						switch (itemType)
						{
							case ItemBigType.Type_Exp:
							case ItemBigType.Type_Item:
							case ItemBigType.Type_Equip:
								unit.GetComponent<BagComponentServer>().OnAddItemData($"{itemType}~{itemId}~{itemNumber}", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", true);
								break;
							case ItemBigType.Type_Pet:
								//1#32#11#1
								unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.GM, itemId);
								break;
                            case ItemBigType.Type_Elf:
                                unit.GetComponent<ChengJiuComponentServer>().OnActiveJingLing(itemId);
                                break;
                        }
						break;
                    //70001001  0    71001010    1       70001003     2      70001011    3
                    case "2":       //72009041死亡技能      //2#72000198#1  90000005-爆炸怪 72002013-脱战技能没移除2#-78#0#0.7#72004002#1  70001001  72009001
						int monsterId = int.Parse(commands[1]);
						int number = int.Parse(commands[2]);
						if (number > 100)
						{
							Log.Error("number > 100");
							return;
						}

                        //83000101 83000104
                        for (int c = 0; c < number; c++)
						{
							await TimerComponent.Instance.WaitAsync(1);
							Vector3 vector3 = RandomHelper.GetRandomPointInCircle(unit.Position, 2f);
							Unit monster = UnitFactory.CreateMonster(unit.DomainScene(), monsterId, vector3, new CreateMonsterInfo()
							{ 
								Camp = CampEnum.CampMonster1,
                            });

							//M2C_CreateSpilings createSpilings = new M2C_CreateSpilings();
							//SpilingInfo spilingInfo = UnitHelper.CreateSpilingInfo(monster);
							//createSpilings.Spilings.Add(spilingInfo);
							//MessageHelper.Broadcast(unit, createSpilings);
						}
						break;

					case "4": //直接接取某个任务      4#30080019
                        unit.GetComponent<TaskComponentServer>().OnGMGetTask(int.Parse(commands[1]));
						break;
					case "6":
						int newLevel = int.Parse(commands[1]);
						RoleInfoComponentServer roleInfoComponentServer =  unit.GetComponent<RoleInfoComponentServer>();
						TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();	
                        //if (newLevel <= roleInfoComponent.GetMaxLevel(taskComponent.RoleComoleteTaskList))
						if(newLevel <= LDGlobalValueCategory.Instance.MaxLevel)
						{
							int level = newLevel - roleInfoComponentServer.RoleInfo.Lv;
                            roleInfoComponentServer.UpdateRoleData(UserDataType.Level, level.ToString());
						}
						break;
					case "10":
                        Log.Warning("刷新机器人！！");
                        long robotSceneId = DBHelper.GetRobotServerId();
                        MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest() { Zone = UnitZoneHelper.GetHomeZone(unit), MessageType = 18, Message = $"1001#{commands[1]}" });
                        break;
					case "11": //11#92041030   11#80002003   11#80002005  11#97050403
                        {
                            BuffManagerComponent buffManager = unit.GetComponent<BuffManagerComponent>();
                            BuffData buffData_2 = new BuffData();
                            buffData_2.SkillId = 67000278;
                            buffData_2.BuffId = int.Parse(commands[1]); 
                            buffManager.BuffFactory(buffData_2, unit, null);
                        }
                        break;
					case "12":
                        {
                            BuffManagerComponent buffManager = unit.GetComponent<BuffManagerComponent>();
                            for (int i = 0; i < long.Parse(commands[1]); i++)
                            {
                                BuffData buffData_2 = new BuffData();
                                buffData_2.SkillId = 67000278;
                                buffData_2.BuffId = int.Parse(commands[2]);
                                buffManager.BuffFactory(buffData_2, unit, null);
                            }
                        }
						break;
					case "13":
						List<Unit> players = unit.GetParent<UnitComponent>().GetAll();
						int buffCount = int.Parse(commands[1]);
						int buffId = int.Parse(commands[2]);
						for (int player = 0; player < players.Count; player++)
						{
							BuffManagerComponent buffMgr = players[player].GetComponent<BuffManagerComponent>();
							if (buffMgr == null)
							{
								continue;
							}
                            for (int i = 0; i < buffCount; i++)
                            {
                                BuffData buffData_2 = new BuffData();
                                buffData_2.SkillId = 67000278;
                                buffData_2.BuffId = buffId;
                                buffMgr.BuffFactory(buffData_2, unit, null);
                            }
                        }
						break;
					case "21": // 跨服旅游 21#目标区号  例：21#2；填本服区号则回本服主城
						if (commands.Length < 2)
						{
							Log.Warning("[WarZoneTour] GM 格式：21#目标区号");
							break;
						}
						await WarZoneTourHelper.TourToZone(unit, int.Parse(commands[1]));
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Debug(ex.ToString());
			}

			await ETTask.CompletedTask;
		}
	}
}
