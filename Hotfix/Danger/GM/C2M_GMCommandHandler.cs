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
				// 输入框手误时末尾常多一个空行，先去掉再匹配和解析
				message.GMMsg = message.GMMsg.TrimEnd();
				string[] commands = message.GMMsg.Split('#');
 
				Console.WriteLine($"C2M_GMCommandRequest: {message.GMMsg}");

				if (commands.Length == 0)
				{
					return;
				}

                if (message.GMMsg == "fillbag#")
                {
                    if (unit.Type != UnitType.Player)
                    {
                        return;
                    }

                    // fillbag#：先补满已有格子的堆叠，再遍历 Item 将常规、材料两页的空格填为满堆，不触发自动使用。
                    BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                    M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
                    string getWay = $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}";
                    foreach (int bagType in new[] { 1, 901 })
                    {
                        int loc = ItemNewHelper.GetBagBelongId(bagType);
                        if (!ItemNewHelper.IsValideBagLoc(loc))
                        {
                            continue;
                        }

                        foreach (BagInfo existing in bag.GetItemByLoc((ItemLocType)loc))
                        {
                            int pileSum = ItemNewHelper.GetNewItemPileSum(existing);
                            if (existing.ItemNum >= pileSum)
                            {
                                continue;
                            }

                            existing.ItemNum = pileSum;
                            bagUpdate.BagInfoUpdate.Add(existing);
                        }

                        int leftCells = bag.GetBagLeftCell(loc);
                        if (leftCells <= 0)
                        {
                            continue;
                        }

                        List<LDItem> items = LDItemCategory.Instance.GetAll().Values.Where(item => item.BagType == bagType
                            && item.IfBag == 1 && item.IfAutoUse == 0 && item.ItemType != ItemTypeEnum.SubType_UnionResource).ToList();
                        if (items.Count == 0)
                        {
                            Log.Warning($"fillbag 无可用道具 BagType={bagType}");
                            continue;
                        }

                        // 固定按初始空格数新建，避免普通入包不断叠加而不能占满空格。
                        for (int i = 0; i < leftCells; i++)
                        {
                            LDItem item = items[i % items.Count];
                            BagInfo newItem = new BagInfo
                            {
                                ItemType = ItemBigType.Type_Item, ItemID = item.Id, Loc = loc, GetWay = getWay,
                                ItemFlags = ItemTradeStatus.Tradable,
                            };
                            bag.OnAddItemDataNewCell(newItem, ItemNewHelper.GetNewItemPileSum(newItem));
                        }
                    }

                    if (bagUpdate.BagInfoUpdate.Count > 0)
                    {
                        MessageHelper.SendToClient(unit, bagUpdate);
                    }
                    await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, bag);
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
                    return;
                }

                if (message.GMMsg.Contains("alladdbuff#"))
                {
                    if (!TryGetSecondHashNumber(message.GMMsg, out int buffId)
                        || buffId <= 0 || !LDSkill_Battle_BuffCategory.Instance.Contain(buffId))
                    {
                        return;
                    }

                    // alladdbuff#BuffId：当前地图玩家和怪物添加 10 秒 Buff；快照分帧处理，每 10 个单位让出一帧。
                    List<Unit> targets = unit.GetParent<UnitComponent>().GetAll()
                            .Where(target => target.Type == UnitType.Player || target.Type == UnitType.Monster).ToList();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (i > 0 && i % 10 == 0)
                        {
                            await TimerComponent.Instance.WaitFrameAsync();
                            if (unit.IsDisposed)
                            {
                                return;
                            }
                        }

                        Unit target = targets[i];
                        if (target.IsDisposed)
                        {
                            continue;
                        }

                        BuffManagerComponent targetBuffManager = target.GetComponent<BuffManagerComponent>();
                        if (targetBuffManager == null)
                        {
                            continue;
                        }

                        BuffData buffData = new BuffData
                        {
                            SkillId = 1,
                            BuffId = buffId,
                            BuffEndTime = TimeHelper.ServerNow() + 10000,
                        };
                        targetBuffManager.BuffFactory(buffData, unit, null);
                    }

                    return;
                }

                if (message.GMMsg.Contains("addbuff#"))
                {
                    if (!TryGetSecondHashNumber(message.GMMsg, out int buffid))
                    {
                        return;
                    }

                    BuffManagerComponent buffManager = unit.GetComponent<BuffManagerComponent>();
                    BuffData buffData_2 = new BuffData();
                    buffData_2.SkillId = 1;
                    buffData_2.BuffId = buffid;
                    // GM 直接添加没有技能时长来源，默认持续 2 秒后结束。
                    buffData_2.BuffEndTime = TimeHelper.ServerNow() + 10000;
                    buffManager.BuffFactory(buffData_2, unit, null);
                    return;
                }
                if (message.GMMsg.Contains("testequip#"))
                {
                    // testequip#1010500
                    if (!TryGetSecondHashNumber(message.GMMsg, out int equipId))
                    {
                        return;
                    }

                    if (!LDEquipCategory.Instance.Contain(equipId))
                    {
                        Log.Warning($"testequip equip not found:{equipId}");
                        return;
                    }

                    List<string> lines = new List<string>();
                    for (int i = 0; i < 1000; i++)
                    {
                        List<AttributeItem> attrs = LDEquipCategory.Instance.GetEquipAttribute(equipId);
                        string line = string.Empty;
                        for (int a = 0; a < attrs.Count; a++)
                        {
                            if (a > 0)
                            {
                                line += "\t";
                            }

                            line += $"{attrs[a].AttributeID}:{attrs[a].AttributeValue}";
                        }

                        lines.Add(line);
                    }

                    LogHelper.WriteLogList(lines, "../Logs/WJ_EquipAttri.txt", false);
                    Log.Warning($"testequip {equipId} count:{lines.Count} -> ../Logs/WJ_EquipAttri.txt");
                    return;
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
                if (message.GMMsg == "fashionallopen#")
                {
                    // fashionallopen#：激活全部时装，保留已有激活记录和当前穿戴。
                    BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                    long now = TimeHelper.ServerNow();
                    int addedCount = 0;
                    foreach (LDFashion fashionConfig in LDFashionCategory.Instance.GetAll().Values)
                    {
                        if (bagComponentServer.GetFashionInfo(fashionConfig.Id) != null)
                        {
                            continue;
                        }

                        bagComponentServer.FashionActiveList.Add(new FashionInfo()
                        {
                            FashionId = fashionConfig.Id,
                            ActivteTime = now
                        });
                        addedCount++;
                    }

                    Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                    bagComponentServer.SyncFashionActiveList(unit);
                    Log.Warning($"fashionallopen 成功 新激活数={addedCount} 持有数={bagComponentServer.FashionActiveList.Count}");
                    return;
                }

                if (message.GMMsg.Contains("fashionwear#"))
                {
                    // fashionwear#时装Id          穿戴（GM 自动激活，绕过获取途径与持有校验，方便试穿任意时装）
                    // fashionwear#时装Id#1        穿戴；#2 卸下，不填操作类型默认穿戴
                    if (!TryGetSecondHashNumber(message.GMMsg, out int fashionId))
                    {
                        Log.Warning("fashionwear 格式：fashionwear#时装Id[#1穿戴/2卸下]");
                        return;
                    }

                    int operateType = 1;
                    if (commands.Length >= 3 && int.TryParse(commands[2].Trim(), out int op))
                    {
                        operateType = op;
                    }

                    if (!LDFashionCategory.Instance.Contain(fashionId))
                    {
                        Log.Warning($"fashionwear 时装配置不存在 Id={fashionId}");
                        return;
                    }

                    BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                    RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;

                    // GM 特权：没有激活记录就补一条，正常途径仍走激活/获取。
                    bool fashionAdded = false;
                    if (bagComponentServer.GetFashionInfo(fashionId) == null)
                    {
                        bagComponentServer.FashionActiveList.Add(new FashionInfo()
                        {
                            FashionId = fashionId,
                            ActivteTime = TimeHelper.ServerNow()
                        });
                        fashionAdded = true;
                    }

                    // 部位覆盖、性别校验、卸下限制等规则与 C2M_FashionWearHandler 共用一份
                    int error = bagComponentServer.ApplyFashionWear(fashionId, operateType, roleInfo.Occ, roleInfo.Sex);
                    if (error != ErrorCode.ERR_Success)
                    {
                        Log.Warning($"fashionwear 失败 FashionId={fashionId} Operate={operateType} Error={error}");
                        return;
                    }

                    Function_Fight.UnitUpdateProperty_Base(unit, true, true);

                    // GM 由服务端直接改数据，客户端不会本地预更新，必须自己也收到 M2C_FashionUpdate
                    UnitHelper.SyncUnitAppear(unit);

                    // 外观广播与持有列表分开：补过激活记录就只给自己同步一次全量持有列表，
                    // 客户端不用等到下次拉背包才知道这件时装已拥有。
                    if (fashionAdded)
                    {
                        bagComponentServer.SyncFashionActiveList(unit);
                    }

                    Log.Warning($"fashionwear 成功 FashionId={fashionId} Operate={operateType} EquipCount={bagComponentServer.FashionEquipList.Count}");
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
                        mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1001100, ItemNum = 1, ItemFlags = ItemTradeStatus.Limited });
                        mailInfo.ItemList.Add(new BagInfo() { ItemType = ItemBigType.Type_Equip, ItemID = 1002100, ItemNum = 1, ItemFlags = ItemTradeStatus.Tradable });
                    }
                    await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(unit), unit.Id, mailInfo);
                }
               
                if (message.GMMsg == "enddungeon#")
                {
                    MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
                    if (mapComponent == null || mapComponent.MapTypeEnum != (int)MapTypeEnum.TeamDungeon)
                    {
                        return;
                    }

                    TeamDungeonComponent teamDungeon = unit.DomainScene().GetComponent<TeamDungeonComponent>();
                    if (teamDungeon == null)
                    {
                        return;
                    }

                    teamDungeon.OnDungeonOver();
                    return;
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
					unit.GetComponent<RoleInfoComponentServer>().RoleInfo.BeginnerGuideProgresses.Clear();
					return;
				}
                if (message.GMMsg == "dailyreset#")
                {
                    // 同一游戏日已清过会直接 return。每次 GM 都再走一遍玩家日清，先把上次日清时间拨回上一个游戏日。
                    RoleDailyDataComponentServer roleDailyData = unit.GetComponent<RoleDailyDataComponentServer>();
                    long now = TimeHelper.ServerNow();
                    if (roleDailyData.LastResetTime > 0 && ActivityHelper.IsSameGameResetDay(roleDailyData.LastResetTime, now))
                    {
                        roleDailyData.LastResetTime = now - TimeHelper.OneDay;
                    }

                    if (ActivityHelper.IsGameWeekResetDay(now))
                    {
                        PlayerDailyResetHelper.OnWeeklyReset(unit, true);
                    }

                    PlayerDailyResetHelper.OnDailyReset(unit, 2);
                    Log.Warning($"dailyreset unit:{unit.Id}");
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
								// GM 发放默认可交易，显式传入交易状态，避免缺省状态 0 被入包校验拒绝。
								unit.GetComponent<BagComponentServer>().OnAddItemData($"{itemType}~{itemId}~{itemNumber}~{ItemTradeStatus.Tradable}", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", true);
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
                            case ItemBigType.Type_Fashion:
                            {
                                // 1#<Type_Fashion>#<时装Id>#<续期天数>
                                // 规则与 C2M_FashionActiveHandler 一致：无记录则激活（ActivteTime=服务器当前时间），
                                // 已有记录则按 itemNumber 续期（不填/非法时按 1 天）。GM 绕过获取途径与消耗。
                                if (itemId <= 0 || !LDFashionCategory.Instance.Contain(itemId))
                                {
                                    Log.Warning($"GM 添加时装失败，配置不存在 Id={itemId}");
                                    break;
                                }

                                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                                if (bagComponentServer == null)
                                {
                                    break;
                                }

                                int renewDays = itemNumber > 0 ? itemNumber : 1;
                                FashionInfo fashionInfo = bagComponentServer.GetFashionInfo(itemId);
                                if (fashionInfo == null)
                                {
                                    bagComponentServer.FashionActiveList.Add(new FashionInfo()
                                    {
                                        FashionId = itemId,
                                        ActivteTime = TimeHelper.ServerNow()
                                    });
                                }
                                else
                                {
                                    fashionInfo.ActivteTime += (long)TimeHelper.OneDay * renewDays;
                                }

                                // 激活会加属性，与正常激活协议保持一致
                                Function_Fight.UnitUpdateProperty_Base(unit, true, true);

                                // 只同步给自己：GM 由服务端直接改数据，客户端不会本地预更新；
                                // 持有列表是隐私数据，也不该像 M2C_FashionUpdate 那样广播给视野内玩家。
                                bagComponentServer.SyncFashionActiveList(unit);
                                Log.Warning($"GM 添加时装成功 FashionId={itemId} 持有数={bagComponentServer.FashionActiveList.Count}");
                                break;
                            }
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
						if (newLevel < roleInfoComponentServer.RoleInfo.Lv && !CommonHelper.IsInnerNet())
						{
							return;
						}

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
