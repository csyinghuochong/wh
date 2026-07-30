using MsgCryptTest;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_GMCommandHandler : AMActorLocationHandler<Unit, C2M_GMCommandRequest>
    {
		protected override async ETTask Run(Unit unit, C2M_GMCommandRequest message)
		{
			try
			{
				string[] commands = message.GMMsg.Split('#');
				if (commands.Length == 0)
				{
					return;
				}
				if (message.GMMsg == "#allmonster")
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
				if (message.GMMsg == "#testmail")
				{
                    for(int i = 0; i < 120; i++)
					{
                        MailInfo mailInfo = new MailInfo();
                        mailInfo.Status = 0;
						mailInfo.Context = i + "_________" + i;
                        mailInfo.Title = "系统通知";
                        mailInfo.MailId = IdGenerater.Instance.GenerateId();
						mailInfo.Form = "官方xxx";
						mailInfo.ValidTime = TimeHelper.ServerNow() + RandomHelper.RandInt64();
                        await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(unit), unit.Id, mailInfo);
                    }
                }
                if (message.GMMsg == "#mianshang" || message.GMMsg == "#wudi")
				{
					BuffManagerComponent buffManager = unit.GetComponent<BuffManagerComponent>();
					BuffData buffData = new BuffData();
					buffData.SkillId = 67000278;
					buffData.BuffId = message.GMMsg == "#mianshang" ? 90106002 : 90106003;
					buffManager.BuffFactory(buffData, unit, null);
					return;
				}
				if (message.GMMsg == "#openall")
				{
					unit.GetComponent<RoleInfoComponentServer>().OpenAll();
					return;
				}
				if (message.GMMsg == "#resetlv")
				{
					RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
					int level = roleInfoComponentServer.RoleInfo.Lv - 1;
					roleInfoComponentServer.UpdateRoleData( UserDataType.Level, (level*-1).ToString());
					return;
				}
                if (message.GMMsg == "#jiandian")
                {
					unit.GetComponent<BagComponentServer>().OnAddJianDing();
                    return;
                }

                if (message.GMMsg == "#killall")
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
                if (message.GMMsg == "#killpet")
                {
                    List<Unit> units = unit.GetParent<UnitComponent>().GetAll();
                    for (int i = units.Count - 1; i >= 0; i--)
                    {
                        if (units[i].Type != UnitType.Pet)
                        {
                            continue;
                        }
                        units[i].GetComponent<NumericComponent>().ApplyChange(unit, NumericType.Numeric_Error, -1000000000, 0);
                    }
                    return;
                }

                if (message.GMMsg == "#killmonster")
				{
					List<Unit> units = unit.GetParent<UnitComponent>().GetAll();
					for (int i = units.Count - 1; i >= 0; i--)
					{
						if (units[i].Type != UnitType.Monster)
						{
							continue;
						}
						if (units[i].GetMonsterType() == (int)MonsterTypeEnum.Boss)
						{
							continue;
						}
						units[i].GetComponent<NumericComponent>().ApplyChange(unit, NumericType.Numeric_Error, -1000000000, 0);
					}
					return;
				}
				if (message.GMMsg == "#resetguide")
				{
					unit.GetComponent<RoleInfoComponentServer>().RoleInfo.CompleteGuideIds.Clear();
					return;
				}
				if (message.GMMsg == "#resetfuben")
				{
					NumericComponent numeric = unit.GetComponent<NumericComponent>();
					numeric.ApplyValue(NumericType.TeamDungeonTimes, 0);
					numeric.ApplyValue(NumericType.TeamDungeonXieZhu, 0);
					unit.GetComponent<RoleDailyDataComponentServer>()?.GetDailyData().DayFubenTimes.Clear();
					return;
				}
                if (message.GMMsg == "#resettower")
                {
                    NumericComponent numeric = unit.GetComponent<NumericComponent>();
                    numeric.ApplyValue(NumericType.SeasonTowerId, 0);
                    return;
                }
                if (message.GMMsg == "#ceshi1203")
                {
                    RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
                    int level = 70 - roleInfo.RoleInfo.Lv;
                    level = level > 0 ? level : 0;		
                    roleInfo.UpdateRoleData(UserDataType.Level, level.ToString());
                    return;
                }
                if (message.GMMsg == "#completetask")
				{
					unit.GetComponent<TaskComponentServer>().GMCompletCurrentTask();
					return;
				}
                if (message.GMMsg == "#resetweek")
                {
                    unit.GetComponent<TaskComponentServer>().UpdateWeeklyTask(true);
                    return;
                }
                if (message.GMMsg.Contains("#addack"))  //#addack#400000
                {
					int addAck = int.Parse(commands[2]);
					unit.GetComponent<NumericComponent>().Set(NumericType.Numeric_Error, addAck);
					return;
				}
				if (message.GMMsg.Contains("#wechattoken"))
				{
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    keyValuePairs.Add("grant_type", "client_credential");
                    keyValuePairs.Add("appid", ConfigData.sAppID);
                    keyValuePairs.Add("secret", ConfigData.sAppSecret);
                    string requestBody = await WXSample.OnGetAccessToken("https://api.weixin.qq.com/cgi-bin/token", keyValuePairs);
					Log.Debug($"wechattoken_requestBody:  {requestBody}");
                    Console.WriteLine($"wechattoken_requestBody:  {requestBody}");
                    //wechattoken_requestBody:  {"access_token":"95_-mqddzTt-bGzxjgFpgRzNhG2DNQA1N_KdfUmAwRy2VduhCjwejaGpl-Plvs05NIoPu-kuc-yx0kyziOOSoP88WEdX3bVEaS_OZTPuw20u8-rzFdzy_esO_R0SXQTBOiAHAYGT","expires_in":7200}
                    
                    return;
                }
				switch (int.Parse(commands[0]))
				{
					case 1:             //新增道具1#1#1001#1 1#2#1012#1 【添加道具/道具类型/道具id/道具数量】
						int itemType  =int.Parse(commands[1]);
						int itemId = int.Parse(commands[2]);
						int itemNumber = int.Parse(commands[3]);
						
						switch (itemType)
						{
							case ItemBigType.Type_Exp:
							case ItemBigType.Type_Item:
							case ItemBigType.Type_Equip:
								unit.GetComponent<BagComponentServer>().OnAddItemData($"{itemType}_{itemId}_{itemNumber}", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", true);
								break;
							case ItemBigType.Type_Pet:
								unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.GM, itemId);
								break;
                            case ItemBigType.Type_Elf:
                                unit.GetComponent<ChengJiuComponentServer>().OnActiveJingLing(itemId);
                                break;
                        }

						break;
                    //70001001  0    71001010    1       70001003     2      70001011    3
                    case 2:       //72009041死亡技能      //2#152#29#-67#72000198#1  90000005-爆炸怪 72002013-脱战技能没移除2#-78#0#0.7#72004002#1  70001001  72009001
                        float posX = float.Parse(commands[1]);
						float posY = float.Parse(commands[2]); 
						float posZ = float.Parse(commands[3]);
						int monsterId = int.Parse(commands[4]);
						int number = int.Parse(commands[5]);
						if (number > 100)
						{
							Log.Error("number > 100");
							return;
						}

                        //83000101 83000104
                        for (int c = 0; c < number; c++)
						{
							await TimerComponent.Instance.WaitAsync(1);
							Vector3 vector3 = new Vector3(posX + RandomHelper.RandomNumberFloat(-1, 1), posY, posZ + RandomHelper.RandomNumberFloat(-1, 1));
							Unit monster = UnitFactory.CreateMonster(unit.DomainScene(), monsterId, vector3, new CreateMonsterInfo()
							{ 
								Camp = CampEnum.CampMonster1,
								MasterID = monsterId == 80002010 ? unit.Id : 0
                            });

							//M2C_CreateSpilings createSpilings = new M2C_CreateSpilings();
							//SpilingInfo spilingInfo = UnitHelper.CreateSpilingInfo(monster);
							//createSpilings.Spilings.Add(spilingInfo);
							//MessageHelper.Broadcast(unit, createSpilings);
						}
						break;
					case 4: //直接接取某个任务      4#30080019
                        unit.GetComponent<TaskComponentServer>().OnGMGetTask(int.Parse(commands[1]));
						break;
					case 5: //直接获得某个宠物      5#1001301
                        unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.GM, int.Parse(commands[1]));
						break;
					case 6:
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
					case 7:
						long userID = long.Parse(commands[1]);
						long dbCacheId = DBHelper.GetUnitCacheConfig(userID);

						List<string> componentList = new List<string>() { DBHelper.BagComponentServer, DBHelper.TaskComponent };
						D2G_GetComponent d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userID, Component = DBHelper.RoleInfoComponent });
						roleInfoComponentServer = d2GGetUnit.Component as RoleInfoComponentServer;
						for (int i = 0; i < componentList.Count; i++)
						{
							d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userID, Component = componentList[i] });
							if (d2GGetUnit.Component == null)
							{
								continue;
							}
						}
						break;
					case 8:
						NumericComponent numericComponentGm = unit.GetComponent<NumericComponent>();
						numericComponentGm.ApplyValue(NumericType.Ling_DiLv, int.Parse(commands[1]));
						numericComponentGm.ApplyValue(NumericType.Ling_DiExp, 0);
						break;
					case 9:
						long robotSceneId = DBHelper.GetRobotServerId();
                        MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest()
                        {
                            Zone = UnitZoneHelper.GetHomeZone(unit),
                            MessageType = NoticeType.YeWaiBoss,
                            Message = $"{2000002}@{7};{0};{15}@{72000003}@{commands[1]}"
                        });
						break;
					case 10:
                        Log.Warning("刷新机器人！！");
                        robotSceneId = DBHelper.GetRobotServerId();
                        MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest() { Zone = UnitZoneHelper.GetHomeZone(unit), MessageType = 18, Message = $"1001#{commands[1]}" });
                        break;
					case 11: //11#92041030   11#80002003   11#80002005  11#97050403
                        {
                            BuffManagerComponent buffManager = unit.GetComponent<BuffManagerComponent>();
                            BuffData buffData_2 = new BuffData();
                            buffData_2.SkillId = 67000278;
                            buffData_2.BuffId = int.Parse(commands[1]); 
                            buffManager.BuffFactory(buffData_2, unit, null);
                        }
                        break;
					case 12:
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
					case 13:
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
					case 14:
                        unit.GetComponent<BagComponentServer>().OnAddJianDing();
                        break;
					case 17: //进入副本
						break;
					case 19:
                        break;
					case 21: // 跨服旅游 21#目标区号  例：21#2；填本服区号则回本服主城
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
