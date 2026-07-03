using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class HeroDataComponentAwakeSystem : AwakeSystem<HeroDataComponent>
    {
        public override void Awake(HeroDataComponent self)
        {

        }
    }

    /// <summary>
    /// 英雄数据组件，负责管理英雄数据
    /// </summary>
    public static class HeroDataComponentSystem
    {

        public static void CheckNumeric(this HeroDataComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            //重置所有属性
            long max = (int)NumericType.Max;
            foreach (int key in numericComponent.NumericDic.Keys)
            {
                //这个范围内的属性为特殊属性不进行重置
                if (key < max)
                {
                    continue;
                }
            }

          
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            int PointLiLiang = numericComponent.GetAsInt(NumericType.Point_Strength);
            int PointZhiLi = numericComponent.GetAsInt(NumericType.Point_Intelligence);
            int PointTiZhi = numericComponent.GetAsInt(NumericType.Point_Constitution);
            int PointNaiLi = numericComponent.GetAsInt(NumericType.Point_Stamina);
            int PointMinJie = numericComponent.GetAsInt(NumericType.Point_Agility);
            int PointRemain = numericComponent.GetAsInt(NumericType.PointRemain);
            int totalPoint = RoleAddPointHelper.GetTotalPointAtLevel(roleInfoComponentServer.RoleInfo.Lv);
            
            //检测属性点
            if (unit.IsRobot())
            {
                //机器人属性点
            }
            else
            {
              
            }
        }

        public static void SendSeasonReward(this HeroDataComponent self, int seasonLevel)
        {
            string rewardItem = SeasonHelper.GetSeasonOverReward(seasonLevel);
            if (string.IsNullOrEmpty(rewardItem))
            {
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.Context = "赛季结束奖励";
            mailInfo.Title = "赛季结束奖励";
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            mailInfo.ItemList.AddRange(ItemHelper.GetRewardItems_2(rewardItem));
            MailHelp.SendUserMail( self.DomainZone(), self.Id, mailInfo ).Coroutine();
        }

        public static void OnLogin(this HeroDataComponent self, int robotId)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.Set((int)NumericType.Now_Dead , 0, false);
            numericComponent.Set((int)NumericType.Now_Damage, 0, false);
            numericComponent.Set((int)NumericType.Now_Stall, 0, false);
            numericComponent.Set((int)NumericType.TeamId, 0, false);
            numericComponent.Set((int)NumericType.HP_Current_8, numericComponent.GetAsLong((int)NumericType.HP_Max_10), false);
            numericComponent.Set((int)NumericType.Now_Weapon, unit.GetComponent<BagComponentServer>().GetWuqiItemId(), false);
            numericComponent.Set(NumericType.JueXingAnger, 0, false);
            numericComponent.Set(NumericType.RunRaceRankId, 0, false);
            numericComponent.Set(NumericType.ZeroClock, 0, false);
            
            if (numericComponent.UpdateNumber < 1)
            {
                numericComponent.UpdateNumber = 1;
                numericComponent.ApplyValue(NumericType.PetExploreLuckly, 100, false);
            }

            if (unit.Type == UnitType.Player && numericComponent.GetAsInt(NumericType.PetExtendNumber) > 0)
            {
                if (unit.GetComponent<RoleInfoComponentServer>().GetTotalUseTimes(10000134) <= 0)
                {
                    unit.GetComponent<RoleInfoComponentServer>().OnTotalUseTimes(10000134, numericComponent.GetAsInt(NumericType.PetExtendNumber));
                }
            }
           
            if (numericComponent.GetAsInt(NumericType.SkillMakePlan2) == 0)
            {
                numericComponent.ApplyValue(NumericType.MakeType_2, 0, false);
            }

            //月卡次数用完，则清空标志
            int yuekatimes = numericComponent.GetAsInt(NumericType.YueKaRemainTimes);
            if (yuekatimes > 0)
            {
                numericComponent.ApplyValue(NumericType.YueKaEndTime, yuekatimes, false);
            }

            self.CheckSeasonOver(false);
            self.CheckSeasonOpen(false);
        }

        public static void CheckSeasonOver(this HeroDataComponent self, bool notice)
        {
            ///赛季数据[赛季开始]
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            long seasonopenTime = numericComponent.GetAsLong(NumericType.SeasonOpenTime);
            KeyValuePairLong keyValuePairLong = SeasonHelper.GetOpenSeason(roleInfoComponentServer.RoleInfo.Lv);

            if (seasonopenTime != 0 &&  (keyValuePairLong== null  || seasonopenTime != keyValuePairLong.Value) )
            {
                //清空赛季相关数据. 赛季任务 晶核
                Log.Warning($"清空赛季数据！:{unit.Id}");
                Console.WriteLine($"清空赛季数据！: {unit.DomainZone()}  {unit.Id}  {seasonopenTime} ");
                self.SendSeasonReward(unit.GetComponent<RoleInfoComponentServer>().RoleInfo.SeasonLevel);

                numericComponent.ApplyValue(NumericType.SeasonOpenTime, 0, notice);
                numericComponent.ApplyValue(NumericType.SeasonReward, 0, notice);
                numericComponent.ApplyValue(NumericType.SeasonBossFuben, 0, notice);
                numericComponent.ApplyValue(NumericType.SeasonBossRefreshTime, 0, notice);
                numericComponent.ApplyValue(NumericType.SeasonTowerId, 0, notice);
                //numericComponent.ApplyValue(NumericType.SeasonTask, 0, notice);

                unit.GetComponent<RoleInfoComponentServer>().OnResetSeason(notice);
                unit.GetComponent<BagComponentServer>().OnResetSeason(notice);
                unit.GetComponent<TaskComponentServer>().OnResetSeason(notice);
            }
        }

        public static void CheckSeasonOpen(this HeroDataComponent self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            if (numericComponent.GetAsInt(NumericType.SeasonBossFuben) >= 100000)
            {
                numericComponent.ApplyValue(NumericType.SeasonBossFuben, SeasonHelper.GetFubenId(roleInfoComponentServer.RoleInfo.Lv));
            }

            if (numericComponent.GetAsInt(NumericType.SeasonBossFuben) >= CommonConfig.GMDungeonId)
            {
                Console.WriteLine($"赛季boss地图：  {numericComponent.GetAsInt(NumericType.SeasonBossFuben)}");
                numericComponent.ApplyValue(NumericType.SeasonBossFuben, SeasonHelper.GetFubenId(roleInfoComponentServer.RoleInfo.Lv));
            }

            KeyValuePairLong seasonOpenTime = SeasonHelper.GetOpenSeason(roleInfoComponentServer.RoleInfo.Lv);
            if (numericComponent.GetAsLong(NumericType.SeasonOpenTime) == 0 && seasonOpenTime != null)
            {
                //Console.WriteLine($"赛季开启: {unit.DomainZone()}  {unit.Id}  {seasonOpenTime.KeyId}");

                //刷新boss
                numericComponent.ApplyValue(NumericType.SeasonBossFuben, SeasonHelper.GetFubenId(roleInfoComponentServer.RoleInfo.Lv), notice);
                numericComponent.ApplyValue(NumericType.SeasonBossRefreshTime, TimeHelper.ServerNow() + TimeHelper.Minute, notice);
                numericComponent.ApplyValue(NumericType.SeasonOpenTime, seasonOpenTime.Value, notice);

                //刷新任务
                TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
                taskComponentServer.InitSeasonMainTask(notice);
                taskComponentServer.UpdateSeasonWeekTask(notice); 
            }
        }

        public static void ActivityV1Reset(this HeroDataComponent self,bool kouchu, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.ApplyValue(NumericType.V1DayCostDiamond, 0, notice);

            //每次活动扣除100积分， 对话任意积分可免扣除
            float v1points =unit.GetComponent<RoleInfoComponentServer>().RoleInfo.V1TotalPoints;  
            if (kouchu)
            {
                v1points = Math.Min(100f, v1points);
                unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.V1TotalPoints, (v1points * -1).ToString());
            }
        }

        /// <summary>
        /// 重置。隔天登录或者零点刷新
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notice"></param>
        public static void OnZeroClockUpdate(this HeroDataComponent self, bool notice = false)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.ApplyValue(NumericType.HongBao, 0, notice);
            numericComponent.ApplyValue(NumericType.Now_XiLian, 0, notice);
            numericComponent.ApplyValue(NumericType.PetChouKa, 0, notice);
            numericComponent.ApplyValue(NumericType.YueKaAward, 0, notice);
            numericComponent.ApplyValue(NumericType.XiuLian_ExpNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.XiuLian_CoinNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.XiuLian_ExpTime, 0, notice);
            numericComponent.ApplyValue(NumericType.XiuLian_CoinTime, 0, notice);
            numericComponent.ApplyValue(NumericType.TiLiKillNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.ChouKa, 0, notice);
            numericComponent.ApplyValue(NumericType.ExpToGoldTimes, 0, notice);
            numericComponent.ApplyValue(NumericType.RechargeSign, 0, notice);
            numericComponent.ApplyValue(NumericType.TeamDungeonTimes, 0, notice);
            numericComponent.ApplyValue(NumericType.TeamDungeonXieZhu, 0, notice);
            numericComponent.ApplyValue(NumericType.BattleTodayKill, 0, notice);
            numericComponent.ApplyValue(NumericType.FubenTimesReset, 0, notice);
            numericComponent.ApplyValue(NumericType.FenShangSet, 0, notice);
            numericComponent.ApplyValue(NumericType.ArenaNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.LocalDungeonTime, 0, notice);
            numericComponent.ApplyValue(NumericType.TreasureTask, 0, notice);
            numericComponent.ApplyValue(NumericType.JiaYuanExchangeZiJin, 0, notice);
            numericComponent.ApplyValue(NumericType.JiaYuanExchangeExp, 0, notice);
            numericComponent.ApplyValue(NumericType.JiaYuanVisitRefresh, 0, notice);
            numericComponent.ApplyValue(NumericType.JiaYuanGatherOther, 0, notice);
            numericComponent.ApplyValue(NumericType.JiaYuanPickOther, 0, notice);
            numericComponent.ApplyValue(NumericType.UnionDonationNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.UnionDiamondDonationNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.RaceDonationNumber, 0, notice);
            // 重置封印之塔数据
            numericComponent.ApplyValue(NumericType.JiaYuanPurchaseRefresh, 0, notice);
            numericComponent.ApplyValue(NumericType.TowerOfSealArrived, 0, notice);
            numericComponent.ApplyValue(NumericType.TowerOfSealFinished, 0, notice);

            numericComponent.ApplyValue(NumericType.RunRaceRankId, 0, notice);
            numericComponent.ApplyValue(NumericType.HappyCellIndex, 0, notice);
            numericComponent.ApplyValue(NumericType.HappyMoveNumber, 0, notice);

            numericComponent.ApplyValue(NumericType.PetMineBattle, 0, notice);
            numericComponent.ApplyValue(NumericType.PetMineLogin, 0, notice);

            numericComponent.ApplyValue(NumericType.CostTiLi, 0, notice);
            numericComponent.ApplyValue(NumericType.DrawIndex, 0, notice);
            numericComponent.ApplyValue(NumericType.DrawReward, 0, notice);

            numericComponent.ApplyValue(NumericType.PetMineReset, 0, notice);
            numericComponent.ApplyValue(NumericType.V1ChouKaNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.V1RechageNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.PetExploreNumber, 0, notice);
            numericComponent.ApplyValue(NumericType.PetHeXinExploreNumber, 0, notice);
            //numericComponent.ApplyValue(NumericType.ItemXiLianNumber, 0, notice);

            //月卡次数用完，则清空标志
            int yuekatimes = numericComponent.GetAsInt(NumericType.YueKaRemainTimes);
            numericComponent.ApplyValue(NumericType.YueKaEndTime, yuekatimes, notice);

            int lirun =  (int)(numericComponent.GetAsInt(NumericType.InvestTotal) * 0.25f);
            numericComponent.ApplyValue(NumericType.InvestTotal, numericComponent.GetAsInt(NumericType.InvestTotal) + lirun, notice);

            self.CheckSeasonOver(notice);
            self.CheckSeasonOpen(notice);   
        }

        /// <summary>
        /// 返回主城
        /// </summary>
        /// <param name="self"></param>
        public static void OnReturn(this HeroDataComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.SetValueNoSync(NumericType.Now_Dead, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Damage,0);
            numericComponent.SetValueNoSync(NumericType.BossBelongID, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_HP, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_MaxHP, 0);
            numericComponent.SetValueNoSync(NumericType.Now_Shield_DamgeCostPro,0);
            if (unit.GetComponent<NumericComponent>().GetAsLong(NumericType.Now_Dead) <= 0)
            {
                long max_hp = self.Parent.GetComponent<NumericComponent>().GetAsLong(NumericType.HP_Max_10);
                unit.GetComponent<NumericComponent>().SetValueNoSync(NumericType.HP_Current_8, max_hp);
            }
        }

        public static void OnResetPoint(this HeroDataComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (!RoleAddPointHelper.CanResetPoint(roleInfoComponentServer.RoleInfo.Lv))
            {
                return;
            }

            RoleAddPointHelper.RecalculateAllPoints(unit);
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
        }

        /// <summary>
        /// 0 不复活 1等待复活
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int OnWaitRevive(this HeroDataComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Monster)
            {
                return 0;
            }

            LDMonster ldMonster = LDMonsterCategory.Instance.Get(unit.ConfigId);
            int resurrection = 0 ;///(int)ldMonster.ReviveTime;
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            if (SeasonHelper.SeasonBossId == unit.ConfigId && mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
            {
                LocalDungeonComponent localDungeon = unit.DomainScene().GetComponent<LocalDungeonComponent>();
                RoleInfoComponentServer roleInfoComponentServer = localDungeon.MainUnit.GetComponent<RoleInfoComponentServer>();
                localDungeon.MainUnit.GetComponent<NumericComponent>().ApplyValue(NumericType.SeasonBossFuben, SeasonHelper.GetFubenId(roleInfoComponentServer.RoleInfo.Lv));
                localDungeon.MainUnit.GetComponent<NumericComponent>().ApplyValue(NumericType.SeasonBossRefreshTime, TimeHelper.ServerNow() + resurrection * 1000);
                resurrection = 0;
            }
            if (resurrection == 0)
            {
                return 0;
            }

            if (unit.MasterId > 0)
            {
                return 0;
            }
            return 0;
        }

        public static void OnKillZhaoHuan(this HeroDataComponent self, Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            UnitInfoComponent unitInfoComponent = unit.GetComponent<UnitInfoComponent>();
            if (unitInfoComponent == null)
            {
                Log.Debug($"unitInfoComponent == null  {unit.Type } {unit.IsDisposed}");
                return;
            }
            for (int i = unitInfoComponent.ZhaohuanIds.Count - 1; i >= 0; i--)
            {
                Unit zhaohuan = unit.GetParent<UnitComponent>().Get(unitInfoComponent.ZhaohuanIds[i]);
                if (zhaohuan == null)
                {
                    continue;
                }
                
                zhaohuan.GetComponent<HeroDataComponent>().OnDead(attack!=null ? attack : zhaohuan);
            }
            unitInfoComponent.ZhaohuanIds.Clear();
        }

        public static void PlayDeathSkill(this HeroDataComponent self,Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type == UnitType.Monster)
            {
                if (unit.ConfigId == 90000202)   //90030005
                {
                    Log.Warning("PlayDeathSkill: 72009045");
                }

                LDMonster ldMonster = LDMonsterCategory.Instance.Get(unit.ConfigId);
            }
        }

        public static void OnRevive(this HeroDataComponent self, bool bornPostion = false)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent  = unit.GetComponent<NumericComponent>();
            long max_hp = numericComponent.GetAsLong(NumericType.HP_Max_10);

            numericComponent.ApplyValue(NumericType.Now_Dead, 0);
            numericComponent.SetValueNoSync(NumericType.HP_Current_8, 0);
            numericComponent.ApplyChange(null, NumericType.HP_Current_8, max_hp, 0);
            numericComponent.ApplyValue(NumericType.ReviveTime, 0);
            unit.GetComponent<SkillPassiveComponent>()?.Activeted();
            unit.GetComponent<BuffManagerComponent>()?.OnRevive();
            unit.Position = unit.GetBornPostion();
            if (unit.Type == UnitType.Monster)
            {
                unit.GetComponent<AIComponent>().Begin();
            }
        }

        public static void InitTempFollower(this HeroDataComponent self, Unit matster, int monster)
        {
            Unit nowUnit = self.GetParent<Unit>();
            NumericComponent numericComponent = nowUnit.GetComponent<NumericComponent>();
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(monster);


            NumericComponent numericComponentMaster = matster.GetComponent<NumericComponent>();
          
            numericComponent.Set((int)NumericType.Speed_Current_15, 5f, false);
            //设置当前血量
            numericComponent.SetValueNoSync(NumericType.HP_Current_8, numericComponent.GetAsInt(NumericType.HP_Current_8));
        }

        public static void InitJiaYuanPet(this HeroDataComponent self,  bool notice)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            numericComponent.Set(NumericType.HP_Max_10, 1, notice);
            numericComponent.Set(NumericType.HP_Current_8, 1, notice);
        }

        public static void InitPet(this HeroDataComponent self, RolePetInfo rolePetInfo, bool notice)
        {
            Unit unit = self.GetParent<Unit>();

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            for (int i = 0; i < rolePetInfo.Ks.Count; i++)
            {
                numericComponent.Set(rolePetInfo.Ks[i], rolePetInfo.Vs[i], notice);
            }
        }

        public static void InitPlan(this HeroDataComponent self, JiaYuanPlant jiaYuanPlant, bool notice)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            numericComponent.Set(NumericType.StartTime, jiaYuanPlant.StartTime);
            numericComponent.Set(NumericType.GatherNumber, jiaYuanPlant.GatherNumber);
            numericComponent.Set(NumericType.GatherLastTime, jiaYuanPlant.GatherLastTime);
            numericComponent.Set(NumericType.GatherCellIndex, jiaYuanPlant.CellIndex);
        }

        public static void InitPasture(this HeroDataComponent self, JiaYuanPastures jiaYuanPlant, bool notice)
        {
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            numericComponent.Set(NumericType.StartTime, jiaYuanPlant.StartTime);
            numericComponent.Set(NumericType.GatherNumber, jiaYuanPlant.GatherNumber);
            numericComponent.Set(NumericType.GatherLastTime, jiaYuanPlant.GatherLastTime);
        }

        public static void InitJingLing(this HeroDataComponent self, Unit master, int jinglingid, bool notice)
        {
            NumericComponent masterNumericComponent = master.GetComponent<NumericComponent>();

            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();
            foreach ((int ntype, long value) in masterNumericComponent.NumericDic)
            {
                if (ntype == NumericType.RechargeNumber || ntype == NumericType.MaoXianExp)
                {
                    continue;
                }

                numericComponent.Set(ntype, value, false);
            }
        }

        /// <summary>
        /// 角色属性模块初始化
        /// </summary>
        public static void InitMonsterInfo_Summon2(this HeroDataComponent self, LDMonster ldMonster, CreateMonsterInfo createMonsterInfo)
        {
            Unit nowUnit = self.GetParent<Unit>();
            NumericComponent numericComponent = nowUnit.GetComponent<NumericComponent>();

            int monsterlevel = 1;
            Unit masterUnit = nowUnit.GetParent<UnitComponent>().Get(createMonsterInfo.MasterID);
            if (masterUnit.Type == UnitType.Player)
            {
                monsterlevel = masterUnit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
            }
            else
            {
                monsterlevel = ldMonster.Lv;
            }

            //0.8,0.8,0.5,0.5;5000,0,0,0,0
            //血量比例,攻击比例,魔法比例,物防比例，魔防比例；血量固定值,攻击固定值，魔法固定值，物防固定值，魔防固定值
            string[] summonInfo = createMonsterInfo.AttributeParams.Split(';');

            //1复刻玩家形象
            int useMasterModel = int.Parse(summonInfo[0]);
            
            if (useMasterModel == 1)
            {
                UnitInfoComponent unitInfoComponent = nowUnit.GetComponent<UnitInfoComponent>();
                unitInfoComponent.FashionEquipList = masterUnit.GetComponent<BagComponentServer>().FashionEquipList;
                numericComponent.Set((int)NumericType.UseMasterModel, masterUnit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ, false);
            }

            string[] attributeList_1 = summonInfo[1].Split(',');    //比列
            string[] attributeList_2 = summonInfo[2].Split(',');    //固定值

            NumericComponent masterNumberComponent = masterUnit.GetComponent<NumericComponent>();
            numericComponent.Set((int)NumericType.Now_Weapon, masterNumberComponent.GetAsInt(NumericType.Now_Weapon), false);


            //属性
            numericComponent.Set((int)NumericType.Speed_Current_15, 5f, false);
            numericComponent.Set((int)NumericType.HP_Max_10, 1, false);
          
            //设置当前血量
            numericComponent.Set((int)NumericType.HP_Current_8, numericComponent.GetAsInt(NumericType.HP_Max_10));
            //Log.Debug("初始化当前怪物血量:" + numericComponent.GetAsLong(NumericType.Numeric_Error));
        }

        /// <summary>
        /// 角色属性模块初始化
        /// </summary>
        public static void InitMonsterInfo(this HeroDataComponent self, LDMonster ldMonster, CreateMonsterInfo createMonsterInfo)
        {
            Unit nowUnit = self.GetParent<Unit>();
            NumericComponent numericComponent = nowUnit.GetComponent<NumericComponent>();
            
            //根据副本难度刷新属性
            //进入 挑战关卡 怪物血量增加 1.5 伤害增加 1.2 低于关卡 血量增加2 伤害增加 1.5
            MapComponent mapComponent = nowUnit.DomainScene().GetComponent<MapComponent>();
            int sceneType = mapComponent.MapTypeEnum;
            int fubenDifficulty = FubenDifficulty.None;

            if (sceneType == MapTypeEnum.LocalDungeon )
            {
                fubenDifficulty = nowUnit.DomainScene().GetComponent<LocalDungeonComponent>().FubenDifficulty;
            }
            if (sceneType == MapTypeEnum.TeamDungeon)
            {
                //副本的怪物难度提升（类似不难度的个人副本 给个配置即可）
                int realplayerNumber = nowUnit.DomainScene().GetComponent<TeamDungeonComponent>().InitPlayerNumber();
                fubenDifficulty = mapComponent.FubenDifficulty;
            }

            //Log.Debug("初始化当前怪物血量:" + numericComponent.GetAsLong(NumericType.Numeric_Error));
        }

        /// <summary>
        /// 更新当前角色身上的buff信息, 更新基础属性
        /// </summary>
        public static void BuffPropertyUpdate_Long(this HeroDataComponent self, int numericType, long NumericTypeValue)
        {
            if (numericType== NumericType.RechargeBuChang || numericType == NumericType.RechargeNumber)
            {
                Log.Error($"BuffPropertyUpdate_Long: {self.DomainZone()}  {self.Id}");
            }

            Unit nowUnit = self.GetParent<Unit>();
            NumericComponent numericComponent = nowUnit.GetComponent<NumericComponent>();
            long newvalue = numericComponent.GetAsLong(numericType) + NumericTypeValue;
            numericComponent.Set(numericType, newvalue);
        }

        public static void BuffPropertyUpdate_Float(this HeroDataComponent self, int numericType, float NumericTypeValue)
        {
            if (numericType == NumericType.RechargeBuChang || numericType == NumericType.RechargeNumber)
            {
                Log.Error($"BuffPropertyUpdate_Long: {self.DomainZone()}  {self.Id}");
            }
            Unit nowUnit = self.GetParent<Unit>();
            NumericComponent numericComponent = nowUnit.GetComponent<NumericComponent>();
            float newvalue = numericComponent.GetAsFloat(numericType) + NumericTypeValue;
            numericComponent.Set(numericType, newvalue);
        }


        public static void OnDead(this HeroDataComponent self, Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            unit.GetComponent<MoveComponent>()?.Stop();
            //{
            //    unit.Stop(-1);
            //}

            unit.GetComponent<AIComponent>()?.Stop();
            unit.GetComponent<SkillPassiveComponent>()?.Stop();
            unit.GetComponent<SkillManagerComponent>()?.OnFinish(false);
            unit.GetComponent<BuffManagerComponent>()?.OnDead(attack);
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (unit.Type == UnitType.Player)
            {
                RolePetInfo rolePetInfo = unit.GetComponent<PetComponentServer>().GetFightPet();
                if (rolePetInfo != null)
                {
                    unit.GetParent<UnitComponent>().Remove(rolePetInfo.Id);
                    unit.GetComponent<PetComponentServer>().OnPetDead(rolePetInfo.Id);
                }

                int now_horse = numericComponent.GetAsInt(NumericType.HorseRide);
                if (now_horse > 0)
                {
                    numericComponent.ApplyValue(NumericType.HorseRide, 0);
                }
            }
            //玩家死亡，怪物技能清空
            if (unit.Type == UnitType.Player && attack != null && attack.Type == UnitType.Monster)
            {
                Unit nearest = AIGetTargetHelp.GetNearestEnemy(attack, attack.GetComponent<AIComponent>().ActRange);
                if (nearest == null)
                {
                    attack.GetComponent<AIComponent>().ChangeTarget(0);
                    attack.GetComponent<SkillManagerComponent>().OnFinish(true);
                }
                List<Unit> units = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Monster);
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].GetComponent<AttackRecordComponent>()?.OnRemoveAttackByUnit(unit.Id);
                }
            }
            if (unit.Type == UnitType.Pet)
            {
                int sceneTypeEnum = unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
                if (sceneTypeEnum != (int)MapTypeEnum.PetTianTi
                 && sceneTypeEnum != (int)MapTypeEnum.PetDungeon
                 && sceneTypeEnum != (int)MapTypeEnum.PetMing)
                {
                    long manster = numericComponent.GetAsLong(NumericType.MasterId);
                    Unit unit_manster = unit.GetParent<UnitComponent>().Get(manster);
                    //修改宠物出战状态
                    unit_manster.GetComponent<PetComponentServer>().OnPetDead(unit.Id);
                }
            }
            
            //怪物死亡， 清除玩家BUFF
            if (unit.Type == UnitType.Monster)  /// && LDMonsterCategory.Instance.Get(unit.ConfigId).RemoveBuff == 0)
            {
                List<Unit> units = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player);
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].GetComponent<BuffManagerComponent>().OnDeadRemoveBuffBy(unit.Id);
                }
            }
            int waitRevive = self.OnWaitRevive();
            numericComponent.ApplyValue(NumericType.Now_Dead, 1);
            Game.EventSystem.Publish(new EventType.KillEvent()
            {
                WaitRevive = waitRevive,
                UnitAttack = attack,
                UnitDefend = unit,
            });
        }


    }
}