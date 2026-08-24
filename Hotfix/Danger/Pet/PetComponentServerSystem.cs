using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    public static class PetComponentServerSystem
    {

        public static void CheckPetList(this PetComponentServer self, List<long> petList)
        {
            HashSet<long> ids = new HashSet<long>();

            for (int i = petList.Count - 1; i >= 0; i--)
            {
                if (petList[i] != 0 && (self.GetPetInfo(petList[i]) == null) || ids.Contains(petList[i]))
                {
                    petList[i] = 0;
                }

                if (petList[i] != 0 && ids.Contains(petList[i]))
                {
                    ids.Add(petList[i]);
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
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                PetInfo rolePetInfo = self.PetInfos[i];
               
            }
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
            self.OnPetScoreChanged();
        }

        public static void CheckPetPingFen(this PetComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
        }


        /// <summary>
        /// 宠物资质/评分变化后刷新成就（洗练、合成、升阶等）
        /// </summary>
        public static void OnPetScoreChanged(this PetComponentServer self)
        {
            self.CheckPetPingFen();
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

            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                PetInfo rolePetInfo = self.PetInfos[i];
                int intFen = 0;
                if (intFen == 0)
                {
                    intFen = PetHelper.PetPingFen(rolePetInfo);
                }
                string strFen = $"{rolePetInfo.ConfigId}{ConfigData.DataCollationSpit}{intFen};";
                pingFen += strFen;
            }

            return pingFen;
        }


        public static int GetPetMaxPingFen(this PetComponentServer self)
        {
            int maxPing = 0;
            for (int i = 0; i < self.PetInfos.Count; i++)
            {
                
            }
            return maxPing;
        }


        /// <summary>
        /// 宠物洗炼
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rolePetInfo"></param>
        /// <param name="XiLianType"> 1 表示出生  2 表示洗炼 </param>
        /// <param name="XiLianType"> itemId 可能为0 </param>
        /// <returns></returns>
        public static PetInfo PetXiLian(this PetComponentServer self, PetInfo rolePetInfo, int getWay, int XiLianType)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            LDPet ldPetConfig = LDPetCategory.Instance.Get(rolePetInfo.ConfigId);


            //表示出生创建
            if (XiLianType == 1)
            {
                rolePetInfo.Star = RandomHelper.RandomNumber(1, ldPetConfig.Star_Limit+1);
            }

            return rolePetInfo;
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
            List<int> weight = new List<int>();

    
            PetInfo newpet = self.GenerateNewPet(petId);

            newpet = self.PetXiLian(newpet,getWay, 1);
            self.UpdatePetAttribute(newpet, false);
            self.OnPetAdded(newpet);

            if (PetHelper.IsShenShou(petId))
            {
                int rechargeNumber = (int)unit.GetTotalRechargeNum();
                if (rechargeNumber < 5000)
                {
                    RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                    AntiCheatAuditHelper.LogShenShouSuspect(unit, roleInfo, rechargeNumber);
                }
            }

            /*if (ItemGetWay.PetExplore == getWay && (ldPetConfig.PetQuality >= 3 || ldPetConfig.Skin[0] != newpet.SkinId))
            {
                string username = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                string petshowname = PetSkinConfigCategory.Instance.Get(newpet.SkinId).Name;
                string messagecontent = $"恭喜{username} 在宠物探索系统中获得 {petshowname}！";
                string messagecontentEn = $"Congratulations, {username}  obtained {petshowname} in the Pet Exploration System!";
                ServerMessageHelper.SendBroadMessage(self.DomainZone(), NoticeType.Notice, messagecontent, messagecontentEn);
            }*/

            self.PetInfos.Add(newpet);
            M2C_PetListUpdate m2C_RolePetUpdate = new M2C_PetListUpdate();
            m2C_RolePetUpdate.PetInfoAdd = new List<PetInfo>();
            m2C_RolePetUpdate.PetInfoAdd.Add(newpet);
            MessageHelper.SendToClient(unit, m2C_RolePetUpdate);

            Log.Debug($"AddPet: unitid:{unit.Id}  petconfigid:{newpet.Id}   RolePetInfos {getWay}");

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

       
        public static void UpdatePetAttributeWithData(this PetComponentServer self, BagComponentServer bagComponentServer, NumericComponent numericComponent, PetInfo rolePetInfo, bool updateUnit = false)
        {
            //存储数据
            rolePetInfo.Ks.Clear();
            rolePetInfo.Vs.Clear();

            Dictionary<int, long> attriDic = new Dictionary<int, long>();

            //获取宠物身上属性
            self.UpdatePetNumeric(attriDic);
       

            //获取加点属性
            string[] attributeinfos = rolePetInfo.AddPropretyValue.Split('_');
            int PointLiLiang = int.Parse(attributeinfos[0]);          //力量
            int PointZhiLi = int.Parse(attributeinfos[1]);            //智力
            int PointTiZhi = int.Parse(attributeinfos[2]);            //体制
            int PointNaiLi = int.Parse(attributeinfos[3]);            //耐力


            //刷新一下属性attriDic  赋值给rolePetInfo.Ks rolePetInfo.Vs
            self.UpdatePetNumeric(attriDic);
            foreach (var item in attriDic)
            {
                int numericType = item.Key;
                rolePetInfo.Ks.Add(numericType);
                rolePetInfo.Vs.Add(item.Value);
            }
        }

        public static void UpdatePetNumeric(this PetComponentServer self, Dictionary<int, long> attriDic)
        {
            foreach (KeyValuePair<int, long> kv in attriDic)
            {
                self.Update(kv.Key, attriDic);
            }
        }

        public static void Update(this PetComponentServer self,  int numericType, Dictionary<int, long> attriDic)
        {
            if (numericType < (int)NumericType.Max)
            {
                return;
            }

            int nowValue = (int)numericType / 100;

            int add = nowValue * 100 + 1;
            int mul = nowValue * 100 + 2;
            int finalAdd = nowValue * 100 + 3;
            int buffAdd = AttrLayer.FightFixed(nowValue);
            int buffMul = AttrLayer.FightPercent(nowValue);
            long old = self.GetByKey( nowValue, attriDic);
            long nowPropertyValue = (long)
            (
                (self.GetByKey( add, attriDic) * (1 + self.GetAsFloat( mul, attriDic)) + self.GetByKey( finalAdd, attriDic)) *
                (1 + self.GetAsFloat( buffMul, attriDic))
                + self.GetByKey( buffAdd, attriDic)
            );

            attriDic[nowValue] = nowPropertyValue;
        }

        public static long GetAsLong(this PetComponentServer self, int numericType, Dictionary<int, long> attriDic)
        {
            return self.GetByKey(numericType, attriDic);
        }

        public static int GetAsInt(this PetComponentServer self, int numericType, Dictionary<int, long> attriDic)
        {
            return (int)self.GetByKey(numericType, attriDic);
        }

        public static float GetAsFloat(this PetComponentServer self, int numericType, Dictionary<int, long> attriDic)
        {
            return NumericConvert.StoredToDisplayFloat(numericType, self.GetByKey(numericType, attriDic));
        }

        public static long GetByKey(this PetComponentServer self,  int numericType, Dictionary<int, long> attriDic)
        {
            long value = 0;
            attriDic.TryGetValue(numericType, out value);
            return value;
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
            Unit unit = self.GetParent<Unit>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            self.UpdatePetAttributeWithData(bagComponentServer, numericComponent, rolePetInfo, updateUnit);

            //如果是出战的宠物。再广播一下属性
            if (updateUnit == false)
            {
                return;
            }
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            Unit petUnit = unitComponent.Get(rolePetInfo.Id);
            if (petUnit == null)
            {
                return;
            }
            for (int i = 0; i < rolePetInfo.Ks.Count; i++)
            {
                numericComponent.Set(rolePetInfo.Ks[i], rolePetInfo.Vs[i], false);
            }
            //NumericComponent numericComponent = petUnit.GetComponent<NumericComponent>();
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
            //numericComponent.ApplyValue(NumericType.Numeric_Error, self.GetByKey(rolePetInfo, NumericType.Numeric_Error), true);
        }

        //根据资质换算出当前系数
        private static float GetZiZhiAddPro(this PetComponentServer self, int type, int value)
        {

            float pro = 0.8f;

            if (type == 1)
            {
                if (value >= 1200)
                {
                    //超出算法
                    pro = 0.8f + ((value - 1200) / 600.0f);
                }
                else
                {
                    //低出算法
                    pro = (float)value / 1500.0f;
                }
            }

            if (type == 2)
            {
                if (value >= 2400)
                {
                    //超出算法
                    pro = 0.8f + ((value - 2400) / 1200.0f);
                }
                else
                {
                    //低出算法
                    pro = (float)value / 3000.0f;
                }
            }

            return pro;
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
                    Log.Debug($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");

                    if (petconfigid >= 2000001)
                    {
                        Log.Error($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");
                        Console.WriteLine($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");
                    }

                    //移除宠物之核
                    //bagComponentServer.OnCostItemData(self.RolePetInfos[i].PetEquipList, ItemLocType.PetLocEquip);

                    self.PetInfos.RemoveAt(i);
                    break;
                }
            }

            self.ResetFormation(self.PetFormations, petId);
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

            M2C_PetListMessage m2C_PetListMessage = new M2C_PetListMessage();
            m2C_PetListMessage.PetList = self.PetInfos;
            m2C_PetListMessage.RemovePetId = petId;
            MessageHelper.SendToClient(unit, m2C_PetListMessage);
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
