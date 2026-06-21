using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    //[MessageHandler(AppType.Gate)]
    public class Function_Fight
    {

        public M2C_UnitNumericListUpdate m2C_UnitNumericListUpdate = new M2C_UnitNumericListUpdate();

        private static readonly object obj = new object();
        //实例化自身
        private static Function_Fight _instance;
        public static Function_Fight GetInstance()
        {
            lock (obj)
            {
                if (_instance == null)
                {
                    _instance = new Function_Fight();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackUnit"></param>
        /// <param name="defendUnit"></param>
        /// <param name="skillHandler"></param>
        /// <param name="hurtMode">0 默认 1持续伤害</param>
        /// <returns></returns>
        public bool Fight(Unit attackUnit, Unit defendUnit, SkillHandler skillHandler, int hurtMode)
        {
            if (defendUnit.IsDisposed)
            {
                return false;
            }

            LDSkill skillconfig = skillHandler.LdSkillConf;
            long damge = -1;
            
            NumericComponent defendNumber = defendUnit.GetComponent<NumericComponent>();
            defendNumber.ApplyChange(attackUnit, NumericType.HP_Current, damge, 0);
                
            return true;
        }

        //暴击等级等属性转换成实际暴击率的方法
        public static float LvProChange(long value, int lv)
        {
            float proValue = (float)value / (float)(7500 + lv * 250);
            if (proValue < 0)
            {
                proValue = 0;
            }
            if (proValue > 0.75f)
            {
                proValue = 0.75f;
            }
            return proValue;
        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的攻击加成
        public static float GetFightValueActProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.75f)
            {
                addPro = 0.75f;
            }

            return addPro;

        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的命中和攻击
        public static float GetFightValueCriAndHitProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.2f)
            {
                addPro = 0.2f;
            }

            return addPro;

        }

        //字典是引用,进来的值会发生改变
        public static void AddUpdateProDicList(int typeID, long typeValue, Dictionary<int, long> dic)
        {
            //缓存属性
            if (dic.ContainsKey(typeID))
            {
                dic[typeID] += typeValue;
            }
            else
            {
                dic[typeID] = typeValue;
            }

        }

        //是否是一级属性
        public static bool ifNumTypeOnePro(int numericType)
        {

            if (numericType < (int)NumericType.Max)
            {
                numericType = numericType * 100;
            }
            int nowValue = (int)numericType / 100;
            if (nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 大恶魔  ...血量提升30倍,攻击提升200%，移动速度变为10，自身会变成恶魔模型
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonBig(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 100000, notice);

            ///可以修改属性乘法 属性附属乘法.     
            //numericComponent.Set(NumericType.Numeric_Error, 0, notice);
        }

        /// <summary>
        /// 小恶魔
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonLittle(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 80000, notice);
        }

        /// <summary>
        /// 幽灵
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonGhost(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 50000, notice);
        }


        /// <summary>
        /// 奔跑大赛属性
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_RunRace(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            int monsterid = numericComponent.GetAsInt(NumericType.RunRaceTransform);
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(monsterid);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 5f, notice);
        }

        /// <summary>
        /// 更新基础的属性
        /// </summary>
        /// <param name="unit"></param>
        public void UnitUpdateProperty_Base(Unit unit, bool notice, bool rank)
        {
            if (unit.SceneType == MapTypeEnum.RunRace)
            {
                return;
            }

            //基础职业属性
            UserInfoComponent UnitInfoComponent = unit.GetComponent<UserInfoComponent>();
            UserInfo userInfo = UnitInfoComponent.UserInfo;
            int roleLv = userInfo.Lv;

            //初始化属性
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ResetProperty();
            
            //缓存列表
            Dictionary<int, long> UpdateProDicList = new Dictionary<int, long>();

            LDOccupation ldOccupation = LDOccupationCategory.Instance.Get(userInfo.Occ);

            //属性点
            int PointLiLiang =  ldOccupation.Add_Point_Default[0];
            int PointZhiLi = ldOccupation.Add_Point_Default[1];
            int PointTiZhi =  ldOccupation.Add_Point_Default[2];
            int PointNaiLi =  ldOccupation.Add_Point_Default[3];
            int PointMinJie = ldOccupation.Add_Point_Default[4];
            AddUpdateProDicList(NumericType.Point_Strength, PointLiLiang,UpdateProDicList );
            AddUpdateProDicList(NumericType.Point_Intelligence, PointLiLiang,UpdateProDicList );
            AddUpdateProDicList(NumericType.Point_Constitution, PointLiLiang,UpdateProDicList );
            AddUpdateProDicList(NumericType.Point_Stamina, PointLiLiang,UpdateProDicList );
            AddUpdateProDicList(NumericType.Point_Agility, PointLiLiang,UpdateProDicList );
            
            //职业属性
            List<HideProList> occInitAttribute = LDOccupationCategory.Instance.GetOccInitAttribute(userInfo.Occ);
            //装备属性
            unit.GetComponent<BagComponent>().GetEquipAttribute(occInitAttribute);
            
            for (int pro = 0; pro < occInitAttribute.Count; pro++)
            {
                AddUpdateProDicList(occInitAttribute[pro].HideID, occInitAttribute[pro].HideValue, UpdateProDicList);
            }
            
            //时装
            List<int> fashionids = unit.GetComponent<BagComponent>().FashionActiveIds;
            for (int i = 0; i < fashionids.Count; i++)
            {
                if (!LDFashionCategory.Instance.Contain(fashionids[i]))
                {
                    continue;
                }

                LDFashion ldFashion = LDFashionCategory.Instance.Get(fashionids[i]);
                if (ldFashion.PropertyKey == null || ldFashion.PropertyKey.Length == 0 || ldFashion.PropertyKey[0] == 0)
                {
                    continue;
                }

                for (int pro = 0; pro < ldFashion.PropertyKey.Length; pro++ )
                {
                    AddUpdateProDicList(ldFashion.PropertyKey[pro], ldFashion.PropertyValue[pro], UpdateProDicList);
                }
            }
            
            //史诗宝石数量
            List<int> ShiShiGemID = new List<int>();

            //生命护盾
            List<PropertyValue> lifeShieldList = unit.GetComponent<SkillSetComponent>().GetShieldProLists();
            for (int i = 0; i < lifeShieldList.Count; i++)
            {
                AddUpdateProDicList(lifeShieldList[i].HideID, lifeShieldList[i].HideValue, UpdateProDicList);
            }

            //称号属性
            List<PropertyValue> titlePros = unit.GetComponent<TitleComponent>().GetTitlePro();
            for (int i = 0; i < titlePros.Count; i++)
            {
                AddUpdateProDicList(titlePros[i].HideID, titlePros[i].HideValue, UpdateProDicList);
            }

            //家园属性
            List<PropertyValue> jiayuanPros = unit.GetComponent<JiaYuanComponent>().GetJianYuanPro();
            for (int i = 0; i < jiayuanPros.Count; i++)
            {
                AddUpdateProDicList(jiayuanPros[i].HideID, jiayuanPros[i].HideValue, UpdateProDicList);
            }

            //技能属性
            List<PropertyValue> skillProList = unit.GetComponent<SkillSetComponent>().GetSkillRoleProLists();
            for (int i = 0; i < skillProList.Count; i++)
            {
                //Log.Info("隐藏:" + skillProList[i].HideID + "skillProList[i].HideValue = " + skillProList[i].HideValue);
                AddUpdateProDicList(skillProList[i].HideID, skillProList[i].HideValue, UpdateProDicList);
            }

            //坐骑属性
            List<PropertyValue> zuoqiPros = unit.GetComponent<UserInfoComponent>().GetZuoQiPro();
            for (int i = 0; i < zuoqiPros.Count; i++)
            {
                AddUpdateProDicList(zuoqiPros[i].HideID, zuoqiPros[i].HideValue, UpdateProDicList);
            }

            //收集属性
            List<PropertyValue> shoujiProList = unit.GetComponent<ShoujiComponent>().GetProList();
            for (int i = 0; i < shoujiProList.Count; i++)
            {
                AddUpdateProDicList(shoujiProList[i].HideID, shoujiProList[i].HideValue, UpdateProDicList);
            }

            //精灵属性
            List<PropertyValue> jinglingProList = unit.GetComponent<ChengJiuComponent>().GetJingLingProLists();
            for (int i = 0; i < jinglingProList.Count; i++)
            {
                AddUpdateProDicList(jinglingProList[i].HideID, jinglingProList[i].HideValue, UpdateProDicList);
            }

            List<PropertyValue> magickaProList = unit.GetComponent<ChengJiuComponent>().GetMagickaProLists();
            for (int i = 0; i < magickaProList.Count; i++)
            {
                AddUpdateProDicList(magickaProList[i].HideID, magickaProList[i].HideValue, UpdateProDicList);
            }

            //神兽羁绊属性
            int shenshouNumber = unit.GetComponent<PetComponent>().GetShenShouNumber();
            List<PropertyValue> shenshoujiban = new List<PropertyValue>();
            foreach ((int petnumber, List<PropertyValue> prolist) in CommonConfig.ShenShouJiBan)
            {
                if (shenshouNumber >= petnumber)
                {
                    shenshoujiban.AddRange(prolist);
                }
            }

            for (int i = 0; i < shenshoujiban.Count; i++)
            {
                AddUpdateProDicList(shenshoujiban[i].HideID, shenshoujiban[i].HideValue, UpdateProDicList);
            }
            
            //家园守护
            /*List<PropertyValue> shouhuPros = unit.GetComponent<PetComponent>().GetPetShouHuPro();
            for (int i = 0; i < shouhuPros.Count; i++)
            {
                AddUpdateProDicList(shouhuPros[i].HideID, shouhuPros[i].HideValue, UpdateProDicList);
            }*/

            //天赋系统
            List<PropertyValue> tianfuProList = unit.GetComponent<SkillSetComponent>().GetTianfuRoleProLists();
            for (int i = 0; i < tianfuProList.Count; i++)
            {
                AddUpdateProDicList(tianfuProList[i].HideID, tianfuProList[i].HideValue, UpdateProDicList);
            }
            
            //--------------------新版属性加点------------------------

            long Power_value_add = 0;
            long Intellect_value_add = 0;
            long Agility_value_add = 0;
            long Stamina_value_add = 0;
            long Constitution_value_add = 0;
            int Power_value = 0;
            int Intellect_value = 0;
            int Agility_value = 0;
            int Stamina_value = 0;
            int Constitution_value = 0;

            //力量加物理穿透
            int wuliChuanTouLv = (PointLiLiang + (int)Power_value + (int)Power_value_add) * 5;
            float adddWuLiChuanTou = LvProChange(wuliChuanTouLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(adddWuLiChuanTou * 10000), UpdateProDicList);

            //智力加魔法穿透
            int mageChuanTouLv = (PointZhiLi + (int)Intellect_value + (int)Intellect_value_add) * 5;
            float adddMageChuanTou = LvProChange(mageChuanTouLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(adddMageChuanTou * 10000), UpdateProDicList);

            //敏捷冷却时间
            int cdTimeLv = (PointMinJie + (int)Agility_value + (int)Agility_value_add) * 2;
            float addMinJie = LvProChange(cdTimeLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(addMinJie * 10000), UpdateProDicList);

            //耐力
            int huixueLv = (PointNaiLi + (int)Stamina_value + (int)Stamina_value_add);
            AddUpdateProDicList((int)NumericType.PATK_Max, huixueLv, UpdateProDicList);

            //体力
            int damgeProCostLv = (PointTiZhi + (int)Constitution_value + (int)Constitution_value_add) * 2;
            float damgeProCost = LvProChange(damgeProCostLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(damgeProCost * 10000), UpdateProDicList);

            //攻击部分

            List<int> keys = new List<int>();

            //更新属性
            foreach (int key in UpdateProDicList.Keys)
            {
                //long setValue = numericComponent.GetAsLong(key) + UpdateProDicList[key];
                long setValue = + UpdateProDicList[key];


                if (!notice)
                {
                    numericComponent.Update(key, setValue, false);
                    continue;
                }
                if (NumericHelp.BroadcastType.Contains(key))
                {
                    numericComponent.Update(key, setValue, true);
                }
                else
                {
                    numericComponent.Update(key, setValue, false);
                    keys.Add(key);
                }
            }

            if (notice)
            {
                List<int> ks = new List<int>();
                List<long> vs = new List<long>();

                for (int i = 0; i < keys.Count; i++)
                {
                    int nowValue = (int)keys[i] / 100;
                    if (!ks.Contains(nowValue))
                    {
                        ks.Add(nowValue);
                        vs.Add(numericComponent.GetAsLong(nowValue));
                    }
                }

                //通知自己
                m2C_UnitNumericListUpdate.UnitID = unit.Id;
                m2C_UnitNumericListUpdate.Vs = vs;
                m2C_UnitNumericListUpdate.Ks = ks;
                MessageHelper.SendToClient(unit, m2C_UnitNumericListUpdate);
            }

            UpdateCombat(unit, numericComponent,notice);
            
            //排行榜
            if (rank)
            {
                unit.GetComponent<UserInfoComponent>().UpdateRankInfo();
            }
            
        }
        
        public void UpdateCombat(Unit unit, NumericComponent numericComponent, bool notice)
        {
            //战力计算
            long ShiLi_Act = 0;
            float ShiLi_ActPro = 0f;
            long ShiLi_Def = 0;
            float ShiLi_DefPro = 0f;
            long ShiLi_Hp = 0;
            float ShiLi_HpPro = 0f;
            //long proLvAdd = criLv + hitLv + dodgeLv + resLv + skillAddLv;
            long proLvAdd = 0;

            //传承鉴定特殊属性加成
            int chuanchengProAdd = 0;
        
            //攻击部分
            foreach (var Item in NumericHelp.ZhanLi_Act)
            {
                ShiLi_Act += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            //隐藏技能算在攻击部分

            foreach (var Item in NumericHelp.ZhanLi_ActPro)
            {
                ShiLi_ActPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //Console.WriteLine("ShiLi_ActPro = " + ShiLi_ActPro);

            //幸运副本附加
            int luck = numericComponent.GetAsInt(NumericType.Numeric_Error);
            switch (luck)
            {
                case 0:
                    ShiLi_ActPro += 0.01f;
                    break;
                case 1:
                    ShiLi_ActPro += 0.02f;
                    break;
                case 2:
                    ShiLi_ActPro += 0.04f;
                    break;
                case 3:
                    ShiLi_ActPro += 0.08f;
                    break;
                case 4:
                    ShiLi_ActPro += 0.12f;
                    break;
                case 5:
                    ShiLi_ActPro += 0.2f;
                    break;
                case 6:
                    ShiLi_ActPro += 0.3f;
                    break;
                case 7:
                    ShiLi_ActPro += 0.4f;
                    break;
                case 8:
                    ShiLi_ActPro += 0.5f;
                    break;
                case 9:
                    ShiLi_ActPro += 0.9f;
                    break;

                default:
                    ShiLi_ActPro += 1f;
                    break;
            }

            //防御部分
            foreach (var Item in NumericHelp.ZhanLi_Def)
            {
                ShiLi_Def += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            foreach (var Item in NumericHelp.ZhanLi_DefPro)
            {
                ShiLi_DefPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //血量部分
            foreach (var Item in NumericHelp.ZhanLi_Hp)
            {
                ShiLi_Hp += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            foreach (var Item in NumericHelp.ZhanLi_HpPro)
            {
                ShiLi_HpPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //宠物守护附加战力
            int fightNum = 0;
            PetComponent petCom = unit.GetComponent<PetComponent>();
            for (int i = 0; i < 4; i++)
            {
                if (petCom.PetShouHuList.Count < 4)
                {
                    break;
                }

                RolePetInfo rolePetInfoNow = petCom.GetPetInfo(petCom.PetShouHuList[i]);
                if (rolePetInfoNow == null)
                {
                    continue;
                }
                fightNum = fightNum + rolePetInfoNow.PetPingFen;
            }

            int addShouHuFight = (int)fightNum / 10;

            //其他战力附加
            int addZhanLi = numericComponent.GetAsInt(NumericType.Numeric_Error);

            //觉醒战力附加
   
            List<int> juexingSkillList = unit.GetComponent<SkillSetComponent>().GetJueSkillIds(0);
            int addJueXingZhanLi = 0;
            if (juexingSkillList.Count >= 1)
            {
                addJueXingZhanLi = Math.Min(juexingSkillList.Count, 3) * 300;
            }
            if (juexingSkillList.Count >= 4)
            {
                addJueXingZhanLi += (Math.Min(juexingSkillList.Count, 7) - 3) * 400;
            }
            if (juexingSkillList.Count >= 8)
            {
                addJueXingZhanLi += 500;
            }

            addZhanLi += addJueXingZhanLi;
            
            long OneProvalueNaiLi = 0;
            long OneProvalueZhiLi = 0;
            long OneProvalueMinJie = 0;
            long OneProvalueLiLiang =0;
            long OneProvalueTiZhi = 0;
            addZhanLi = (int)((OneProvalueNaiLi + OneProvalueZhiLi + OneProvalueMinJie + OneProvalueLiLiang + OneProvalueTiZhi));   //属性点放大系数

            //技能属性点附加战力
            int skillPointFight = 0;  //剩余属性点

            skillPointFight = skillPointFight * 50;
            if (skillPointFight < 0)
            {
                skillPointFight = 0;
            }
            //理论不会超过此值
            if (skillPointFight >= 5000)
            {
                skillPointFight = 5000;
            }

            //int zhanliValue =(int)(ShiLi_Act * (1 + ShiLi_ActPro) + ShiLi_Def * (1 + ShiLi_DefPro) + (ShiLi_Hp * 0.1f) * (1 + ShiLi_HpPro)) + roleLv * 50 + (int)proLvAdd + addZhanLi + addShouHuFight;
            int zhanliValue = (int)(ShiLi_Act * (1 + ShiLi_ActPro) + ShiLi_Def * (1 + ShiLi_DefPro) + (ShiLi_Hp * 0.1f) * (1 + ShiLi_HpPro)) + 1 * 100 + (int)proLvAdd + addZhanLi + addShouHuFight + chuanchengProAdd + skillPointFight;
            //Console.WriteLine("ShiLi_Act = " + ShiLi_Act + " ShiLi_ActPro = " + ShiLi_ActPro + " ShiLi_Def = " + ShiLi_Def + " ShiLi_DefPro = "+ ShiLi_DefPro + " ShiLi_Hp = " + ShiLi_Hp + " ShiLi_HpPro = " + ShiLi_HpPro + " proLvAdd = " + proLvAdd + " addZhanLi = " + addZhanLi);

            //根据属性点整体放大发
            long oneProSum = 0;
            int addZhanliValue = (int)(zhanliValue * (oneProSum/30000f));
            if (addZhanliValue > 0) {
                zhanliValue = zhanliValue + addZhanliValue;
                //Console.WriteLine("zhanliValue = " + zhanliValue + " addZhanliValue = " + addZhanliValue + "oneProSum = " + oneProSum);
            }

            //更新战力
            unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.Combat, zhanliValue.ToString(), notice);

            if (zhanliValue < 0 || zhanliValue > 500000)
            {
                Log.Error($"战力异常: {unit.DomainZone()}  {unit.GetComponent<UserInfoComponent>().UserInfo.Name}  {zhanliValue}");
            }

        }
    }


}
