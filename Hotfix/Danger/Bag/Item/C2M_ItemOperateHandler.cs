using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemOperateHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOperateRequest, M2C_ItemOperateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemOperateRequest request, M2C_ItemOperateResponse response, Action reply)
        {
            try
            {
                //获取UserID及User数据
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
                RoleInfo useInfo = roleInfoComponentServer.RoleInfo;
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                long bagInfoID = request.OperateBagID;

                ItemLocType locType = ItemLocType.ItemLocBag;
              
                if (request.OperateType == 4)
                {
                    locType = ItemLocType.ItemLocEquip;
                }
                if (request.OperateType == 7)
                {
                    locType = (ItemLocType)(int.Parse(request.OperatePar));
                }

                int weizhi = -1;
                LDItem ldItem = null;
                BagInfo useBagInfo = bagComponentServer.GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null && request.OperateType != 8)
                {
                    reply();
                    return;
                }
                if (useBagInfo != null)
                {
                    ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);
                    weizhi = ldItem.ItemType;
                }

                //通知客户端背包刷新
                M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
                //使用道具
                if (request.OperateType == 1 && ldItem != null)
                {
                    Scene domainScene = unit.DomainScene();
                    MapComponent mapComponent = domainScene.GetComponent<MapComponent>();
                    string[] itemOperateParParts = null;
                    if (ldItem.Id == 10000156)
                    {
                        reply();
                        return;
                    }
                    if (ldItem.DayUseNum > 0 && (daily?.GetDayItemUse(ldItem.Id) ?? 0) >= ldItem.DayUseNum)
                    {
                        response.Error = ErrorCode.ERR_ItemNoUseTime;
                        reply();
                        return;
                    }
                    if (ldItem.SumUseNum > 0 && roleInfoComponentServer.GetTotalUseTimes(ldItem.Id) >= ldItem.SumUseNum)
                    {
                        response.Error = ErrorCode.ERR_ItemNoUseTime;
                        reply();
                        return;
                    }

                    //获取背包数据
                    int costNumber = 1;
                    bool bagIsFull = false;
                    List<RewardItem> droplist = new List<RewardItem>();
                    if (ldItem.ItemType == 110 && mapComponent.SceneId != 2000001) // 领主怪物召唤
                    {
                        response.Error = ErrorCode.ERR_ItemOnlyUseMiJing;
                        reply();
                        return;
                    }

                    if ((ldItem.ItemType == 111 || ldItem.ItemType == 112)
                        && CommonConfig.BatchUseItemList.Contains(ldItem.Id))
                    {
                        //目前只有111类型支持批量使用
                        if (!string.IsNullOrEmpty(request.OperatePar))
                        {
                            if (ldItem.ItemType == 112)
                            {
                                // 经验盒子特殊处理，有免费开启和钻石开启
                                itemOperateParParts = request.OperatePar.Split(';');
                                costNumber = int.Parse(itemOperateParParts[1]);

                                string[] expInfos = null;//ldItem.ItemUsePar;
                                int needZuanshi = itemOperateParParts[0] == "1" ? int.Parse(expInfos[0]) * costNumber : 0;
                                string[] paramInfo = expInfos[int.Parse(itemOperateParParts[0])].Split(';');
                             
                                //如果当前钻石不足返回错误
                                if (roleInfoComponentServer.RoleInfo.Diamond < needZuanshi)
                                {
                                    response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                                    reply();
                                    return;
                                }
                            }
                            else
                            {
                                costNumber = int.Parse(request.OperatePar);
                            }
                        }

                    }

                    if (ldItem.ItemType == 14      //召唤卷轴
                        || ldItem.ItemType == 114) //宝石
                    {
                        costNumber = 0;
                    }
                    if (ldItem.ItemType == 112)   //经验木桩
                    {
                        int openDay = DBHelper.GetOpenServerDay(unit);
                        if (openDay <= 1)
                        {
                            response.Error = ErrorCode.ERR_ItemNoUseTime;
                            reply();
                            return;
                        }
                    }
                    if (ldItem.ItemType == 127)
                    {
                        if (bagComponentServer.GetBagLeftCell() < 1)
                        {
                            bagIsFull = true;
                        }
                    }
                    if (ldItem.ItemType == 137)
                    {
                        //检测要附灵的宠物蛋是否存在
                        long chongwudanId = long.Parse(request.OperatePar);      
                        BagInfo chongwudan = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, chongwudanId);
                        if (chongwudan == null)
                        {
                            response.Error = ErrorCode.ERR_ItemNotExist;
                            reply();
                            return;
                        }
                    }
                    if (ldItem.ItemType == 138)
                    {
                        if (numericComponent.GetAsInt(NumericType.TeamDungeonTimes) <= 0)
                        {
                            response.Error = ErrorCode.ERR_TeamDungeonTimesMax;
                            reply();
                            return;
                        }
                    }

                    if (ldItem.ItemType == 142)
                    {
                        if (bagComponentServer.GetBagLeftCell() < 1)
                        {
                            bagIsFull = true;
                        }
                        if (request.OperatePar!="0" && request.OperatePar != "1")
                        {
                            request.OperatePar = "0";
                        }
                    }

                    if (bagIsFull)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }

                    if (ldItem.ItemType != 1
                       && ldItem.ItemType != 2)
                    {
                        reply();
                        return;
                    }
                    if (bagComponentServer.OnCostItemData(useBagInfo, ItemLocType.ItemLocBag, costNumber))
                    {
                        bool costItemStatus = true;
                        //根据道具子类分发不同的功能
                        switch (ldItem.ItemType)
                        {
                            //增加金币
                            case 1:
                              //  roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Gold, ldItem.ItemUsePar, true, ItemGetWay.ItemBox_6);
                                break;
                            //增加经验
                            case 2:
                               // roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Exp, ldItem.ItemUsePar, true, ItemGetWay.ItemBox_6);
                                break;
                            //回城卷轴[返回另外一个副本场景]
                            case 4:
                                if (mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
                                {
                                    LocalDungeonComponent localDungeon = domainScene.GetComponent<LocalDungeonComponent>();
                                    //TransferHelper.LocalDungeonTransfer(unit, 0, int.Parse(ldItem.ItemUsePar), localDungeon.FubenDifficulty).Coroutine();
                                }
                                break;
                            //图纸制造
                            case 5:
                                break;
                            //随机宝箱
                            case 6:
                                int dropId = 0;
                                try
                                {
                                   // dropId = int.Parse(ldItem.ItemUsePar);
                                }
                                catch (Exception ex)
                                { 
                                    Log.Error(ex.ToString() + $"{ldItem.Id}   dropId ==0");

                                }
                                if (dropId > 0)
                                {
                                    DropHelper.DropIDToDropItem_2(dropId, droplist);
                                    bagComponentServer.OnAddItemData(droplist, string.Empty, $"{ItemGetWay.ItemBox_6}_{TimeHelper.ServerNow()}_{ldItem.Id}");
                                }
                                break;
                            //兑换：
                            case 8:
                                string[] duihuanparams =  null;//ldItem.ItemUsePar;
                                int neednum = int.Parse(duihuanparams[0]);
                                int newItem = int.Parse(duihuanparams[1]);

                                bagComponentServer.OnCostItemData($"{ldItem.Id};{neednum - 1}", ItemLocType.ItemLocBag, ItemGetWay.DuiHuan);
                                bagComponentServer.OnAddItemData($"{newItem};1", $"{ItemGetWay.ItemBox_8}_{TimeHelper.ServerNow()}");
                                break;
                            case 9:
                                bagComponentServer.OnAddItemData(droplist, string.Empty, $"{ItemGetWay.ActivityHongBao}_{TimeHelper.ServerNow()}");
                                break;
                            //冷却时间清空卷轴"
                            case 12:
                                roleInfoComponentServer.OnCleanBossCD();
                                if (mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
                                {
                                    domainScene.GetComponent<LocalDungeonComponent>().OnCleanBossCD();
                                }
                                break;
                            //召唤卷轴
                            case 14:
                                if (mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
                                {
                                    //UnitFactory.CreateTempFollower(unit, int.Parse(ldItem.ItemUsePar));
                                }
                                break;
                           
                            case 16: //附魔技能
                               // roleInfoComponentServer.RoleInfo.MakeList.Add(int.Parse(ldItem.ItemUsePar));
                                break;
                            //使用技能
                            case 101:
                                break;
                            //宠物蛋
                            case 102:
                            case 103:
                                {
                                    string[] getway = useBagInfo.GetWay.Split('_');
                                    if (ldItem.ItemType == 102)
                                    {
                                        //unit.GetComponent<PetComponentServer>().OnAddPet(int.Parse(getway[0]), int.Parse(ldItem.ItemUsePar), 0, useBagInfo.FuLing);
                                    }
                                    else
                                    {
                                        int skinId = 0;
                                        if(!string.IsNullOrEmpty(useBagInfo.ItemPar))
                                        {
                                            skinId = int.Parse(useBagInfo.ItemPar);
                                        }
                                    }
                                }
                                break;
                            //随机盒子
                            case 104:
                                bagComponentServer.OnAddItemData(droplist, string.Empty, $"{ItemGetWay.ItemBox_104}_{TimeHelper.ServerNow()}");
                                break;
                            //指定道具
                            case 106:
                              //  unit.GetComponent<BagComponentServer>().OnAddItemData(ldItem.ItemUsePar, $"{ItemGetWay.ItemBox_106}_{TimeHelper.ServerNow()}");
                                break;
                            //永久技能
                            case 107:
                                //判定职业是否符合
                                
                                //unit.GetComponent<SkillSetComponent>().OnAddSkillBook(SkillSourceEnum.Book, int.Parse(Item.SkillID));
                                break;
                            case 108:   //宠物经验骨头
                            case 109:   //宠物经验牛奶
                                break;
                            case 110:
                                //1;20;70010101,70010102@21;70;70020101,70020102
                                int createMonsterID = 0;
                                int lv = roleInfoComponentServer.RoleInfo.Lv;
                                string[] monsters = null;//ldItem.ItemUsePar;
                                if (monsters.Length > 100)
                                {
                                  //  Log.Error($"monsters.Length > 100:  {ldItem.ItemUsePar}");
                                    reply();
                                    return;
                                }
                                for (int c = 0; c < monsters.Length; c++)
                                {
                                    //1;20;70010101,70010102
                                    string[] lelveparams = monsters[c].Split(";");
                                    int level_1 = int.Parse(lelveparams[0]);
                                    int level_2 = int.Parse(lelveparams[1]);
                                    if (lv < level_1 || lv > level_2)
                                    {
                                        continue;
                                    }
                                    string[] ids = lelveparams[2].Split(',');
                                    int r_number = RandomHelper.RandomNumber(0, ids.Length);
                                    Vector3 vector3 = new Vector3(unit.Position.x + RandomHelper.RandFloat01() * 1, unit.Position.y, unit.Position.z + RandomHelper.RandFloat01() * 1);
                                    Unit monster = UnitFactory.CreateMonster(domainScene, int.Parse(ids[r_number]), vector3, new CreateMonsterInfo()
                                    {
                                        Camp = CampEnum.CampMonster1
                                    });

                                    createMonsterID = int.Parse(ids[r_number]);
                                }
                                //发送广播信息
                                if (createMonsterID != 0)
                                {
                                    LDMonster ldMonsterCof = LDMonsterCategory.Instance.Get(createMonsterID);
                                    ServerMessageHelper.SendServerMessage(DBHelper.GetChatServerId(unit),
                                        NoticeType.Notice, "玩家" + roleInfoComponentServer.RoleInfo.Name + "在宝藏之地召唤出领主怪物:<color=#FF75F0>" + ldMonsterCof.Name + "</color>").Coroutine();
                                }
                                break;
                            //金币袋子
                            case 111:
                             //   string[] jinbiInfos =  null;//ldItem.ItemUsePar;
                                int userLv = roleInfoComponentServer.RoleInfo.Lv;
                                LDExp ldExp = LDExpCategory.Instance.Get(userLv);
                                /*long addCoin = (int)RandomHelper.RandomNumberFloat(float.Parse(jinbiInfos[0]) * exp.Exp_Role, float.Parse(jinbiInfos[1]) * exp.Exp_Role);
                                addCoin *= costNumber;
                                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Gold, addCoin.ToString(), true, ItemGetWay.ItemBox_6);*/
                                break;
                            //经验木桩
                            case 112:
                                string[] expInfos =  null;//ldItem.ItemUsePar;
                                string[] operatePar = itemOperateParParts ?? request.OperatePar.Split(';'); //使用类型;数量
                                int needZuanshi = operatePar[0] == "1"? int.Parse(expInfos[0]) * costNumber : 0;
                                string[] paramInfo = expInfos[int.Parse(operatePar[0])].Split(';');
                                userLv = roleInfoComponentServer.RoleInfo.Lv;

                                /*exp = ExpCategory.Instance.Get(userLv);
                                int addExp = (int)RandomHelper.RandomNumberFloat(float.Parse(paramInfo[0]) * exp.RoseExpPro, float.Parse(paramInfo[1]) * exp.RoseExpPro);
                                addExp *= costNumber;   
                                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Exp, addExp.ToString(), true, ItemGetWay.DuiHuan);
                                if (needZuanshi > 0)
                                {
                                    roleInfoComponentServer.UpdateRoleMoneySub(UserDataType.Diamond, (needZuanshi * -1).ToString(), true, ItemGetWay.DuiHuan);
                                }*/

                                //response.OperatePar = addExp.ToString();
                                break;
                            //藏宝图
                            case 113:
                                int dropid = int.Parse(useBagInfo.ItemPar.Split('@')[2]);
                                UnitFactory.CreateDropItems(unit, unit, 0, dropid, request.OperatePar);
                                break;
                            case 114: //宝石
                                break;
                            case 115://宠物皮肤激活道具
                               // unit.GetComponent<PetComponentServer>().OnUnlockSkin(ldItem.ItemUsePar);
                                break;
                            case 116:   //角色洗点
                                unit.GetComponent<PlayerSessionComponent>()?.OnResetPoint();
                                break;
                            case 117:   //宠物洗点
                            case 118:   //宠物资质
                            case 119:   //宠物成长
                                break;
                            case 120://120 冒险积分
                            //    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.MaoXianExp, int.Parse(ldItem.ItemUsePar), 0);
                                break;
                            case 121: //鉴定符
                                break;
                            case 122:   //宠物技能书
                                break;
                            case 123:   //宠物扩展工具
                                numericComponent.ApplyChange(null, NumericType.PetExtendNumber, 1, 0);
                                break;
                            case 124: //仓库扩展工具
                                int cangkuNumber = numericComponent.GetAsInt(NumericType.CangKuNumber);
                                numericComponent.ApplyValue(NumericType.CangKuNumber, cangkuNumber + 1);
                                break;
                            case 125://坐骑获取
                                
                                Function_Fight.UnitUpdateProperty_Base( unit, true, true );
                                break;
                            case 126: //集字
                                break;
                            case 127: //藏宝图
                                string rewardItem = useBagInfo.ItemPar.Split('@')[2];
                                bagComponentServer.OnAddItemData(rewardItem, $"{ItemGetWay.TreasureMap}_{TimeHelper.ServerNow()}");
                                unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.TreasureMapNumber_210, 0, 1);

                                //普通
                                if (ldItem.Quality == 4)
                                {
                                    unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.TreasureMapNormal_26, 0, 1);
                                }
                                if (ldItem.Quality == 5)
                                {
                                    unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.TreasureMapHigh_27, 0, 1);
                                }

                                break;
                            case 128://激活称号
                              //  unit.GetComponent<TitleComponentServer>().OnActiveTile(int.Parse(ldItem.ItemUsePar));
                                break;
                            case 129://激活精灵
                              //  unit.GetComponent<ChengJiuComponentServer>().OnActiveJingLing(int.Parse(ldItem.ItemUsePar));
                                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                                break;
                            case 131://增加饱食度
                                string[] baoshipas = null;//ldItem.ItemUsePar;
                                int baoshiadd = RandomHelper.RandomNumber(int.Parse(baoshipas[0]), int.Parse(baoshipas[1]) + 1);
                                roleInfoComponentServer.UpdateRoleData(UserDataType.BaoShiDu, baoshiadd.ToString());
                                break;
                            case 132:
                                long reduceTime = 0;// long.Parse(ldItem.ItemUsePar);
                                numericComponent.ApplyChange(null, NumericType.SeasonBossRefreshTime, -1 * reduceTime, 0);
                                break;
                            case 133:
                            case 134:
                                break;
                            case 135:
                                C2M_SkillCmd cmd = new C2M_SkillCmd();
                                cmd.SkillID = 0;// int.Parse(ldItem.ItemUsePar);
                                cmd.TargetID = unit.Id;
                                cmd.TargetAngle = (int)Quaternion.QuaternionToEuler(unit.Rotation).y;
                                cmd.TargetDistance = 0f;
                                unit.GetComponent<SkillManagerComponent>().OnUseSkill(cmd);
                                break;
                            case 136:
                                break;
                            case 137:
                                //宠物蛋附灵
                                long chongwudanId = long.Parse(request.OperatePar);
                                BagInfo chongwudan = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, chongwudanId);
                                m2c_bagUpdate.BagInfoUpdate.Add(chongwudan);
                                break;
                            case 138:
                                // 增加副本次数
                                numericComponent.ApplyValue(NumericType.TeamDungeonTimes, unit.GetTeamDungeonTimes() - 1);
                                break;
                            case 139:
                                //增加背包格子
                                bagComponentServer.AdditionalCellNum[0]++;
                                break;
                            case 140:
                                bagComponentServer.AdditionalCellNum[5]++;
                                bagComponentServer.AdditionalCellNum[6]++;
                                bagComponentServer.AdditionalCellNum[7]++;
                                bagComponentServer.AdditionalCellNum[8]++;
                                //增加仓库格子
                                break;
                            case 141:  //转职道具
                                break;
                            case 142:
                                //封印的武器
                                break;
                            case 143:
                                //钻石抽奖券
                                break;
                            default:
                                break;
                        }

                        //扣除道具
                        if (costItemStatus)
                        {
                            if (useBagInfo.ItemNum <= 0)
                            {
                                m2c_bagUpdate.BagInfoDelete.Add(useBagInfo);
                            }
                            else
                            {
                                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                            }
                        }
                        if (ldItem.DayUseNum > 0)
                        {
                            daily?.OnDayItemUse(ldItem.Id);
                        }
                        if (ldItem.SumUseNum > 0)
                        {
                            roleInfoComponentServer.OnTotalUseTimes(ldItem.Id);
                        }
                    }
                }

                //出售道具
                if (request.OperateType == 2 && locType == ItemLocType.ItemLocBag)
                {
                    Log.Error("request.OperateType == 222");
                }

                //穿戴装备
                if (request.OperateType == 3)
                {
                    Log.Error("request.OperateType == 3");
                }

                //卸下装备
                if (request.OperateType == 4)
                {
                    Log.Error("request.OperateType == 4");
                }

                //鉴定装备
                if (request.OperateType == 5)
                {
                  
                }

                //放入仓库
                if (request.OperateType == 6)
                {
                    int hourseId = int.Parse(request.OperatePar);
                    if (bagComponentServer.IsBagFullByLoc(hourseId))
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;     //错误码:仓库已满
                        reply();
                        return;
                    }
                    if (useBagInfo.Loc != (int)ItemLocType.ItemLocBag)
                    {
                        Log.Error($"C2M_ItemOperateHandler 5");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    bagComponentServer.OnChangeItemLoc(useBagInfo, (ItemLocType)hourseId, ItemLocType.ItemLocBag);

                    m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }

                //放回背包
                if (request.OperateType == 7)
                {
                    int hourseId = useBagInfo.Loc;
                    if (bagComponentServer.IsBagFullByLoc((int)ItemLocType.ItemLocBag))
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;     //错误码:仓库已满
                        reply();
                        return;
                    }
                    if (useBagInfo.Loc != hourseId)
                    {
                        Log.Error($"C2M_ItemOperateHandler 6");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    bagComponentServer.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, (ItemLocType)hourseId);
                    unit.GetComponent<TaskComponentServer>().OnGetItemForWarehouse(useBagInfo.ItemID);
                    m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }

             
                if (unit.IsRobot())
                {
                    DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, bagComponentServer).Coroutine();
                }

                MessageHelper.SendToClient(unit, m2c_bagUpdate);
                //通知客户端属性刷新
                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }
    }
}
