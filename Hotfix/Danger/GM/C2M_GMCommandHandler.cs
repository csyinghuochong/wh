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
        /// 样例: //testmail#12 → 12
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

				Console.WriteLine($"C2M_GMCommandRequest: {message.GMMsg}");

				if (commands.Length == 0)
				{
					return;
				}

                if (message.GMMsg == "chatsystem#")
                {
                    M2C_SyncChatInfo m2C_SyncChatInfo = new M2C_SyncChatInfo()
                    {
                        ChatInfo = new ChatInfo()
                        {
                            UserId = unit.Id,
                            ChannelId = ChannelEnum.System,
                            ChatMsg = $"测试系统消息:{TimeHelper.ServerNow()}",
                            Time = TimeHelper.ServerNow(),
                        }
                    };
                    MessageHelper.SendToClient(unit, m2C_SyncChatInfo);
                }

				if (message.GMMsg == "allmonster#")
				{
					foreach (LDMonster monsterConfig in LDMonsterCategory.Instance.GetAll().Values)
					{
						await TimerComponent.Instance.WaitAsync(1);
						Vector3 pos = unit.Position;
						Vector3 vector3 = new Vector3(pos.x + RandomHelper.RandFloat01() * 1, pos.y, pos.z + RandomHelper.RandFloat01() * 1);
						Unit monster = UnitFactory.CreateMonster(unit.DomainScene(), monsterConfig.Id, vector3, new CreateMonsterInfo()
						{
							Camp = CampEnum.CampMonster1
						});
					}
					return;
				}
                if (message.GMMsg.Contains("skillmakeId#"))
                {
                    //roleHeadId#头像Id
                    if (!TryGetSecondHashNumber(message.GMMsg, out int skillmakeId))
                    {
                        return;
                    }

                    RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                    roleInfoComponentServer.LearnRecipe(skillmakeId);
                    return;
                }
                if (message.GMMsg.Contains("realnamereset#"))
                {
                    long accountId = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.AccInfoID;
                    if (accountId <= 0)
                    {
                        Log.Warning($"realnamereset AccInfoID invalid unit:{unit.Id}");
                        return;
                    }

                    if (!StartZoneConfigCategory.Instance.Contain(CommonConfig.CenterZoneId))
                    {
                        Log.Warning($"realnamereset center zone missing account:{accountId}");
                        return;
                    }

                    Game.Scene.GetComponent<DBComponent>().InitDatabase(StartZoneConfigCategory.Instance.Get(CommonConfig.CenterZoneId));
                    DBCenterAccountInfo dbCenterAccountInfo = await DBHelper.GetComponent<DBCenterAccountInfo>(CommonConfig.CenterZoneId, accountId);
                    if (dbCenterAccountInfo == null || dbCenterAccountInfo.PlayerInfo == null)
                    {
                        Log.Warning($"realnamereset account not found:{accountId}");
                        return;
                    }

                    dbCenterAccountInfo.PlayerInfo.RealNameV2 = 0;
                    await DBHelper.SaveComponent(CommonConfig.CenterZoneId, dbCenterAccountInfo.Id, dbCenterAccountInfo);
                    Log.Warning($"realnamereset RealNameV2=0 account:{accountId} unit:{unit.Id}");
                    return;
                }
                if (message.GMMsg.Contains("skillmakeall#"))
                {
                    RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                    RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                    for (int makeType = 1; makeType <= 5; makeType++)
                    {
                        if (roleInfo.MakeTypeList.Contains(makeType))
                        {
                            continue;
                        }
                        roleInfo.MakeTypeList.Add(makeType);
                    }

                    foreach (LDSkill_Make config in LDSkill_MakeCategory.Instance.GetAll().Values)
                    {
                        if (roleInfo.MakeIdList.Contains(config.Id))
                        {
                            continue;
                        }
                        roleInfo.MakeIdList.Add(config.Id);
                    }

                    MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfo });
                    DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, roleInfoComponentServer).Coroutine();
                    return;
                }
                if (message.GMMsg.Contains("roleHeadId#"))
				{
					//roleHeadId#头像Id
					if (!TryGetSecondHashNumber(message.GMMsg, out int roleHeadId))
					{
						return;
					}
					if (!LDRoleHeadCategory.Instance.Contain(roleHeadId))
					{
						return;
					}
					RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
					roleInfoComponentServer.RoleInfo.AddRoleHead(roleHeadId);
					MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfoComponentServer.RoleInfo });
					return;
				}
				if (message.GMMsg.Contains("roleHeadFrame#"))
				{
					//roleHeadFrame#头像框Id
					if (!TryGetSecondHashNumber(message.GMMsg, out int roleHeadFrame))
					{
						return;
					}
					if (!LDRoleHeadFrameCategory.Instance.Contain(roleHeadFrame))
					{
						return;
					}
					RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
					roleInfoComponentServer.RoleInfo.AddRoleHeadFrame(roleHeadFrame);
					MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfoComponentServer.RoleInfo });
					return;
				}

				if (message.GMMsg.Contains("title#"))
				{
                    //title#称号Id
                    if (!TryGetSecondHashNumber(message.GMMsg, out int titleId))
					{
						return;
					}

                    Console.WriteLine($"C2M_GMCommandRequest: titleId; {titleId}");

                    if (!LDTitleCategory.Instance.Contain(titleId))
					{
						return;
					}
					TitleComponentServer titleComponent = unit.GetComponent<TitleComponentServer>();
					titleComponent.OnActiveTile(titleId);
					titleComponent.TitleUpdateResult.TitleList = titleComponent.TitleList;
					MessageHelper.SendToClient(unit, titleComponent.TitleUpdateResult);
					Function_Fight.UnitUpdateProperty_Base(unit, true, true);
					return;
				}
				if (message.GMMsg.Contains("marquee#"))
				{
                    //marquee#跑马灯Id#VIP等级
                    if (commands.Length < 3)
                    {
                        return;
                    }

                    if (!int.TryParse(commands[1], out int marqueeId) || !int.TryParse(commands[2], out int vipLevel))
                    {
                        return;
                    }

                    if (!LDMarqueeCategory.Instance.Contain(marqueeId))
                    {
                        return;
                    }

                    LDMarquee ldMarquee = LDMarqueeCategory.Instance.Get(marqueeId);
                    string roleName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                    _ = string.Format(ldMarquee.CN, vipLevel, roleName);
                    long expireTime = 0;
                    if (ldMarquee.Lifespan > 0)
                    {
                        expireTime = TimeHelper.ServerNow() + ldMarquee.Lifespan * 1000;
                    }
      
                    ServerMessageHelper.SendMarquee(UnitZoneHelper.GetCurrentZone(unit), marqueeId, expireTime, vipLevel.ToString(), roleName);
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
                    mailInfo.ValidTime = TimeHelper.ServerNow() + TimeHelper.OneDay * 30;
					mailInfo.ParamList.Add("AAAA");
                    mailInfo.ParamList.Add("BBBB");

                    if (RandomHelper.RandFloat01() < 0.5f)
                    {
                        mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1001100, ItemNum = 1, ItemFlags = ItemFlagEnum.Limited });
                        mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1002100, ItemNum = 1 });
                    }
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
                if (message.GMMsg.Contains("jitui"))
                {
                    HitFlyServerHelper.GmNearbyMonsters(unit);
                    return;
                }
                if (message.GMMsg.Contains("addspeed#"))
                {
					int addspeed = int.Parse(message.GMMsg.Split("#")[1]);
                    unit.GetComponent<NumericComponent>().ChangeAttrFixed(null, NumericType.Speed_Fixed_16, addspeed, 0);
                    return;
                }
				if (message.GMMsg.Contains("gettask#"))
				{
                    int taskid = int.Parse(message.GMMsg.Split("#")[1]);
                    unit.GetComponent<TaskComponentServer>().OnAcceptedTask_1(taskid, 231);
				}
                if (message.GMMsg == "resetguide#")
				{
					unit.GetComponent<RoleInfoComponentServer>().RoleInfo.CompleteGuideIds.Clear();
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
							case ItemBigType.Type_Mount:
								unit.GetComponent<MountComponentServer>().OnAddMount(ItemGetWay.GM, itemId);
								break;
                            case ItemBigType.Type_Elf:
                                break;
							case ItemBigType.Type_Title:
                                TitleComponentServer titleComponent = unit.GetComponent<TitleComponentServer>();
                                titleComponent.OnActiveTile(itemId);
                                titleComponent.TitleUpdateResult.TitleList = titleComponent.TitleList;
                                MessageHelper.SendToClient(unit, titleComponent.TitleUpdateResult);
                                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                                break;
                        }
						break;
                    case "2":      //2#13091#1
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
							Vector3 vector3 = RandomHelper.GetRandomPointInCircle(unit.Position,2f, 6f);
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
					case "6":
						int newLevel = int.Parse(commands[1]);
						RoleInfoComponentServer roleInfoComponentServer =  unit.GetComponent<RoleInfoComponentServer>();
						TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();	
                        //if (newLevel <= roleInfoComponent.GetMaxLevel(taskComponent.RoleComoleteTaskList_2))
						if(newLevel <= 80)
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
