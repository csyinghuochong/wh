using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class ChengJiuComponentAwakeSystem : AwakeSystem<ChengJiuComponentServer>
    {
        public override void Awake(ChengJiuComponentServer self)
        {
            self.RandomDrop = 0;
            self.ChengJiuEventBatchDepth = 0;
            self.ChengJiuEventCoalesceAdd?.Clear();
            self.ChengJiuEventCoalesceSet?.Clear();
            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            self.TriggerEvent(ChengJiuTargetEnum.PlayerLevel_205, 0, roleInfo.Lv);
        }
    }

    public static class ChengJiuComponentServerSystem
    {

        public static List<AttributeItem> GetJingLingProLists(this ChengJiuComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();
           
            for (int i = 0; i < self.JingLingList.Count; i++)
            {
                LDElf jinglingCof = LDElfCategory.Instance.Get(self.JingLingList[i]);
                //NumericHelp.GetProList(jinglingCof.AddProperty, proList);
            }

            if (self.JingLingId == 0)
            {
                return proList;
            }
            LDElf lifeShieldConfig = LDElfCategory.Instance.Get(self.JingLingId);
           // NumericHelp.GetProList(lifeShieldConfig.AddProperty, proList);
            //if (lifeShieldConfig.FunctionType == JingLingFunctionType.AddProperty)
            //{
            //    NumericHelp.GetProList(lifeShieldConfig.FunctionValue, proList);
            //}
            
            return proList;
        }

        public static void OnLogin(this ChengJiuComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            if (self.DomainZone() <= 48 && numericComponent.GetAsLong(NumericType.RechargeNumber) < 400 && self.JingLingList.Contains(10003))
            {
                Log.Warning($"充值小于400有精灵龟: {self.Id}");
                self.JingLingList.Remove(10003);
                self.JingLingId = 0;
                self.JingLingUnitId = 0;
            }

            self.TriggerEvent(ChengJiuTargetEnum.PlayerLevel_205, 0, roleInfo.RoleInfo.Lv);
        }

        public static void OnZeroClockUpdate(this ChengJiuComponentServer self)
        {
            self.RandomDrop = 0;
        }

        //击杀怪物可触发多种类型的成就
        public static void OnKillUnit(this ChengJiuComponentServer self, Unit defend)
        {
            if (defend == null || defend.IsDisposed)
                return;

            using (self.ChengJiuEventBatch())
            {
                if (defend.Type == UnitType.Player)
                {
                    Unit unit = self.GetParent<Unit>();
                    self.TriggerEvent(ChengJiuTargetEnum.KillPlayerNumber_209, 0, 1);
                    LogHelper.KillPlayerInfo(unit, defend);
                }
                if (defend.Type == UnitType.Monster)
                {
                    int unitconfigId = defend.ConfigId;
                    LDMonster ldMonster = LDMonsterCategory.Instance.Get(unitconfigId);
                    bool isBoss = ldMonster.Type == (int)MonsterTypeEnum.Boss;
                    Scene domainScene = self.DomainScene();
                    MapComponent mapComponent = domainScene.GetComponent<MapComponent>();
                    int fubenDifficulty = (int)FubenDifficulty.None;
                    if (mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
                    {
                        fubenDifficulty = (int)domainScene.GetComponent<LocalDungeonComponent>().FubenDifficulty;
                    }

                    self.TriggerEvent(ChengJiuTargetEnum.KillIDMonster_1, unitconfigId, 1);
                    self.TriggerEvent(ChengJiuTargetEnum.KillTotalMonster_2, 0, 1);

                    if (isBoss)
                    {
                        self.TriggerEvent(ChengJiuTargetEnum.KillTotalBoss_3, 0, 1);
                        self.TriggerEvent(ChengJiuTargetEnum.KillNormalBoss_4, unitconfigId, 1);
                    }
                    if (fubenDifficulty >= (int)FubenDifficulty.TiaoZhan && isBoss) //挑战
                    {
                        self.TriggerEvent(ChengJiuTargetEnum.KillChallengeBoss_5, unitconfigId, 1);
                    }
                    if (fubenDifficulty == (int)FubenDifficulty.DiYu && isBoss) //地狱
                    {
                        self.TriggerEvent(ChengJiuTargetEnum.KillInfernalBoss_6, unitconfigId, 1);
                    }
                }
            }
        }

        public static void OnPassFuben(this ChengJiuComponentServer self, int difficulty, int chapterid, int star)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.PassNormalFubenID_11, chapterid, 1);
                if ((int)difficulty >= (int)FubenDifficulty.TiaoZhan)  //挑战
                {
                    self.TriggerEvent(ChengJiuTargetEnum.PassChallengeFubenID_12, chapterid, 1);
                }
                if ((int)difficulty == (int)FubenDifficulty.DiYu)  //地狱
                {
                    self.TriggerEvent(ChengJiuTargetEnum.PassInfernalFubenID_13, chapterid, 1);
                }
                if (star == 3 && (int)difficulty == (int)FubenDifficulty.DiYu)
                {
                    self.TriggerEvent(ChengJiuTargetEnum.PerfectPassInfernalFubenID_14, chapterid, 1);
                }
            }
        }

        public static void OnChouKaTen(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.TotalChouKaTen_202, 0, 1);
        }

        public static void OnEquipXiLian(this ChengJiuComponentServer self, int times)
        {
            self.TriggerEvent(ChengJiuTargetEnum.TotalEquipXiLian_203, 0, times);
        }

        /// <summary>
        /// 洗练结果推进（隐藏技能等），Handler 只调此门面
        /// </summary>
        public static void OnEquipXiLianResults(this ChengJiuComponentServer self, List<ItemXiLianResult> results, int times)
        {
            using (self.ChengJiuEventBatch())
            {
                if (results != null)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        ItemXiLianResult itemXiLianResult = results[i];
                        for (int skill = 0; skill < itemXiLianResult.HideSkillLists.Count; skill++)
                        {
                            self.TriggerEvent(ChengJiuTargetEnum.EquipActiveSkillId_222, itemXiLianResult.HideSkillLists[skill], 1);
                        }
                    }
                }
                self.OnEquipXiLian(times);
            }
        }

        public static void OnMakeEquip(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.MakeNumber_216, 0, 1);
        }

        public static void OnJiaYuanLevel(this ChengJiuComponentServer self, int jiaYuanLv)
        {
            self.TriggerEvent(ChengJiuTargetEnum.JiaYuanLevel_404, 0, jiaYuanLv);
        }

        public static void OnCombatToValue(this ChengJiuComponentServer self, int combat)
        {
            self.TriggerEvent(ChengJiuTargetEnum.CombatToValue_211, 0, combat);
        }

        public static void OnPetTianTiRank(this ChengJiuComponentServer self, int rankId)
        {
            self.TriggerEvent(ChengJiuTargetEnum.PetTianTiRank_309, 0, rankId);
        }

        public static void OnTeamDungeonSettle(this ChengJiuComponentServer self, bool shenYuan)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.PassTeamFubenNumber_20, 0, 1);
                if (shenYuan)
                {
                    self.TriggerEvent(ChengJiuTargetEnum.PassTeamShenYuanNumber_21, 0, 1);
                }
            }
        }

        public static void OnRevive(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.TotalRevive_204, 0, 1);
        }

        public static void OnUpdateLevel(this ChengJiuComponentServer self, int lv)
        {
            self.TriggerEvent(ChengJiuTargetEnum.PlayerLevel_205, 0, lv);
        }

        public static void OnGetGold(this ChengJiuComponentServer self, int coin)
        {
            if (coin < 0)
            {
                self.TriggerEvent(ChengJiuTargetEnum.TotalCostGold_219, 0, coin * -1);
            }
            else
            {
                self.TriggerEvent(ChengJiuTargetEnum.TotalCoinGet_201, 0, coin);
            }
        }

        public static void OnGetPet(this ChengJiuComponentServer self, RolePetInfo rolePetInfo)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.PetIdNumber_301, rolePetInfo.ConfigId, 1);
                self.TriggerEvent(ChengJiuTargetEnum.TotalPetNumber_302, 0, 1);
                self.TriggerEvent(ChengJiuTargetEnum.PetNSkill_305, 0, rolePetInfo.PetSkill.Count);
            }
        }

        public static void OnPetPingFen(this ChengJiuComponentServer self, int maxPing, int arrayPing)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.PegScoreToValue_307, 0, maxPing);
                self.TriggerEvent(ChengJiuTargetEnum.PetArrayScoreToValue_308, 0, arrayPing);
            }
        }

        public static void OnPetMaxZiZhi(this ChengJiuComponentServer self, int hp, int act, int def, int adf, int mage)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.ZiZhiToValue_311, 1, hp);
                self.TriggerEvent(ChengJiuTargetEnum.ZiZhiToValue_311, 2, act);
                self.TriggerEvent(ChengJiuTargetEnum.ZiZhiToValue_311, 3, def);
                self.TriggerEvent(ChengJiuTargetEnum.ZiZhiToValue_311, 4, adf);
                self.TriggerEvent(ChengJiuTargetEnum.ZiZhiToValue_311, 5, mage);
            }
        }

        public static void OnPetHeCheng(this ChengJiuComponentServer self, RolePetInfo rolePetInfo)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.TotalPetHeCheng_303, 0, 1);
                self.TriggerEvent(ChengJiuTargetEnum.PetNSkill_305, 0, rolePetInfo.PetSkill.Count);
            }
        }

        public static void OnPetXiLian(this ChengJiuComponentServer self, RolePetInfo rolePetInfo)
        {
            using (self.ChengJiuEventBatch())
            {
                self.TriggerEvent(ChengJiuTargetEnum.TotalPetXiLian_304, 0, 1);
                self.TriggerEvent(ChengJiuTargetEnum.PetNSkill_305, 0, rolePetInfo.PetSkill.Count);
            }
        }

        public static void OnItemHuiShow(this ChengJiuComponentServer self, int itemNumber)
        {
            self.TriggerEvent(ChengJiuTargetEnum.TotalEquipHuiShou_206, 0, itemNumber);
        }

        public static void OnCostDiamond(this ChengJiuComponentServer self, long costNumber)
        {
            self.TriggerEvent(ChengJiuTargetEnum.TotalDiamondCost_207, 0, (int)(costNumber * -1));
        }

        public static void OnSkillShuLianDu(this ChengJiuComponentServer self, int shuLianDu)
        {
            self.TriggerEvent(ChengJiuTargetEnum.SkillShuLianDu_208, 0, shuLianDu);
        }

        public static void OnZodiacEquipNumber(this ChengJiuComponentServer self, int zodiacNumber)
        {
            self.TriggerEvent(ChengJiuTargetEnum.ZodiacEquipNumber_215, 0, zodiacNumber);
        }

        public static void OnFuMo(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.FoMoNumber_213, 0, 1);
        }

        public static void OnShare(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.ShareTotalNumber_220, 0, 1);
        }

        public static void OnPaiMaiSell(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.PaiMaiSellNumber_218, 0, 1);
        }

        public static void OnPaiMaiGetGold(this ChengJiuComponentServer self, int gold)
        {
            self.TriggerEvent(ChengJiuTargetEnum.PaiMaiGetGoldNumber_217, 0, gold);
        }

        public static void OnBattleUseItem(this ChengJiuComponentServer self)
        {
            self.TriggerEvent(ChengJiuTargetEnum.BattleUseItem_214, 0, 1);
        }

        public static int ReceivedReward(this ChengJiuComponentServer self, int rewardId)
        {
            return ErrorCode.ERR_ModifyData;
#if false // TODO: migrate to LD config
            if (self.AlreadReceivedId.Contains(rewardId))
            {
                return ErrorCode.ERR_Success;
            }

            ChengJiuRewardConfig chengJiuRewardConfig = ChengJiuRewardConfigCategory.Instance.Get(rewardId);
            bool success = self.GetParent<Unit>().GetComponent<BagComponentServer>().OnAddItemData(chengJiuRewardConfig.RewardItems, $"{ItemGetWay.ChengJiuRward}_{TimeHelper.ServerNow()}");
            if (success)
            {
                self.AlreadReceivedId.Add(rewardId);
                return ErrorCode.ERR_Success;
            }
            else
            {
                return ErrorCode.ERR_BagIsFull;
            }
#endif
        }

        public static void OnActiveJingLing(this ChengJiuComponentServer self, int jid)
        {
            if (self.JingLingList.Contains(jid))
            {
                return;
            }
            self.JingLingList.Add(jid);
        }

        public static void OnGmGaoJi(this ChengJiuComponentServer self)
        {
            self.ChengJiuProgessList.Clear();
            self.ChengJiuCompleteList.Clear();
#if false // TODO: migrate to LD config
            Dictionary<int, ChengJiuConfig> allchengjiu = ChengJiuConfigCategory.Instance.GetAll();
            foreach (var item in allchengjiu)
            {
                self.ChengJiuCompleteList.Add( item.Key );
            }
#endif

            self.JingLingList.Clear();  
            Dictionary<int, LDElf> alljingling = LDElfCategory.Instance.GetAll();
            foreach (var item in alljingling)
            {
                self.OnActiveJingLing(item.Key); 
            }
        }

        /// <summary>
        /// 成就事件批处理作用域，离开 using 自动 End/Flush。仅限 Component 内部 OnXxx 使用。
        /// </summary>
        public static ChengJiuEventBatchScope ChengJiuEventBatch(this ChengJiuComponentServer self)
        {
            return new ChengJiuEventBatchScope(self);
        }

        /// <summary>
        /// 开始成就事件批处理。优先用 ChengJiuEventBatch() + using。
        /// </summary>
        public static void BeginChengJiuEventBatch(this ChengJiuComponentServer self)
        {
            self.ChengJiuEventBatchDepth++;
        }

        /// <summary>
        /// 结束成就事件批处理（支持嵌套）。优先用 ChengJiuEventBatch() + using。
        /// </summary>
        public static void EndChengJiuEventBatch(this ChengJiuComponentServer self)
        {
            if (self.ChengJiuEventBatchDepth <= 0)
            {
                return;
            }
            self.ChengJiuEventBatchDepth--;
            if (self.ChengJiuEventBatchDepth == 0)
            {
                self.FlushChengJiuEventBatch();
            }
        }

        private static bool IsChengJiuSetValue(ChengJiuTargetEnum chengJiuTarget)
        {
            switch (chengJiuTarget)
            {
                case ChengJiuTargetEnum.PlayerLevel_205:
                case ChengJiuTargetEnum.SkillShuLianDu_208:
                case ChengJiuTargetEnum.CombatToValue_211:
                case ChengJiuTargetEnum.ZodiacEquipNumber_215:
                case ChengJiuTargetEnum.PetNSkill_305:
                case ChengJiuTargetEnum.PegScoreToValue_307:
                case ChengJiuTargetEnum.PetArrayScoreToValue_308:
                case ChengJiuTargetEnum.PetTianTiRank_309:
                case ChengJiuTargetEnum.ZiZhiToValue_311:
                case ChengJiuTargetEnum.ZiZhiUpValue_312:
                case ChengJiuTargetEnum.JiaYuanLevel_404:
                    return true;
                default:
                    return false;
            }
        }

        public static void TriggerEvent(this ChengJiuComponentServer self, ChengJiuTargetEnum chengJiuTarget, int target_id, int target_value=1)
        {
            if (self.ChengJiuEventBatchDepth > 0)
            {
                var key = ((int)chengJiuTarget, target_id);
                if (IsChengJiuSetValue(chengJiuTarget))
                {
                    self.ChengJiuEventCoalesceSet[key] = target_value;
                }
                else
                {
                    self.ChengJiuEventCoalesceAdd.TryGetValue(key, out int sum);
                    self.ChengJiuEventCoalesceAdd[key] = sum + target_value;
                }
                return;
            }

            self.ApplyChengJiuEvent(chengJiuTarget, target_id, target_value);
        }

        private static void FlushChengJiuEventBatch(this ChengJiuComponentServer self)
        {
            foreach (var kv in self.ChengJiuEventCoalesceSet)
            {
                self.ApplyChengJiuEvent((ChengJiuTargetEnum)kv.Key.Item1, kv.Key.Item2, kv.Value);
            }
            foreach (var kv in self.ChengJiuEventCoalesceAdd)
            {
                self.ApplyChengJiuEvent((ChengJiuTargetEnum)kv.Key.Item1, kv.Key.Item2, kv.Value);
            }
            self.ChengJiuEventCoalesceSet.Clear();
            self.ChengJiuEventCoalesceAdd.Clear();
        }

        private static void ApplyChengJiuEvent(this ChengJiuComponentServer self, ChengJiuTargetEnum chengJiuTarget, int target_id, int target_value=1)
        {
            int chengJiuTargetInt = (int)chengJiuTarget;
            List<int> chengjiuList = null;
            ChengJiuHelper.Instance.ChengJiuTargetData.TryGetValue(chengJiuTargetInt, out chengjiuList);
            if (chengjiuList == null)
            {
                return;
            }

            HashSet<int> progressIds = new HashSet<int>();
            for (int k = 0; k < self.ChengJiuProgessList.Count; k++)
            {
                progressIds.Add(self.ChengJiuProgessList[k].ChengJiuID);
            }
            HashSet<int> completeIds = new HashSet<int>(self.ChengJiuCompleteList);

            for (int i = 0;i < chengjiuList.Count;i ++)
            {
                if (progressIds.Contains(chengjiuList[i]) || completeIds.Contains(chengjiuList[i]))
                {
                    continue;
                }

                self.ChengJiuProgessList.Add(new ChengJiuInfo() { ChengJiuID = chengjiuList[i] });
            }

            for (int i = self.ChengJiuProgessList.Count - 1; i >= 0; i--)
            {
                ChengJiuInfo chengJiuInfo = self.ChengJiuProgessList[i];
                /*
                ChengJiuConfig chengJiuConfig = ChengJiuConfigCategory.Instance.Get(chengJiuInfo.ChengJiuID);
                if (chengJiuTargetInt != chengJiuConfig.TargetType)
                {
                    continue;
                }
                
                switch (chengJiuTarget)
                {
                    case ChengJiuTargetEnum.PlayerLevel_205:
                    case ChengJiuTargetEnum.SkillShuLianDu_208:
                    case ChengJiuTargetEnum.CombatToValue_211:
                    case ChengJiuTargetEnum.ZodiacEquipNumber_215:
                    case ChengJiuTargetEnum.PetNSkill_305:
                    case ChengJiuTargetEnum.PegScoreToValue_307:
                    case ChengJiuTargetEnum.PetArrayScoreToValue_308:
                    case ChengJiuTargetEnum.PetTianTiRank_309:
                    case ChengJiuTargetEnum.ZiZhiToValue_311:
                    case ChengJiuTargetEnum.ZiZhiUpValue_312:
                        if (target_id != chengJiuConfig.TargetID)
                        {
                            continue;
                        }
                        chengJiuInfo.ChengJiuProgess = target_value;
                        break;
                    case ChengJiuTargetEnum.JianDingEqipNumber_212:
                        if (target_id < chengJiuConfig.TargetID)
                        {
                            continue;
                        }
                        chengJiuInfo.ChengJiuProgess += target_value;
                        break;
                    default:
                        if (target_id != chengJiuConfig.TargetID)
                        {
                            continue;
                        }
                        chengJiuInfo.ChengJiuProgess += target_value;
                        break;
                }

                int acitiveId = 0;
                switch (chengJiuTarget)
                {
                    case ChengJiuTargetEnum.PetTianTiRank_309:
                        if (chengJiuInfo.ChengJiuProgess <= chengJiuConfig.TargetValue)
                        {
                            acitiveId = chengJiuInfo.ChengJiuID;
                            self.TotalChengJiuPoint += chengJiuConfig.RewardNum;
                            self.ChengJiuCompleteList.Add(chengJiuInfo.ChengJiuID);
                            self.ChengJiuProgessList.RemoveAt(i);
                        }
                        break;
                    case ChengJiuTargetEnum.ZiZhiUpValue_312:
                        if (chengJiuInfo.ChengJiuProgess > chengJiuConfig.TargetValue)
                        {
                            acitiveId = chengJiuInfo.ChengJiuID;
                            self.TotalChengJiuPoint += chengJiuConfig.RewardNum;
                            self.ChengJiuCompleteList.Add(chengJiuInfo.ChengJiuID);
                            self.ChengJiuProgessList.RemoveAt(i);
                        }
                        break;
                    default:
                        if (chengJiuInfo.ChengJiuProgess >= chengJiuConfig.TargetValue)
                        {
                            acitiveId = chengJiuInfo.ChengJiuID;
                            self.TotalChengJiuPoint += chengJiuConfig.RewardNum;
                            self.ChengJiuCompleteList.Add(chengJiuInfo.ChengJiuID);
                            self.ChengJiuProgessList.RemoveAt(i);
                        }
                        break;
                }

                Unit unit = self.GetParent<Unit>();
                if (unit.GetComponent<UnitGateComponent>() == null)
                {
                    return;
                }

                if (acitiveId > 0 && !unit.IsRobot())
                {
                    MessageHelper.SendToClient(unit, new M2C_ChengJiuActiveMessage() { ChengJiuId = acitiveId });
                }
                */
            }
        }

        public static int GetCurrentMagickaSlotIdByPosition(this ChengJiuComponentServer self, int position)
        {
            foreach (var magicinfo in self.MagickaSlotIdList)
            {
               
            }
            return 0;
        }

        public static void OnAddMagickaExpByPosition(this ChengJiuComponentServer self, int position, int addexp)
        {
            /*MagickaSlotInfo magickaSlotInfo = null;

            foreach (var magicinfo in self.MagickaSlotIdList)
            {
                MagickaSlotConfig magickaSlotConfig = MagickaSlotConfigCategory.Instance.Get(magicinfo.SlotId);
                if (magickaSlotConfig.Position == position + 1)
                {
                    magickaSlotInfo = magicinfo;
                    break;
                }
            }
            if (magickaSlotInfo == null || magickaSlotInfo.SlotId == 0)
            {
                return;
            }
            magickaSlotInfo.Exp += addexp;
            int nexid = self.GetNextMagickaSlotIdByPosition(position);
            int curid = self.GetCurrentMagickaSlotIdByPosition(position);
            if (nexid <= curid)
            {
                return;
            }

            int needexp = MagickaSlotConfigCategory.Instance.Get(curid).NeedExp;
            if (magickaSlotInfo.Exp >= needexp)
            {
                magickaSlotInfo.Exp -= needexp;
                magickaSlotInfo.SlotId = nexid;
            }
            */
        }

        public static int GetCurrentMagickaTotalLevel(this ChengJiuComponentServer self)
        {
            int totallevel = 0;
            foreach( var magicinfo in self.MagickaSlotIdList )
            {
              
            }
            return totallevel;
        }

        public static void OnOpenMagicka(this ChengJiuComponentServer self,int position, int magicid)
        {
            for (int i = self.MagickaSlotIdList.Count - 1; i >= 0; i--)
            {
              
            }

            self.MagickaSlotIdList.Add(new MagickaSlotInfo() { SlotId = magicid, Exp = 0 });
        }

        public static List<AttributeItem> GetMagickaProLists(this ChengJiuComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();
            for (int i = self.MagickaSlotIdList.Count - 1; i >= 0; i--)
            {
            }

            return proList;
        }

        public static int GetMaxMagickaSlotIdPosition(this ChengJiuComponentServer self)
        {
            int position = 0;
           
            return position;
        }

        public static int GetFirstMagickaSlotIdByPosition(this ChengJiuComponentServer self, int position)
        {

            return 0;
        }

        public static int GetNextMagickaSlotIdByPosition(this ChengJiuComponentServer self, int position)
        {
            int id = self.GetCurrentMagickaSlotIdByPosition(position);
           
            return id;
        }
    }
}
