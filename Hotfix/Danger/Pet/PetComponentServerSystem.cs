using System;
using System.Collections.Generic;

namespace ET
{

    public static class PetComponentServerSystem
    {

        public static void CheckPetList(this PetComponentServer self, List<long> petList)
        {
            HashSet<long> ids = new HashSet<long>();

            for (int i = petList.Count - 1; i >= 0; i--)
            {
                long petId = petList[i];
                if (petId != 0 && (self.GetPetInfo(petId) == null || !ids.Add(petId)))
                {
                    petList[i] = 0;
                }
            }
        }

        public static void InitPetInfo(this PetComponentServer self)
        {
            
            if (self.PetFormations.Count != 9)
            {
                self.PetFormations.Clear();
                for (int i = 0; i < 9; i++)
                {
                    self.PetFormations.Add(0);
                }
            }
           
            self.CheckPetList(self.PetFormations);

            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int maxLv = LDGlobalValueCategory.Instance.TempValue;
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                PetInfo rolePetInfo = self.PetInfos[i];
                rolePetInfo.PlayerName = roleInfo.Name;
                
                PetHelper.CheckPropretyPoint(rolePetInfo);
            }
        }

        //获取新宠物
        public static PetInfo GenerateNewPet(this PetComponentServer self, int petId)
        {
            Unit unit = self.GetParent<Unit>();
            LDPet ldPetConfig = LDPetCategory.Instance.Get(petId);
            PetInfo newpet = new PetInfo();
            newpet.Id = IdGenerater.Instance.GenerateId();
            newpet.PetStatus = 0;
            newpet.ConfigId = ldPetConfig.Id;
            newpet.PetLv = 1;/// ldPetConfig.l;
            newpet.PetExp = 0;
            newpet.PetName = ldPetConfig.Name.ToString();
            newpet.IfChange = ldPetConfig.Is_Best == 1 ? 1 :0;
            newpet.AddPropretyNum = 0;
            // 先天 Skill / 异化 Skill_Change 读 LDPet，不入库。Pet表备注：固定技能1：3个先天技能；固定技能2：1个异化技能（异化后才激活）
            newpet.Skill_Extra_A = new List<int>();
            newpet.Skill_Extra_B = new List<int>();
            newpet.EatItems = new List<RewardItem>();
            newpet.Aptitude_1 = new PetAptitudeInfo();
            newpet.Aptitude_2 = new PetAptitudeInfo();
            newpet.Aptitude_3 = new PetAptitudeInfo();
            newpet.Aptitude_4 = new PetAptitudeInfo();
            newpet.Aptitude_5 = new PetAptitudeInfo();
            newpet.Aptitude_6 = new PetAptitudeInfo();
            newpet.Star = 1;// RandomHelper.RandomNumber(1, ldPetConfig.Star_Limit + 1);
            PetHelper.InitPetAptitude(newpet);
            //newpet.PetName = PetSkinConfigCategory.Instance.Get(newpet.SkinId).Name;
            newpet.PlayerName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
            return newpet;
        }

        //取随机值 保留两位
        public static float RandomNumberFloatKeep2(this PetComponentServer self, float lower, float upper)
        {

            float value = lower + ((upper - lower) * RandomHelper.RandFloat());
            return (float)Math.Round(value, 2);
        }

        public static void CheckSkin(this PetComponentServer self)
        {
        }

        public static void OnLogin(this PetComponentServer self)
        {
            for (int  i = self.PetInfos.Count - 1; i >= 0; i--)
            {
                if (!LDPetCategory.Instance.Contain(self.PetInfos[i].ConfigId))
                {
                    self.PetInfos.RemoveAt(i);
                }
            }

            self.CheckSkin();
        }


        /// <summary>
        /// 获得宠物后的任务/成就推进
        /// </summary>
        public static void OnPetAdded(this PetComponentServer self, PetInfo newpet)
        {
            PetProgressionHelper.NotifyPetAcquired(self.GetParent<Unit>(), newpet);
        }

        public static string GetPingfenList(this PetComponentServer self)
        {
            string pingFen = string.Empty;

            return pingFen;
        }



