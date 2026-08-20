using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    public static class PetComponentServerSystem
    {

        public static List<AttributeItem> GetPetShouHuPro(this PetComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();
            if (self.PetShouHuActive == 0)
            {
                return proList;
            }

            int fightNum = 0;       //评分
            int nowNum = 0;
            for (int i = 0; i < 4; i++)
            {
                RolePetInfo rolePetInfoNow = self.GetPetInfo(self.PetShouHuList[i]);
                if (rolePetInfoNow == null)
                {
                    continue;
                }
                fightNum = fightNum + rolePetInfoNow.PetPingFen;
                if (i == (self.PetShouHuActive - 1))
                {
                    //获取当前守护
                    nowNum = rolePetInfoNow.PetPingFen;
                }
            }

            //增加属性
            float addFloat = CommonHelper.GetPetShouHuPro(nowNum, fightNum);
            AttributeItem hide = new AttributeItem();
            hide.AttributeID = int.Parse(CommonConfig.PetShouHuAttri[self.PetShouHuActive - 1].Value2);
            hide.AttributeValue = NumericHelp.ToStoredValue(hide.AttributeID, addFloat);
            proList.Add(hide);

            return proList;
        }


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
            if (!self.PetCangKuOpen.Contains(0))
            {
                self.PetCangKuOpen.Add(0);
            }
            if (self.RolePetEggs.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    self.RolePetEggs.Add(new RolePetEgg());
                }
            }
            if (self.PetFormations.Count != 9)
            {
                self.PetFormations.Clear();
                for (int i = 0; i < 9; i++)
                {
                    self.PetFormations.Add(0);
                }
            }
            if (self.TeamPetList.Count != 9)
            {
                self.TeamPetList.Clear();
                for (int i = 0; i < 9; i++)
                {
                    self.TeamPetList.Add(0);
                }
            }
            if (self.PetShouHuList.Count != 4)
            {
                self.PetShouHuList.Clear();
                for (int i = 0; i < 4; i++)
                {
                    self.PetShouHuList.Add(0);
                }
            }
            if (self.PetMingList.Count != 15)
            {
                self.PetMingList.Clear();
                for (int i = 0; i < 15; i++)
                {
                    self.PetMingList.Add(0);
                }
            }
            if (self.PetMingPosition.Count != 27)
            {
                self.PetMingPosition.Clear();

                for (int i = 0; i < 27; i++)
                {
                    int index = i % 9;
                    int teamid = i / 9;
                    if (index < 5)
                    {
                        long petId = self.PetMingList[teamid * 5 + index];
                        self.PetMingPosition.Add(petId);
                    }
                    else
                    {
                        self.PetMingPosition.Add(0);
                    }
                }
            }
            self.CheckPetList(self.PetFormations);
            self.CheckPetList(self.TeamPetList);
            self.CheckPetList(self.PetShouHuList);
            self.CheckPetList(self.PetMingList);
            self.CheckPetList(self.PetMingPosition);

            if (self.PetShouHuActive == 0)
            {
                self.PetShouHuActive = 1;
            }
            foreach (LDPet petConfig in LDPetCategory.Instance.GetAll().Values)
            {
                bool havepet = false;
                for (int p = 0; p < self.PetSkinList.Count; p++)
                {
                    if (self.PetSkinList[p].KeyId == petConfig.Id)
                    {
                        havepet = true;
                        break;
                    }
                }
                if (!havepet)
                {
                    self.PetSkinList.Add(new KeyValuePair() { KeyId = petConfig.Id, Value = String.Empty });
                }
            }

            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            int maxLv = LDGlobalValueCategory.Instance.TempValue;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                RolePetInfo rolePetInfo = self.RolePetInfos[i];
                rolePetInfo.PlayerName = roleInfo.Name;
                if (rolePetInfo.PetHeXinList.Count == 0)
                {
                    rolePetInfo.PetHeXinList = new List<long>() { 0, 0, 0 };
                }
                if (rolePetInfo.ShouHuPos == 0)
                {
                    rolePetInfo.ShouHuPos = RandomHelper.RandomNumber(1, 5);
                }
                if (PetHelper.IsShenShou(rolePetInfo.ConfigId))
                {
                    for (int skill = rolePetInfo.PetSkill.Count - 1; skill >= 0; skill--)
                    {
                        int skillid = rolePetInfo.PetSkill[skill];
                        if (skillid >= 80001001 && skillid <= 80001028)
                        {
                            rolePetInfo.PetSkill.RemoveAt(skill);
                        }
                    }
                    rolePetInfo.ShouHuPos = 5;
                }

                if (rolePetInfo.PetLv > maxLv && !LDExpCategory.Instance.Contain(rolePetInfo.PetLv))
                {
                    rolePetInfo.PetLv = maxLv;
                }

                PetHelper.CheckPropretyPoint(rolePetInfo);
            }

            if (self.UpdateNumber == 0)
            {
                self.UpdateNumber = 1;

                int skill8Number = 0;
                for (int i = 0; i < self.RolePetInfos.Count; i++)
                {
                    RolePetInfo rolePetInfo = self.RolePetInfos[i];
                    rolePetInfo.SkinId = 0;///LDPetCategory.Instance.Get(rolePetInfo.ConfigId).Skin[0];
                    skill8Number += (rolePetInfo.PetSkill.Count >= 8 ? 1 : 0);

                    if (PetHelper.IsShenShou(rolePetInfo.ConfigId))
                    {
                        self.PetXiLian(rolePetInfo,0, 2, 0, 0);
                    }
                    self.UpdatePetAttribute(rolePetInfo, false);
                }

                skill8Number = Math.Min(5, skill8Number);
                if (skill8Number > 0)
                {
                    //unit.GetComponent<BagComponentServer>().OnAddItemData($"10010097;{skill8Number}", $"{ItemGetWay.PetFenjie}_{TimeHelper.ServerNow()}");
                }
            }
        }

        //获取新宠物
        public static RolePetInfo GenerateNewPet(this PetComponentServer self, int petId, int skinId)
        {
            Unit unit = self.GetParent<Unit>();
            LDPet ldPetConfig = LDPetCategory.Instance.Get(petId);
            RolePetInfo newpet = new RolePetInfo();
            newpet.Id = IdGenerater.Instance.GenerateId();
            newpet.PetStatus = 0;
            newpet.ConfigId = ldPetConfig.Id;
            newpet.PetLv = 1;/// ldPetConfig.l;
            newpet.PetExp = 0;
            newpet.PetName = ldPetConfig.Name.ToString();
            newpet.IfBaby = true;
            newpet.SkinId = 0;/// skinId != 0 ? skinId : ldPetConfig.Skin[0];
            newpet.PetHeXinList = new List<long>() { 0, 0, 0 };
            newpet.AddPropretyNum = 0;
            newpet.AddPropretyValue = ItemNewHelper.GetDefaultGem();
            newpet.ShouHuPos = RandomHelper.RandomNumber(1, 5);
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
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                RolePetInfo rolePetInfo = self.RolePetInfos[i];
               
            }
            for (int i = 0; i < self.RolePetBag.Count; i++)
            {
                RolePetInfo rolePetInfo = self.RolePetBag[i];
               
            }

        }

        public static void OnLogin(this PetComponentServer self)
        {
            for (int  i = self.RolePetInfos.Count - 1; i >= 0; i--)
            {
                if (!LDPetCategory.Instance.Contain(self.RolePetInfos[i].ConfigId))
                {
                    self.RolePetInfos.RemoveAt(i);
                }
            }

            self.CheckSkin();
            self.OnPetScoreChanged();
        }

        public static void CheckPetPingFen(this PetComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            unit.GetComponent<ChengJiuComponentServer>().OnPetPingFen(
                self.GetPetMaxPingFen(),
                self.GetPetArrayPingFen());
        }

        public static void CheckPetZiZhi(this PetComponentServer self)
        {
            int maxHp = 0, maxAct = 0, maxDef = 0, maxAdf = 0, maxMage = 0;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                RolePetInfo petInfo = self.RolePetInfos[i];
                if (petInfo.ZiZhi_Hp > maxHp) maxHp = petInfo.ZiZhi_Hp;
                if (petInfo.ZiZhi_Act > maxAct) maxAct = petInfo.ZiZhi_Act;
                if (petInfo.ZiZhi_Def > maxDef) maxDef = petInfo.ZiZhi_Def;
                if (petInfo.ZiZhi_Adf > maxAdf) maxAdf = petInfo.ZiZhi_Adf;
                if (petInfo.ZiZhi_MageAct > maxMage) maxMage = petInfo.ZiZhi_MageAct;
            }
            self.GetParent<Unit>().GetComponent<ChengJiuComponentServer>()
                .OnPetMaxZiZhi(maxHp, maxAct, maxDef, maxAdf, maxMage);
        }

        /// <summary>
        /// 宠物资质/评分变化后刷新成就（洗练、合成、升阶等）
        /// </summary>
        public static void OnPetScoreChanged(this PetComponentServer self)
        {
            self.CheckPetPingFen();
            self.CheckPetZiZhi();
        }

        /// <summary>
        /// 获得宠物后的任务/成就推进
        /// </summary>
        public static void OnPetAdded(this PetComponentServer self, RolePetInfo newpet)
        {
            PetProgressionHelper.NotifyPetAcquired(self.GetParent<Unit>(), newpet);
        }

        public static int GetPetMaxZiZhi(this PetComponentServer self, int zizhiType)
        {
            int maxPing = 0;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                int zishi = 0;
                switch (zizhiType)
                {

                    case 1: //="获得宠物生命资质超过"&K386&"点"
                        zishi = self.RolePetInfos[i].ZiZhi_Hp;
                        break;
                    case 2: //="获得宠物攻击资质超过"&K387&"点"
                        zishi = self.RolePetInfos[i].ZiZhi_Act;
                        break;
                    case 3: //="获得宠物物防资质超过"&K388&"点"
                        zishi = self.RolePetInfos[i].ZiZhi_Def;
                        break;
                    case 4: //="获得宠物魔防资质超过"&K389&"点"
                        zishi = self.RolePetInfos[i].ZiZhi_Adf;
                        break;
                    case 5: //="获得宠物魔法资质超过"&K390&"点"
                        zishi = self.RolePetInfos[i].ZiZhi_MageAct;
                        break;
                }

                if (zishi >= maxPing)
                {
                    maxPing = zishi;
                }
            }
            return maxPing;
        }

        public static string GetPingfenList(this PetComponentServer self)
        {
            string pingFen = string.Empty;

            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                RolePetInfo rolePetInfo = self.RolePetInfos[i];
                int intFen = rolePetInfo.PetPingFen;
                if (intFen == 0)
                {
                    intFen = PetHelper.PetPingJia(rolePetInfo);
                }
                string strFen = $"{rolePetInfo.ConfigId}{ConfigData.DataCollationSpit}{intFen};";
                pingFen += strFen;
            }

            return pingFen;
        }


        public static int GetPetMaxPingFen(this PetComponentServer self)
        {
            int maxPing = 0;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (self.RolePetInfos[i].PetPingFen >= maxPing)
                {
                    maxPing = self.RolePetInfos[i].PetPingFen;
                }
            }
            return maxPing;
        }

        public static int GetPetArrayPingFen(this PetComponentServer self)
        {
            int pingfen_1 = 0;
            int pingfen_2 = 0;
            for (int i = 0; i < self.TeamPetList.Count; i++)
            {
                RolePetInfo rolePetInfo = self.GetPetInfo(self.TeamPetList[i]);
                if (rolePetInfo != null)
                {
                    pingfen_1 += rolePetInfo.PetPingFen;
                }
            }
            for (int i = 0; i < self.PetFormations.Count; i++)
            {
                RolePetInfo rolePetInfo = self.GetPetInfo(self.PetFormations[i]);
                if (rolePetInfo != null)
                {
                    pingfen_2 += rolePetInfo.PetPingFen;
                }
            }
            return Math.Max(pingfen_1, pingfen_2);
        }

        /// <summary>
        /// 宠物洗炼
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rolePetInfo"></param>
        /// <param name="XiLianType"> 1 表示出生  2 表示洗炼 </param>
        /// <param name="XiLianType"> itemId 可能为0 </param>
        /// <returns></returns>
        public static RolePetInfo PetXiLian(this PetComponentServer self, RolePetInfo rolePetInfo, int getWay, int XiLianType, int itemId, int fuling)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            LDPet ldPetConfig = LDPetCategory.Instance.Get(rolePetInfo.ConfigId);

            bool fulingStatus = false;
            if (XiLianType == 1 && fuling == 1)
            {
                //Log.Console("已附灵！！！！！");
               
                fulingStatus = true;
            }

            /*
            rolePetInfo.PetPingFen = int.Parse(ldPetConfig.Base_PingFen);
            rolePetInfo.ZiZhi_Hp = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_Hp_Min, ldPetConfig.ZiZhi_Hp_Max + addValue);
            rolePetInfo.ZiZhi_Act = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_Act_Min, ldPetConfig.ZiZhi_Act_Max + addValue);
            rolePetInfo.ZiZhi_MageAct = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_MageAct_Min, ldPetConfig.ZiZhi_MageAct_Max + addValue);
            rolePetInfo.ZiZhi_Def = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_Def_Min, ldPetConfig.ZiZhi_Def_Max + addValue);
            rolePetInfo.ZiZhi_Adf = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_Adf_Min, ldPetConfig.ZiZhi_Adf_Max + addValue);
            rolePetInfo.ZiZhi_ActSpeed = RandomHelper.RandomNumber(ldPetConfig.ZiZhi_ActSpeed_Min, ldPetConfig.ZiZhi_ActSpeed_Max + addValue);
            rolePetInfo.ZiZhi_ChengZhang = self.RandomNumberFloatKeep2((float)ldPetConfig.ZiZhi_ChengZhang_Min, (float)ldPetConfig.ZiZhi_ChengZhang_Max);
            */

            //表示出生创建
            if (XiLianType == 1)
            {
                int minStart = 0;// ldPetConfig.InitStartNum[0];
                int maxStart = 1;//ldPetConfig.InitStartNum[1];
                rolePetInfo.Star = RandomHelper.RandomNumber(minStart, maxStart);
            }


            rolePetInfo.Luckly = 0;   //1为运气加倍 

            string[] skilll = null;// ldPetConfig.BaseSkillID.Split(';');
            rolePetInfo.PetSkill = new List<int>();
            for (int i = 0; i < skilll.Length; i++)
            {
                if (skilll[i] == "0")
                {
                    continue;
                }
                rolePetInfo.PetSkill.Add(int.Parse(skilll[i]));
            }

            //增加宠物专注技能
            /*skilll = ldPetConfig.ZhuanZhuSkillID.Split(';');
            for (int i = 0; i < skilll.Length; i++)
            {
                if (skilll[i] == "0")
                {
                    continue;
                }
                rolePetInfo.PetSkill.Add(int.Parse(skilll[i]));
            }*/

            //增加宠物随机技能
            string randomSkillID = string.Empty;// ldPetConfig.RandomSkillID;
            float randomAddPro = 1;
            if (fulingStatus)
            {
                randomAddPro = 2.5f;
            }
            //80001010,01;80001014,0.1;80001015.1

            if (!CommonHelper.IfNull(randomSkillID))
            {
                string[] randomSkillList = randomSkillID.Split(';');
                for (int i = 0; i < randomSkillList.Length; i++)
                {
                    string[] skillInfo = randomSkillList[i].Split(",");

                    int skillID = int.Parse(skillInfo[0]);

                    if (RandomHelper.RandFloat() <= float.Parse(skillInfo[1]) * randomAddPro)
                    {
                        rolePetInfo.PetSkill.Add(skillID);
                    }
                }
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
        /// <param name="skinId"></param>
        /// <param name="fuling"></param>
        /// <returns></returns>
        public static RolePetInfo OnAddPet(this PetComponentServer self, int getWay, int petId, int skinId = 0, int fuling = 0)
        {
            Unit unit = self.GetParent<Unit>();
            LDPet ldPetConfig =LDPetCategory.Instance.Get(petId);
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            List<int> weight = new List<int>();

            if (skinId == 0)
            {
                int index = RandomHelper.RandomByWeight(weight);
                skinId = 0;//ldPetConfig.Skin[index];
            }

            self.OnUnlockSkin(ldPetConfig.Id + ";" + skinId.ToString());

            RolePetInfo newpet = self.GenerateNewPet(petId, skinId);

            newpet = self.PetXiLian(newpet,getWay, 1, 0, fuling);
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

            if (ItemGetWay.PetExplore == getWay)
            {
                self.RolePetBag.Add(newpet);
                M2C_RolePetBagUpdate m2C_RolePetBag = new M2C_RolePetBagUpdate();
                m2C_RolePetBag.RolePetBag = self.RolePetBag;
                m2C_RolePetBag.UpdateMode = 1;
                MessageHelper.SendToClient(unit, m2C_RolePetBag);

                Log.Debug($"AddPet: unitid:{unit.Id}  petconfigid:{newpet.Id}  {newpet.IfBaby}  RolePetBag");
            }
            else
            {
                self.RolePetInfos.Add(newpet);
                M2C_RolePetUpdate m2C_RolePetUpdate = new M2C_RolePetUpdate();
                m2C_RolePetUpdate.PetInfoAdd = new List<RolePetInfo>();
                m2C_RolePetUpdate.PetInfoAdd.Add(newpet);
                MessageHelper.SendToClient(unit, m2C_RolePetUpdate);

                Log.Debug($"AddPet: unitid:{unit.Id}  petconfigid:{newpet.Id}  {newpet.IfBaby}  RolePetInfos {getWay}");
            }

            //如果有皮肤的话更新一次角色属性
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            return newpet;
        }

        //击杀怪物,增加经验等
        public static void OnKillUnit(this PetComponentServer self, Unit beKill)
        {
            RolePetInfo rolePetInfo = self.GetFightPet();
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

        public static void UpdatePetZiZhi(this PetComponentServer self, RolePetInfo rolePetInfo, int itemId)
        {
            // LD 尚未提供资质丹使用参数串，安全跳过，避免 NRE
            Log.Warning($"UpdatePetZiZhi skip until LD ItemUsePar ready: itemId={itemId} pet={rolePetInfo?.ConfigId}");
        }

        //宠物进化
        public static void UpdatePetStage(this PetComponentServer self, RolePetInfo rolePetInfo, int pingfen)
        {
            int maxZiZhi = 20;
            int minZiZhi = 10;

            float floatPro = (float)(pingfen / 7500);
            minZiZhi = (int)((float)minZiZhi * floatPro);
            maxZiZhi = (int)((float)maxZiZhi * floatPro);

            if (minZiZhi < 5)
            {
                minZiZhi = 5;
            }

            if (minZiZhi > 10)
            {
                minZiZhi = 10;
            }

            if (maxZiZhi < 20)
            {
                maxZiZhi = 20;
            }

            if (maxZiZhi > 30)
            {
                maxZiZhi = 30;
            }

            string[] ZiZhi_Hp = new string[] { (minZiZhi * 2).ToString(), (maxZiZhi * 2f).ToString() };
            string[] ZiZhi_Act = new string[] { minZiZhi.ToString(), maxZiZhi.ToString() };
            string[] ZiZhi_Def = new string[] { minZiZhi.ToString(), maxZiZhi.ToString() };
            string[] ZiZhi_Adf = new string[] { minZiZhi.ToString(), maxZiZhi.ToString() };
            string[] ZiZhi_MageAct = new string[] { minZiZhi.ToString(), maxZiZhi.ToString() };

            int oldZiZhiHp = rolePetInfo.ZiZhi_Hp;
            int oldZiZhiAct = rolePetInfo.ZiZhi_Act;
            int oldZiZhiDef = rolePetInfo.ZiZhi_Def;
            int oldZiZhiAdf = rolePetInfo.ZiZhi_Adf;
            int oldZiZhiMageAct = rolePetInfo.ZiZhi_MageAct;

            rolePetInfo.ZiZhi_Hp += RandomHelper.RandomNumber(int.Parse(ZiZhi_Hp[0]), int.Parse(ZiZhi_Hp[1]) + 1);
            rolePetInfo.ZiZhi_Act += RandomHelper.RandomNumber(int.Parse(ZiZhi_Act[0]), int.Parse(ZiZhi_Act[1]) + 1);
            rolePetInfo.ZiZhi_Def += RandomHelper.RandomNumber(int.Parse(ZiZhi_Def[0]), int.Parse(ZiZhi_Def[1]) + 1);
            rolePetInfo.ZiZhi_Adf += RandomHelper.RandomNumber(int.Parse(ZiZhi_Adf[0]), int.Parse(ZiZhi_Adf[1]) + 1);
            rolePetInfo.ZiZhi_MageAct += RandomHelper.RandomNumber(int.Parse(ZiZhi_MageAct[0]), int.Parse(ZiZhi_MageAct[1]) + 1);

            /*
            rolePetInfo.ZiZhi_Hp = Math.Min(rolePetInfo.ZiZhi_Hp, ldPetConfig.ZiZhi_Hp_Max);
            rolePetInfo.ZiZhi_Act = Math.Min(rolePetInfo.ZiZhi_Act, ldPetConfig.ZiZhi_Act_Max);
            rolePetInfo.ZiZhi_Def = Math.Min(rolePetInfo.ZiZhi_Def, ldPetConfig.ZiZhi_Def_Max);
            rolePetInfo.ZiZhi_Adf = Math.Min(rolePetInfo.ZiZhi_Adf, ldPetConfig.ZiZhi_Adf_Max);
            rolePetInfo.ZiZhi_MageAct = Math.Min(rolePetInfo.ZiZhi_MageAct, ldPetConfig.ZiZhi_MageAct_Max);
            */

            //有些宠物突破上线需要在这里做处理
            rolePetInfo.ZiZhi_Hp = Math.Max(rolePetInfo.ZiZhi_Hp, oldZiZhiHp);
            rolePetInfo.ZiZhi_Act = Math.Max(rolePetInfo.ZiZhi_Act, oldZiZhiAct);
            rolePetInfo.ZiZhi_Def = Math.Max(rolePetInfo.ZiZhi_Def, oldZiZhiDef);
            rolePetInfo.ZiZhi_Adf = Math.Max(rolePetInfo.ZiZhi_Adf, oldZiZhiAdf);
            rolePetInfo.ZiZhi_MageAct = Math.Max(rolePetInfo.ZiZhi_MageAct, oldZiZhiMageAct);

            //概率增加1个技能    1-2  100%   3 50%   4 20%    5 10%  
            int addSkillID = 0;

            //获取原始宠物技能数量
            float addSkillPro = 0;
            if (rolePetInfo.PetSkill.Count <= 2)
            {
                addSkillPro = 1;
            }

            if (rolePetInfo.PetSkill.Count == 3)
            {
                addSkillPro = 0.5f;
            }

            if (rolePetInfo.PetSkill.Count == 4)
            {
                addSkillPro = 0.2f;
            }

            if (rolePetInfo.PetSkill.Count == 5)
            {
                addSkillPro = 0.1f;
            }

            if (RandomHelper.RandFloat01() < addSkillPro)
            {
                if (RandomHelper.RandFloat01() <= 0.7f)
                {
                    //低级技能概率70%
                    int add = RandomHelper.RandomNumber(1, 28);
                    addSkillID = 80001000 + add;
                }
                else
                {
                    //高级技能30%
                    int add = RandomHelper.RandomNumber(1, 28);
                    addSkillID = 80002000 + add;
                }
            }

            //如果当前技能有了那么就忽略掉此次技能附加。
            if (rolePetInfo.PetSkill.Contains(addSkillID))
            {
                addSkillID = 0;
            }

            //设置成已进化
            rolePetInfo.UpStageStatus = 2;

            //刷新一下宠物属性
            self.UpdatePetAttribute(rolePetInfo, true);
        }

        public static void UpdatePetChengZhang(this PetComponentServer self, RolePetInfo rolePetInfo, int itemId)
        {
            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            // LD 未迁 ItemUsePar：用 ItemTypeParam1/2 作成长区间下限/上限（万分比或整数值由配置约定）
            if (ldItem.ItemTypeParam1 <= 0 && ldItem.ItemTypeParam2 <= 0)
            {
                Log.Warning($"UpdatePetChengZhang skip: no param itemId={itemId}");
                return;
            }
            float minV = ldItem.ItemTypeParam1 / 10000f;
            float maxV = ldItem.ItemTypeParam2 / 10000f;
            if (maxV < minV)
            {
                float tmp = minV;
                minV = maxV;
                maxV = tmp;
            }
            float addChengZhang = RandomHelper.RandomNumberFloat(minV, maxV);
            rolePetInfo.ZiZhi_ChengZhang += addChengZhang;
        }

        //重置属性点
        public static void OnResetPoint(this PetComponentServer self, RolePetInfo rolePetInfo)
        {
            rolePetInfo.AddPropretyNum = (rolePetInfo.PetLv - 1) * 5;
            rolePetInfo.AddPropretyValue = CommonConfig.DefaultProprety;
            self.UpdatePetAttribute(rolePetInfo, false);
        }

        //增加经验
        public static void PetAddLv(this PetComponentServer self, RolePetInfo rolePetInfo, int lv)
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
            RolePetInfo petinfo = self.GetPetInfo(petId);
            if (petinfo == null)
            {
                Log.Warning($"petinfo == null:  {self.Id} {petId}");
                return;
            }
            petinfo.PetStatus = 0;
        }

        public static void OnPetWalk(this PetComponentServer self, long petId, int petstatu)
        {
            RolePetInfo petinfo = self.GetPetInfo(petId);
            petinfo.PetStatus = petstatu;
        }

        //增加等级
        public static void PetAddExp(this PetComponentServer self, RolePetInfo rolePetInfo, int exp)
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

       
        public static void UpdatePetAttributeWithData(this PetComponentServer self, BagComponentServer bagComponentServer, NumericComponent numericComponent, RolePetInfo rolePetInfo, bool updateUnit = false)
        {
            //存储数据
            rolePetInfo.Ks.Clear();
            rolePetInfo.Vs.Clear();

            Dictionary<int, long> attriDic = new Dictionary<int, long>();

            //获取宠物资质
            float actPro = self.GetZiZhiAddPro(1, rolePetInfo.ZiZhi_Act);
            float magePro = self.GetZiZhiAddPro(1, rolePetInfo.ZiZhi_MageAct);
            float defPro = self.GetZiZhiAddPro(1, rolePetInfo.ZiZhi_Def);
            float adfPro = self.GetZiZhiAddPro(1, rolePetInfo.ZiZhi_Adf);
            float hpPro = self.GetZiZhiAddPro(2, rolePetInfo.ZiZhi_Hp);

            //属性加点对应属性 力量-攻击 智力-魔法 体质-血量 耐力就是物防和魔防
            LDPet ldPetCof = LDPetCategory.Instance.Get(rolePetInfo.ConfigId);

            PetHelper.CheckPropretyPoint(rolePetInfo);

            //宠物装备(三个一个的属性激活新技能  添加到rolePetInfo.PetSkill, 防止技能重复添加，脱装备的时候直接C2M_PetEquipRequest去掉装备技能 )
            Dictionary<int, int> hideSkillId = new Dictionary<int, int>();
            for (int i = 0; i < rolePetInfo.PetEquipList.Count; i++)
            {
                long baginfoId = rolePetInfo.PetEquipList[i];
                if (baginfoId == 0)
                {
                    continue;
                }


                BagInfo userBagInfo = null;// bagComponentServer.GetItemByLoc(ItemLocType.PetLocEquip, baginfoId);
                if (userBagInfo == null || !LDItemCategory.Instance.Contain(userBagInfo.ItemID))
                {
                    continue;
                }

                //存储装备ID
                LDItem ldItemCof = LDItemCategory.Instance.Get(userBagInfo.ItemID);

                LDEquip mLdEquipCon = LDEquipCategory.Instance.Get(ldItemCof.Id);

                /*
                string[] addpro = mEquipCon.AddProperty.Split("|");
                for (int y = 0; y < addpro.Length; y++)
                {
                    if (!string.IsNullOrEmpty(addpro[y] ))
                    {
                        //记录属性
                        string[] proinfo = addpro[y].Split("&");
                        Function_Fight.AddUpdateProDicList(int.Parse(proinfo[0]), long.Parse(proinfo[1]), attriDic);
                    }
                }*/
            }

            //获取宠物身上属性
            self.UpdatePetNumeric(attriDic);
            /*long Power_value = 0;           //力量
            //long Agility_value = 0;  //敏捷
            long Intellect_value = 0;    //智力
            long Stamina_value = 0;      //耐力
            long Constitution_value =  0;       //体质*/
            //Console.WriteLine($"Power_value: {Power_value} {Agility_value} {Intellect_value} {Stamina_value}  {Constitution_value}");


            //获取加点属性
            string[] attributeinfos = rolePetInfo.AddPropretyValue.Split('_');
            int PointLiLiang = int.Parse(attributeinfos[0]);          //力量
            int PointZhiLi = int.Parse(attributeinfos[1]);            //智力
            int PointTiZhi = int.Parse(attributeinfos[2]);            //体制
            int PointNaiLi = int.Parse(attributeinfos[3]);            //耐力


            /*
            int act_Now = (int)((ldPetCof.Base_Act + rolePetInfo.PetLv * ldPetCof.Lv_Act + (PointLiLiang + Power_value) * 10) * actPro * rolePetInfo.ZiZhi_ChengZhang);
            int mage_Now = (int)((ldPetCof.Base_MageAct + rolePetInfo.PetLv * ldPetCof.Lv_MageAct + (PointZhiLi + Intellect_value) * 10) * magePro * rolePetInfo.ZiZhi_ChengZhang);
            int hp_Now = (int)((ldPetCof.Base_Hp + rolePetInfo.PetLv * ldPetCof.Lv_Hp + (PointTiZhi + Constitution_value) * 100 + (PointNaiLi+ Stamina_value) * 30) * hpPro * rolePetInfo.ZiZhi_ChengZhang);      //给额外血宠的属性
            int def_Now = (int)((ldPetCof.Base_Def + rolePetInfo.PetLv * ldPetCof.Lv_Def + (PointNaiLi + Stamina_value) * 8) * defPro * rolePetInfo.ZiZhi_ChengZhang);
            int adf_Now = (int)((ldPetCof.Base_Adf + rolePetInfo.PetLv * ldPetCof.Lv_Adf + (PointNaiLi + Stamina_value) * 8) * adfPro * rolePetInfo.ZiZhi_ChengZhang);

            float speed = ldPetCof.Base_MoveSpeed;*/
            //float speed = self.GetParent<Unit>().GetComponent<NumericComponent>().GetAsFloat(NumericType.Numeric_Error);


            ///传承鉴定：你的召唤物属性提升10%
            ///宠物如有需要 ，在此处加上
            ///rolePetInfo.Ks.Add((int)NumericType.Numeric_Error);
            ///rolePetInfo.Vs.Add(hp_Now * (1 + now_SummonAddPro));
            float now_SummonAddPro = numericComponent.GetAsFloat(NumericType.Numeric_Error);

            //宠物之核
            List<int> petheXinLv = new List<int>();

            /*
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, hp_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.PetSkin, rolePetInfo.SkinId, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, NumericHelp.ToStoredValue(NumericType.Numeric_Error, speed), attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, hp_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, act_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, mage_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, def_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, adf_Now, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, 0, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, 0, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, 0, attriDic);
            Function_Fight.AddUpdateProDicList(NumericType.Numeric_Error, 0, attriDic);
            */

            for (int i = 0; i < rolePetInfo.PetHeXinList.Count; i++)
            {
                long baginfoId = rolePetInfo.PetHeXinList[i];
                if (baginfoId == 0)
                {
                    continue;
                }

                BagInfo bagInfo = null;
                if (bagInfo == null)
                {
                    continue;
                }
                if (!LDItemCategory.Instance.GetAll().TryGetValue(bagInfo.ItemID, out LDItem ldItem))
                {
                    continue;
                }

                //100203;790
                petheXinLv.Add(ldItem.UseLv_Min);

                string attriStr = null;//ldItem.ItemUsePar;
                string[] attriList = attriStr.Split('@');
                for (int a = 0; a < attriList.Length; a++)
                {
                    try
                    {
                        string[] attriItem = attriList[a].Split(';');
                        int typeId = int.Parse(attriItem[0]);
                        AttrConfigManager.MergeAttributeValue(typeId, long.Parse(attriItem[1]), attriDic);
                    }
                    catch (Exception ex)
                    {
                        Log.Info($"attriStrexc Eption： {attriStr} {ex.ToString()}");
                    }
                }
            }

            foreach ((int skillId, int skillNum) in hideSkillId)
            {
               
            }

            //宠物之核套装属性
            string petheXinPro = CommonConfig.GetPetSuitProperty(petheXinLv);
            if (!CommonHelper.IfNull(petheXinPro))
            {
                string[] attriList = petheXinPro.Split(';');
                for (int a = 0; a < attriList.Length; a++)
                {
                    try
                    {
                        string[] attriItem = attriList[a].Split(',');
                        int typeId = int.Parse(attriItem[0]);
                        AttrConfigManager.MergeAttributeValue(typeId, long.Parse(attriItem[1]), attriDic);
                    }
                    catch (Exception ex)
                    {
                        Log.Info($"petheXinPro Exption： {petheXinPro} {ex.ToString()}");
                    }
                }
            }

            //宠物技能
            for (int i = 0; i < rolePetInfo.PetSkill.Count; i++)
            {
                LDSkill_Battle ldSkillCof = LDSkill_BattleCategory.Instance.Get(rolePetInfo.PetSkill[i]);
                
            }


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

        public static void RemoveEquipSkill(this PetComponentServer self, RolePetInfo rolePetInfom, BagInfo bagInfo)
        {
            if (bagInfo == null)
            {
                return;
            }
        }

        public static void UpdatePetAttribute(this PetComponentServer self, RolePetInfo rolePetInfo, bool updateUnit)
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
            for (int i = self.RolePetInfos.Count - 1; i >= 0; i--)
            {
                if (self.RolePetInfos[i].Id == petId)
                {
                    int petconfigid = self.RolePetInfos[i].ConfigId;
                    Log.Debug($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");

                    if (petconfigid >= 2000001)
                    {
                        Log.Error($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");
                        Console.WriteLine($"RemovePet: unitid:{unit.Id}  petconfigid:{petconfigid}");
                    }

                    //移除宠物之核
                    //bagComponentServer.OnCostItemData(self.RolePetInfos[i].PetEquipList, ItemLocType.PetLocEquip);

                    self.RolePetInfos.RemoveAt(i);
                    break;
                }
            }

            self.ResetFormation(self.PetFormations, petId);
            self.ResetFormation(self.TeamPetList, petId);
            self.ResetFormation(self.PetMingList, petId);
            self.ResetFormation(self.PetMingPosition, petId);
        }

        /// <summary>
        /// Get可以取缓存数据，不用读缓存数据库
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RolePetInfo GetPetInfo(this PetComponentServer self, long PetId)
        {
            RolePetInfo petInfo = null;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (self.RolePetInfos[i].Id == PetId)
                {
                    return self.RolePetInfos[i];
                }
            }
            return petInfo;
        }

        /// <summary>
        /// Get可以取缓存数据，不用读缓存数据库
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static RolePetInfo GetPetInfoByBag(this PetComponentServer self, long PetId)
        {
            RolePetInfo petInfo = null;
            for (int i = 0; i < self.RolePetBag.Count; i++)
            {
                if (self.RolePetBag[i].Id == PetId)
                {
                    return self.RolePetBag[i];
                }
            }
            return petInfo;
        }

        public static long GetFightPetId(this PetComponentServer self)
        {
            RolePetInfo rolePetInfo = self.GetFightPet();
            return rolePetInfo != null ? rolePetInfo.Id : 0;
        }

        public static RolePetInfo GetFightPet(this PetComponentServer self)
        {
            RolePetInfo petId = null;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (self.RolePetInfos[i].PetStatus == 1)
                {
                    petId = self.RolePetInfos[i];
                }
            }
            return petId;
        }

        public static void TakeOutBag(this PetComponentServer self, long petId)
        {
            RolePetInfo rolePetInfo = self.GetPetInfoByBag(petId);
            if (rolePetInfo == null)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();

            self.RemovePetBag(petId);

            self.RolePetInfos.Add(rolePetInfo);
            M2C_RolePetUpdate m2C_RolePetUpdate = new M2C_RolePetUpdate();
            m2C_RolePetUpdate.PetInfoAdd = new List<RolePetInfo>();
            m2C_RolePetUpdate.PetInfoAdd.Add(rolePetInfo);
            m2C_RolePetUpdate.GetWay = 2;
            MessageHelper.SendToClient(unit, m2C_RolePetUpdate);
        }

        public static void RemovePetBag(this PetComponentServer self, long petId)
        {
            for (int i = self.RolePetBag.Count - 1; i >= 0; i--)
            {
                if (self.RolePetBag[i].Id == petId)
                {
                    self.RolePetBag.RemoveAt(i);
                    break;
                }
            }

            Unit unit = self.GetParent<Unit>();
            M2C_RolePetBagUpdate m2C_RolePetBag = new M2C_RolePetBagUpdate();
            m2C_RolePetBag.RolePetBag = self.RolePetBag;
            m2C_RolePetBag.UpdateMode = 2;
            MessageHelper.SendToClient(unit, m2C_RolePetBag);
        }



        public static void OnRolePetFenjie(this PetComponentServer self, long petId)
        {
            Unit unit = self.GetParent<Unit>();
            self.RemovePet(petId, 4);

            for (int i = self.RolePetInfos.Count - 1; i >= 0; i--)
            {
                self.UpdatePetAttribute(self.RolePetInfos[i], false);
            }

            M2C_PetListMessage m2C_PetListMessage = new M2C_PetListMessage();
            m2C_PetListMessage.PetList = self.RolePetInfos;
            m2C_PetListMessage.RemovePetId = petId;
            MessageHelper.SendToClient(unit, m2C_PetListMessage);
        }

        public static int GetMaxSkillNumber(this PetComponentServer self)
        {
            int skillNumber = 0;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (self.RolePetInfos[i].PetSkill.Count > skillNumber)
                {
                    skillNumber = self.RolePetInfos[i].PetSkill.Count;
                }
            }
            return skillNumber;
        }

        public static List<RolePetInfo> GetAllPets(this PetComponentServer self)
        {
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                RolePetInfo rolePetInfo = self.RolePetInfos[i];
                if (string.IsNullOrEmpty(rolePetInfo.AddPropretyValue))
                {
                    rolePetInfo.AddPropretyNum = (rolePetInfo.PetLv - 1) * 5;
                    rolePetInfo.AddPropretyValue = CommonConfig.DefaultProprety;
                }
            }
            return self.RolePetInfos;
        }

        public static int GetShenShouNumber(this PetComponentServer self)
        {
            int shenshouNumber = 0;
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (PetHelper.IsShenShou(self.RolePetInfos[i].ConfigId))
                {
                    shenshouNumber++;
                }
            }
            return shenshouNumber;
        }

        public static int GetTotalStar(this PetComponentServer self)
        {
            int star = 0;
            for (int i = 0; i < self.PetFubenInfos.Count; i++)
            {
                star += self.PetFubenInfos[i].Star;
            }

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

        public static void OnUnlockSkin(this PetComponentServer self, string skininfo)
        {
            string[] petskininfo = skininfo.Split(';');
            int petId = int.Parse(petskininfo[0]);
            int skinId = int.Parse(petskininfo[1]);

            for (int p = 0; p < self.PetSkinList.Count; p++)
            {
                if (self.PetSkinList[p].KeyId != petId)
                {
                    //重复激活
                    continue;
                }
                if (!self.PetSkinList[p].Value.Contains(skinId.ToString()))
                {
                    self.PetSkinList[p].Value += ("_" + skinId.ToString());
                }
            }
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
            for (int i = 0; i < self.PetFubenInfos.Count; i++)
            {
                if (self.PetFubenInfos[i].PetFubenId > maxid)
                {
                    maxid = self.PetFubenInfos[i].PetFubenId;
                }
            }

            return maxid;
        }

        public static void OnPassPetFuben(this PetComponentServer self, int petfubenId, int star)
        {
            for (int i = 0; i < self.PetFubenInfos.Count; i++)
            {
                if (self.PetFubenInfos[i].PetFubenId == petfubenId)
                {
                    self.PetFubenInfos[i].Star = star > self.PetFubenInfos[i].Star ? star : self.PetFubenInfos[i].Star;
                    return;
                }
            }
            self.PetFubenInfos.Add(new PetFubenInfo() { PetFubenId = petfubenId, Star = star, Reward = 0 });
        }

        public static void OnPetMingRecord(this PetComponentServer self, PetMingRecord record)
        {
            if (self.PetMingRecordList.Count >= 10)
            {
                self.PetMingRecordList.RemoveAt(0);
            }
            self.PetMingRecordList.Add(record);
        }

        public static void OnGmGaoJi(this PetComponentServer self)
        {

            //每个宠物附带满级的宠物之核,并进化
            List<int> itemids = new List<int>()
            {
            10031001,10031005,10031011,10031013,10031014,10031015,10031016,10031017
            };

            for (int i = 0; i < itemids.Count; i++)
            {
                string itempar =null;//ldItem.ItemUsePar;
                int petid = int.Parse(itempar);
                if (self.HavePetConfigId(petid))
                {
                    continue;
                }
                self.OnGmAddPet(petid);
            }
            
              
            M2C_RolePetUpdate m2C_RolePetUpdate = new M2C_RolePetUpdate();
            m2C_RolePetUpdate.PetInfoAdd = new List<RolePetInfo>();
            m2C_RolePetUpdate.PetInfoAdd.AddRange(self.RolePetInfos);
            MessageHelper.SendToClient(self.GetParent<Unit>(), m2C_RolePetUpdate);
        }

        public static bool HavePetConfigId(this PetComponentServer self, int configId)
        {
            for (int i = 0; i < self.RolePetInfos.Count; i++)
            {
                if (self.RolePetInfos[i].ConfigId == configId)
                {
                    return true;
                }
            }
            return false;
        }

        public static void OnGmAddPet(this PetComponentServer self, int petId)
        {
            //10060230(攻击之核-1)   10060430(物防之核-2) 10060130(生命之核-3)  

            Unit unit = self.GetParent<Unit>();
            LDPet ldPetConfig = LDPetCategory.Instance.Get(petId);
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            List<int> weight = new List<int>();

            int index = RandomHelper.RandomByWeight(weight);
            int skinId = 0;// ldPetConfig.Skin[index];

            self.OnUnlockSkin(ldPetConfig.Id + ";" + skinId.ToString());

            RolePetInfo newpet = self.GenerateNewPet(petId, skinId);

            newpet = self.PetXiLian(newpet, ItemGetWay.GM, 1, 0, 0);
            newpet.PetLv = roleInfoComponentServer.RoleInfo.Lv;
            newpet.AddPropretyValue = $"{newpet.PetLv}_{newpet.PetLv}_{newpet.PetLv}_{newpet.PetLv}";
            newpet.UpStageStatus = 2;
            self.UpdatePetAttribute(newpet, false);
            self.OnPetAdded(newpet);

            self.OnGmPetEquip(10060230, newpet);
            self.OnGmPetEquip(10060430, newpet);
            self.OnGmPetEquip(10060130, newpet);

            self.RolePetInfos.Add(newpet);
            Log.Debug($"AddPet: unitid:{unit.Id}  petconfigid:{newpet.Id}  {newpet.IfBaby}  RolePetInfos.OnGmAddPet");
        }

        public static void OnGmPetEquip(this PetComponentServer self, int itemid, RolePetInfo rolePetInfo)
        {
            Unit unit = self.GetParent<Unit>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            //bagComponentServer.OnAddItemData($"{itemid};1", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}");
            List<BagInfo> bagitemList = null;
            if (bagitemList.Count == 0)
            {
                return;
            }
            LDItem ldItem = LDItemCategory.Instance.Get(itemid);
            int postion = ldItem.ItemType - 1;
            rolePetInfo.PetHeXinList[postion] = bagitemList[0].BagInfoID;
        }

        //判断当前宠物是否已满
        public static bool PetIsFull(this PetComponentServer self)
        {

            Unit unit = self.GetParent<Unit>();
            int userLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
            if (PetHelper.GetBagPetNum(self.RolePetInfos) >= PetHelper.GetPetMaxNumber(unit, userLv))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static PetHeChengResult TryHeChengPets(this PetComponentServer self, RolePetInfo petinfo_1, RolePetInfo petinfo_2, int petHeChengNumber)
        {
            int petLv_1 = petinfo_1.PetLv;
            int petLv_2 = petinfo_2.PetLv;
            int petID_1 = petinfo_1.ConfigId;
            int petID_2 = petinfo_2.ConfigId;
            List<int> petSkillList_1 = petinfo_1.PetSkill;
            List<int> petSkillList_2 = petinfo_2.PetSkill;

            float skillpro = 0.4f;
            int sumValue = petSkillList_1.Count + petSkillList_2.Count;
            if (sumValue < 8)
            {
                skillpro = 0.5f;
            }
            if (sumValue < 6)
            {
                skillpro = 0.6f;
            }
            if (sumValue < 4)
            {
                skillpro = 0.7f;
            }

            float addPro = 0;
            if (petHeChengNumber <= 10 && sumValue <= 6)
            {
                addPro = 0.05f;
            }
            if (petHeChengNumber <= 5 && sumValue <= 6)
            {
                addPro = 0.1f;
            }
            if (petHeChengNumber <= 1 && sumValue <= 6)
            {
                addPro = 0.15f;
            }
            skillpro += addPro;

            List<int> savePetSkillID = new List<int>();
            HashSet<int> savePetSkillIDSet = new HashSet<int>();
            List<int> deletPetSkillID = new List<int>();

            for (int i = 0; i < petSkillList_1.Count; i++)
            {
                if (!savePetSkillIDSet.Contains(petSkillList_1[i]))
                {
                    if (RandomHelper.RandFloat01() <= skillpro && savePetSkillIDSet.Count <= 12)
                    {
                        savePetSkillIDSet.Add(petSkillList_1[i]);
                        savePetSkillID.Add(petSkillList_1[i]);
                    }
                    else
                    {
                        deletPetSkillID.Add(petSkillList_1[i]);
                    }
                }
            }

            try
            {
                for (int i = 0; i < petSkillList_2.Count; i++)
                {
                    if (!savePetSkillIDSet.Contains(petSkillList_2[i]))
                    {
                        if (RandomHelper.RandFloat01() <= skillpro && savePetSkillIDSet.Count <= 12)
                        {
                            savePetSkillIDSet.Add(petSkillList_2[i]);
                            savePetSkillID.Add(petSkillList_2[i]);
                        }
                        else
                        {
                            deletPetSkillID.Add(petSkillList_2[i]);
                        }
                    }
                }

                if (sumValue <= 12 && savePetSkillID.Count < (int)((float)sumValue / 2f))
                {
                    if (deletPetSkillID.Count >= 1 && !savePetSkillIDSet.Contains(deletPetSkillID[0]))
                    {
                        savePetSkillIDSet.Add(deletPetSkillID[0]);
                        savePetSkillID.Add(deletPetSkillID[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Console("PetComponentServer.TryHeChengPets: " + ex.ToString());
            }

            int petID = RandomHelper.RandFloat01() <= 0.5f ? petID_2 : petID_1;

            LDPet bidaiLdPet = LDPetCategory.Instance.Get(petID);
            string[] baseSkillID = null;// bidaiLdPet.BaseSkillID.Split(';');
            if (baseSkillID != null)
            {
                for (int i = 0; i < baseSkillID.Length; i++)
                {
                    int baseSkill = int.Parse(baseSkillID[i]);
                    if (!savePetSkillIDSet.Contains(baseSkill))
                    {
                        savePetSkillIDSet.Add(baseSkill);
                        savePetSkillID.Add(baseSkill);
                    }
                }
            }

            int zizhiNow_Hp = (int)HeChengZiZhi(petinfo_1.ZiZhi_Hp, petinfo_2.ZiZhi_Hp, 3000);
            int zizhiNow_Act = (int)HeChengZiZhi(petinfo_1.ZiZhi_Act, petinfo_2.ZiZhi_Act, 1600);
            int zizhiNow_MageAct = (int)HeChengZiZhi(petinfo_1.ZiZhi_MageAct, petinfo_2.ZiZhi_MageAct, 1600);
            int zizhiNow_Def = (int)HeChengZiZhi(petinfo_1.ZiZhi_Def, petinfo_2.ZiZhi_Def, 1600);
            int zizhiNow_Adf = (int)HeChengZiZhi(petinfo_1.ZiZhi_Adf, petinfo_2.ZiZhi_Adf, 1600);
            int zizhiNow_ActSpeed = (int)HeChengZiZhi(petinfo_1.ZiZhi_ActSpeed, petinfo_2.ZiZhi_ActSpeed, 3000);
            float zizhiNow_ChengZhang = HeChengZiZhi(petinfo_1.ZiZhi_ChengZhang, petinfo_2.ZiZhi_ChengZhang, 1.3f);
            zizhiNow_ActSpeed = 3000;

            int pet_Lv = (int)(Math.Min(petLv_1, petLv_2) * 0.75f + (Math.Max(petLv_1, petLv_2) - Math.Min(petLv_1, petLv_2)) * HeChengRandomZeroToOne());
            int pet_exp = (int)(10000 * HeChengRandomZeroToOne());
            if (pet_Lv < 1)
            {
                pet_Lv = 1;
            }

            RolePetInfo petinfo_update = petID == petID_1 ? petinfo_1 : petinfo_2;
            RolePetInfo petinfo_delete = petID == petID_1 ? petinfo_2 : petinfo_1;

            return new PetHeChengResult()
            {
                UpdatePet = petinfo_update,
                DeletePet = petinfo_delete,
                PetID = petID,
                PetLv = pet_Lv,
                PetExp = pet_exp,
                AddPropretyNum = pet_Lv * 5 + 20,
                AddPropretyValue = ItemNewHelper.GetDefaultGem(),
                IfBaby = false,
                ZiZhi_Hp = zizhiNow_Hp,
                ZiZhi_Act = zizhiNow_Act,
                ZiZhi_MageAct = zizhiNow_MageAct,
                ZiZhi_Def = zizhiNow_Def,
                ZiZhi_Adf = zizhiNow_Adf,
                ZiZhi_ActSpeed = zizhiNow_ActSpeed,
                ZiZhi_ChengZhang = zizhiNow_ChengZhang,
                SavePetSkillID = savePetSkillID,
            };
        }

        private static float HeChengRandomZeroToOne()
        {
            return RandomHelper.RandomNumber(0, 10) * 0.1f;
        }

        private static float HeChengZiZhi(float zizhiValue_1, float zizhiValue_2, float maxZiZhi)
        {
            float ziZhiMin = Math.Min(zizhiValue_1, zizhiValue_2) * 0.95f;
            float ziZhiMax = Math.Max(zizhiValue_1, zizhiValue_2) * 1.05f;
            float zhizhiValue = ziZhiMin + (ziZhiMax - ziZhiMin) * RandomHelper.RandFloat01();
            if (zhizhiValue > maxZiZhi)
            {
                zhizhiValue = maxZiZhi;
            }
            return (float)Math.Round(zhizhiValue, 2);
        }
    }
}