        //第一次获得宠物的时候调用
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="getWay">-1</param>
        /// <param name="petId"></param>
        /// <returns></returns>
        public static PetInfo OnAddPet(this PetComponentServer self, int getWay, int petId)
        {
            Unit unit = self.GetParent<Unit>();
            LDPet ldPetConfig =LDPetCategory.Instance.Get(petId);
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
         
            PetInfo newpet = self.GenerateNewPet(petId);
            self.UpdatePetAttribute(newpet, false);
            self.OnPetAdded(newpet);

            self.PetInfos.Add(newpet);
            M2C_PetListUpdate m2C_PetListUpdate = new M2C_PetListUpdate();
            m2C_PetListUpdate.PetInfoAdd.Add(newpet);
            m2C_PetListUpdate.GetWay = 1;
            MessageHelper.SendToClient(unit, m2C_PetListUpdate);

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"AddPet: unitid:{unit.Id}  petconfigid:{newpet.Id}   RolePetInfos {getWay}");
            }

            //如果有皮肤的话更新一次角色属性
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            return newpet;
        }

        //击杀怪物,增加经验等
        public static void OnKillUnit(this PetComponentServer self, Unit beKill)
        {
            PetInfo rolePetInfo = self.GetFightPet();
            if (rolePetInfo == null)
            {
                return;
            }
            if (beKill.Type != UnitType.Monster)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int playerLv = roleInfo.Lv;

            //超过5级不能获得经验
            if (rolePetInfo.PetLv >= playerLv + 5)
            {
                return;
            }

            self.PetAddExp(rolePetInfo, 1);
        }

        public static void UpdatePetZiZhi(this PetComponentServer self, PetInfo rolePetInfo, int itemId)
        {
            // LD 尚未提供资质丹使用参数串，安全跳过，避免 NRE
            Log.Warning($"UpdatePetZiZhi skip until LD ItemUsePar ready: itemId={itemId} pet={rolePetInfo?.ConfigId}");
        }

        //重置属性点
        public static void OnResetPoint(this PetComponentServer self, PetInfo rolePetInfo)
        {
            rolePetInfo.AddPropretyNum = (rolePetInfo.PetLv - 1) * 5;
            rolePetInfo.AddPropretyValue = CommonConfig.DefaultProprety;
            self.UpdatePetAttribute(rolePetInfo, false);
        }

        //增加经验
        public static void PetAddLv(this PetComponentServer self, PetInfo rolePetInfo, int lv)
        {
            if (rolePetInfo == null)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();
            int playerLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
            int newLevel = rolePetInfo.PetLv + lv;
            newLevel = Math.Min(Math.Max(0, newLevel), playerLv + 5);
            rolePetInfo.AddPropretyNum += (newLevel - rolePetInfo.PetLv) * 5;
            rolePetInfo.PetLv = newLevel;

            //刷新属性
            self.UpdatePetAttribute(rolePetInfo, true);

            //通知客户端
            MessageHelper.SendToClient(unit, new M2C_PetDataUpdate() { UpdateType = (int)UserDataType.Level, PetId = rolePetInfo.Id, UpdateTypeValue = rolePetInfo.PetLv.ToString() });
            MessageHelper.Broadcast(unit, new M2C_PetDataBroadcast() { UnitId = unit.Id, UpdateType = (int)UserDataType.Level, PetId = rolePetInfo.Id, UpdateTypeValue = rolePetInfo.PetLv.ToString() });

        }

        public static void OnPetDead(this PetComponentServer self, long petId)
        {
            PetInfo petinfo = self.GetPetInfo(petId);
            if (petinfo == null)
            {
                Log.Warning($"petinfo == null:  {self.Id} {petId}");
                return;
            }
            petinfo.PetStatus = 0;
        }

        public static void OnPetWalk(this PetComponentServer self, long petId, int petstatu)
        {
            PetInfo petinfo = self.GetPetInfo(petId);
            petinfo.PetStatus = petstatu;
        }

        //增加等级
        public static void PetAddExp(this PetComponentServer self, PetInfo rolePetInfo, int exp)
        {
            if (rolePetInfo == null)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();

            int maxLv = LDGlobalValueCategory.Instance.TempValue;
            int newExp = rolePetInfo.PetExp + exp;
            LDExp_Lv xiulianconf1 = LDExp_LvCategory.Instance.Get(rolePetInfo.PetLv);
            if (newExp >= xiulianconf1.Exp_Role && rolePetInfo.PetLv < maxLv)
            {
                self.PetAddLv(rolePetInfo, 1);
                newExp -= xiulianconf1.Exp_Role;
            }

            rolePetInfo.PetExp = newExp;

            //通知客户端
            MessageHelper.SendToClient(unit, new M2C_PetDataUpdate() { UpdateType = (int)UserDataType.Exp, PetId = rolePetInfo.Id, UpdateTypeValue = rolePetInfo.PetExp.ToString() });
        }

       
        public static void RemoveEquipSkill(this PetComponentServer self, PetInfo rolePetInfom, BagInfo bagInfo)
        {
            if (bagInfo == null)
            {
                return;
            }
        }

        public static void UpdatePetAttribute(this PetComponentServer self, PetInfo rolePetInfo, bool updateUnit)
        {
            PetHelper.ApplyAptitudeAttributes(rolePetInfo);
            if (!updateUnit)
            {
                return;
            }

            Unit petUnit = self.GetParent<Unit>().GetParent<UnitComponent>().Get(rolePetInfo.Id);
            NumericComponent petNumeric = petUnit?.GetComponent<NumericComponent>();
            if (petNumeric == null)
            {
                return;
            }

            Dictionary<int, long> applyDic = new Dictionary<int, long>();
            int count = Math.Min(rolePetInfo.Ks.Count, rolePetInfo.Vs.Count);
            for (int i = 0; i < count; i++)
            {
                if (!AttrConfigManager.HasSubItems(rolePetInfo.Ks[i]))
                {
                    applyDic[rolePetInfo.Ks[i]] = rolePetInfo.Vs[i];
                }
            }

            petNumeric.ApplyAttributeDictionary(applyDic, false);
        }

        public static void RemovePet(this PetComponentServer self, long petId, int removetype)
        {
            Unit unit = self.GetParent<Unit>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            for (int i = self.PetInfos.Count - 1; i >= 0; i--)
            {
                if (self.PetInfos[i].Id == petId)
                {
                    int petconfigid = self.PetInfos[i].ConfigId;
                    self.PetInfos.RemoveAt(i);
                    break;
                }
            }

            self.ResetFormation(self.PetFormations, petId);
        }

        /// <summary>
        /// Pet表备注：合体前对副宠进行重置，把药退出来。按 EatItems 退还背包，并清掉副宠 F/G。
        /// 背包满返回 false，不改副宠。
        /// </summary>
        public static bool ResetSubPetRefundItems(this PetComponentServer self, PetInfo subPet)
        {
            if (subPet == null)
            {
                return false;
            }

            if (subPet.EatItems.Count > 0)
            {
                List<RewardItem> refund = new List<RewardItem>();
                for (int i = 0; i < subPet.EatItems.Count; i++)
                {
                    RewardItem eat = subPet.EatItems[i];
                    if (eat == null || eat.ItemID <= 0 || eat.ItemNum <= 0)
                    {
                        continue;
                    }

                    refund.Add(new RewardItem
                    {
                        ItemType = ItemBigType.Type_Item,
                        ItemID = eat.ItemID,
                        ItemNum = eat.ItemNum
                    });
                }

                if (refund.Count > 0)
                {
                    BagComponentServer bag = self.GetParent<Unit>().GetComponent<BagComponentServer>();
                    if (!bag.OnAddItemData(refund, string.Empty, $"{ItemGetWay.PetHeCheng}_{TimeHelper.ServerNow()}"))
                    {
                        return false;
                    }
                }

                subPet.EatItems.Clear();
            }

            ResetAptitudeEat(subPet.Aptitude_1);
            ResetAptitudeEat(subPet.Aptitude_2);
            ResetAptitudeEat(subPet.Aptitude_3);
            ResetAptitudeEat(subPet.Aptitude_4);
            ResetAptitudeEat(subPet.Aptitude_5);
            ResetAptitudeEat(subPet.Aptitude_6);
            return true;
        }

        static void ResetAptitudeEat(PetAptitudeInfo apt)
        {
            if (apt == null)
            {
                return;
            }

            apt.F = 0;
            apt.G = 0;
            apt.Z = apt.E;
        }

        /// <summary>
        /// Get可以取缓存数据，不用读缓存数据库
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static PetInfo GetPetInfo(this PetComponentServer self, long PetId)
        {
            PetInfo petInfo = null;
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                if (self.PetInfos[i].Id == PetId)
                {
                    return self.PetInfos[i];
                }
            }
            return petInfo;
        }

        public static long GetFightPetId(this PetComponentServer self)
        {
            PetInfo rolePetInfo = self.GetFightPet();
            return rolePetInfo != null ? rolePetInfo.Id : 0;
        }

        public static PetInfo GetFightPet(this PetComponentServer self)
        {
            PetInfo petId = null;
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                if (self.PetInfos[i].PetStatus == 1)
                {
                    petId = self.PetInfos[i];
                }
            }
            return petId;
        }

        public static void OnRolePetFenjie(this PetComponentServer self, long petId)
        {
            Unit unit = self.GetParent<Unit>();
            self.RemovePet(petId, 4);

            for (int i = self.PetInfos.Count - 1; i >= 0; i--)
            {
                self.UpdatePetAttribute(self.PetInfos[i], false);
            }

            M2C_PetListUpdate m2C_PetListUpdate = new M2C_PetListUpdate();
            m2C_PetListUpdate.PetInfoDelete.Add(new PetInfo { Id = petId });
            MessageHelper.SendToClient(unit, m2C_PetListUpdate);
        }


        public static List<PetInfo> GetAllPets(this PetComponentServer self)
        {
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                PetInfo rolePetInfo = self.PetInfos[i];
                if (string.IsNullOrEmpty(rolePetInfo.AddPropretyValue))
                {
                    rolePetInfo.AddPropretyNum = (rolePetInfo.PetLv - 1) * 5;
                    rolePetInfo.AddPropretyValue = CommonConfig.DefaultProprety;
                }
            }
            return self.PetInfos;
        }

        public static int GetShenShouNumber(this PetComponentServer self)
        {
            int shenshouNumber = 0;
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                if (PetHelper.IsShenShou(self.PetInfos[i].ConfigId))
                {
                    shenshouNumber++;
                }
            }
            return shenshouNumber;
        }

        public static int GetTotalStar(this PetComponentServer self)
        {
            int star = 0;
            //for (int i = 0; i < self.PetFubenInfos.Count; i++)
            //{
            //    star += self.PetFubenInfos[i].Star;
            //}

            return star;
        }

        /// <summary>
        /// 获取可以领取的最小星级奖励
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetCanRewardId(this PetComponentServer self)
        {
            int rewardId = 0;
            int totalStar = self.GetTotalStar();
          

            return rewardId;
        }

        public static void ResetFormation(this PetComponentServer self, List<long> formation, long petId)
        {
            for (int i = 0; i < formation.Count; i++)
            {
                if (formation[i] == petId)
                {
                    formation[i] = 0;
                }
            }
        }

        /// <summary>
        /// 通关副本ID
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetPassMaxFubenId(this PetComponentServer self)
        {
            int maxid = 0;
            //for (int i = 0; i < self.PetFubenInfos.Count; i++)
            //{
            //    if (self.PetFubenInfos[i].PetFubenId > maxid)
            //    {
            //        maxid = self.PetFubenInfos[i].PetFubenId;
            //    }
            //}

            return maxid;
        }

        public static void OnPassPetFuben(this PetComponentServer self, int petfubenId, int star)
        {
            //for (int i = 0; i < self.PetFubenInfos.Count; i++)
            //{
            //    if (self.PetFubenInfos[i].PetFubenId == petfubenId)
            //    {
            //        self.PetFubenInfos[i].Star = star > self.PetFubenInfos[i].Star ? star : self.PetFubenInfos[i].Star;
            //        return;
            //    }
            //}
            //self.PetFubenInfos.Add(new PetFubenInfo() { PetFubenId = petfubenId, Star = star, Reward = 0 });
        }

        public static void OnPetMingRecord(this PetComponentServer self, PetMingRecord record)
        {

        }

        public static void OnGmGaoJi(this PetComponentServer self)
        {

        }

        public static bool HavePetConfigId(this PetComponentServer self, int configId)
        {
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                if (self.PetInfos[i].ConfigId == configId)
                {
                    return true;
                }
            }
            return false;
        }

        public static void OnGmAddPet(this PetComponentServer self, int petId)
        {

        }


    }
}
