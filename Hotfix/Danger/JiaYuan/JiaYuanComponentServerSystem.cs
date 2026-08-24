using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    public class JianYuanComponentAwake : AwakeSystem<JiaYuanComponentServer>
    {
        public override void Awake(JiaYuanComponentServer self)
        {
            self.InitOpenList();
        }
    }

    public static class JianYuanComponentServerSystem
    {
        /// <summary>
        /// int32 Statu = 3;    //0停止散步 1开始散步
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unitid"></param>
        /// <param name="status"></param>
        public static void OnJiaYuanPetWalk(this JiaYuanComponentServer self, RolePetInfo rolePetInfo, int status, int position)
        {

        }

        public static void AddJiaYuanRecord(this JiaYuanComponentServer self, JiaYuanRecord jiaYuanRecord)
        {
            self.JiaYuanRecordList_1.Add(jiaYuanRecord);

            if (self.JiaYuanRecordList_1.Count >= 100)
            {
                self.JiaYuanRecordList_1.RemoveAt(0);   
            }
        }


        public static void CheckDaShiPro(this JiaYuanComponentServer self)
        {
        }

        public static List<AttributeItem> GetJianYuanPro(this JiaYuanComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();

            for (int i = self.JiaYuanProList_7.Count - 1; i >= 0; i--)
            {
                int numericType = self.JiaYuanProList_7[i].KeyId;
                long lvalue = long.Parse(self.JiaYuanProList_7[i].Value );
                proList.Add(new AttributeItem() { AttributeID = numericType, AttributeValue = lvalue });
            }

            List<KeyValuePair> jiayuandashi = CommonConfig.JiaYuanDaShiPro;
            for (int i = 0; i < jiayuandashi.Count; i++)
            {
                string dashiValue2 = jiayuandashi[i].Value2;
                string[] infolist = dashiValue2.Split('@');
                int need_time = int.Parse(infolist[0]);
                string[] attriInfo = infolist[1].Split(',');
                int attributeId = int.Parse(attriInfo[0]);

                int lvalue = 0;
                if (self.JiaYuanDaShiTime_1 >= need_time)
                {
                    lvalue = int.Parse(attriInfo[1]);
                }
                if (lvalue > 0)
                {
                    proList.Add(new AttributeItem() { AttributeID = attributeId, AttributeValue = lvalue });
                }
            }
            return proList;
        }

        public static void OnGmGaoJi(this JiaYuanComponentServer self)
        {
#if SERVER
            LDHome maxjiayuan = null;
            Dictionary<int, LDHome> allJiayuan = LDHomeCategory.Instance.GetAll();
            foreach ( (int jiayualv, LDHome jiaYuanConfig) in allJiayuan)
            {
                maxjiayuan = jiaYuanConfig;
            }

            Dictionary<int,int> maxpro = LDHomeCategory.Instance.JiaYuanProMax[maxjiayuan.Id];
            foreach ( (int keyid, int addvalue) in maxpro)
            {
                self.UpdateDaShiProInfo( keyid, addvalue );
            }


            List<int> FoodList = new List<int>();
            Dictionary<int, LDItem> allItem = LDItemCategory.Instance.GetAll();
            foreach ((int itemid, LDItem Item) in allItem)
            {
                if (Item.ItemType == 1 && Item.ItemType == 131 && Item.Quality > 2)
                {
                    FoodList.Add(Item.Id);
                }
            }
            self.LearnMakeIds_7.Clear();
            self.LearnMakeIds_7.AddRange(FoodList);

            self.PlanOpenList_7.Clear();
            int planMax = CommonConfig.JiaYuanFarmOpen.Count + 4;
            for (int i = 0; i < planMax; i++)
            {
                self.PlanOpenList_7.Add(i);
            }

            self.JiaYuanDaShiTime_1 = 5000;


#endif
        }

        public static void UpdateDaShiProInfo(this JiaYuanComponentServer self, int keyid, int addvalue)
        {
            for (int i = 0; i < self.JiaYuanProList_7.Count; i++)
            {
                if (self.JiaYuanProList_7[i].KeyId == keyid)
                {
                    int oldvalue = int.Parse(self.JiaYuanProList_7[i].Value);
                    oldvalue += addvalue;
                    self.JiaYuanProList_7[i].Value = oldvalue.ToString();
                    return;
                }
            }
            self.JiaYuanProList_7.Add( new KeyValuePair() { KeyId = keyid, Value = addvalue.ToString() } );
        }

        public static KeyValuePair GetDaShiProInfo(this JiaYuanComponentServer self, int keyid)
        {
            for (int i = 0; i < self.JiaYuanProList_7.Count; i++)
            {
                if (self.JiaYuanProList_7[i].KeyId == keyid)
                {
                    return self.JiaYuanProList_7[i];
                }
            }
            return null;
        }

        public static bool IsMyJiaYuan(this JiaYuanComponentServer self, long selfId)
        {
#if !SERVER
            return self.MasterId == selfId;
#else
            return false;
#endif

        }

        /// <summary>
        /// 老的农场作物 过了24个小时自动去掉
        /// </summary>
        /// <param name="self"></param>
        public static void CheckOvertime(this JiaYuanComponentServer self)
        {
#if SERVER
            long serverTime = TimeHelper.ServerNow();
            //植物
            for (int i = self.JianYuanPlantList_7.Count- 1; i >= 0; i--)
            {
                JiaYuanPlant jiaYuanPlant = self.JianYuanPlantList_7[i];
                int state = JiaYuanHelper.GetPlanStage(jiaYuanPlant.ItemId, jiaYuanPlant.StartTime, jiaYuanPlant.GatherNumber);

                if (state != 4)
                {
                    continue;
                }
                if (serverTime - jiaYuanPlant.GatherLastTime <= TimeHelper.OneDay)
                {
                    continue;
                }

                self.JianYuanPlantList_7.RemoveAt (i);
            }

            //动物
            for (int i = self.JiaYuanPastureList_7.Count - 1; i>= 0; i--)
            {
                JiaYuanPastures jiaYuanPlant = self.JiaYuanPastureList_7[i];
                int state = JiaYuanHelper.GetPastureState(jiaYuanPlant.ConfigId, jiaYuanPlant.StartTime, jiaYuanPlant.GatherNumber);

                if (state != 4)
                {
                    continue;
                }
                if (serverTime - jiaYuanPlant.GatherLastTime <= TimeHelper.OneDay)
                {
                    continue;
                }

                self.JiaYuanPastureList_7.RemoveAt(i);
            }
#endif
        }

        public static List<int> InitOpenList(this JiaYuanComponentServer self)
        {
            List<int> inits = new List<int>() { 0, 1, 2, 3 };
            for (int i = 0; i < inits.Count; i++)
            {
                if (!self.PlanOpenList_7.Contains(inits[i]))
                {
                    self.PlanOpenList_7.Add(inits[i]);
                }
            }
            return self.PlanOpenList_7;
        }

        /// <summary>
        /// 新号默认 1 级 / 10000 资金。
        /// </summary>
        public static void EnsureJiaYuanData(this JiaYuanComponentServer self)
        {
            if (self.JiaYuanLv > 0)
            {
                return;
            }

            self.JiaYuanLv = 1;
            self.JiaYuanFund = 10000;
        }

        public static void CheckJiaYuanData(this JiaYuanComponentServer self)
        {
            self.EnsureJiaYuanData();
            if (!LDHomeCategory.Instance.Contain(self.JiaYuanLv))
            {
                self.JiaYuanLv = 1;
            }
            if (!LDHomeCategory.Instance.Contain(self.JiaYuanLv + 1) && self.JiaYuanExp > 0)
            {
                self.JiaYuanExp = 0;
            }
        }

        public static void AddJiaYuanFund(this JiaYuanComponentServer self, long delta)
        {
            self.JiaYuanFund += delta;
            if (self.JiaYuanFund < 0)
            {
                self.JiaYuanFund = 0;
            }
        }

        public static void AddJiaYuanExp(this JiaYuanComponentServer self, long delta)
        {
            self.JiaYuanExp += delta;
            if (self.JiaYuanExp < 0)
            {
                self.JiaYuanExp = 0;
            }
        }

        public static void AddJiaYuanLv(this JiaYuanComponentServer self, int delta)
        {
            self.JiaYuanLv += delta;
            if (self.JiaYuanLv < 1)
            {
                self.JiaYuanLv = 1;
            }
            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            PlayerEconomyHelper.NotifyRoleDataProgression(unit, UserDataType.JiaYuanLv, roleInfo);
        }

        public static void OnLogin(this JiaYuanComponentServer self)
        {
#if SERVER
            self.CheckJiaYuanData();
            List<int> numbers = self.LearnMakeIds_7;

            // 使用 Distinct() 去除重复元素
            self.LearnMakeIds_7 = numbers.Distinct().ToList();

            if (self.RefreshMonsterTime_2 == 0)
            {
                self.RefreshMonsterTime_2 = TimeHelper.ServerNow() - TimeHelper.Hour * 5;
            }
#endif
        }

        public static void OnBeforEnter(this JiaYuanComponentServer self)
        {
            self.CheckOvertime();
            self.CheckRefreshMonster();
            self.CheckPetExp();
        }

        public static void CheckPetExp(this JiaYuanComponentServer self)
        {

        }

        public static void OnRemoveUnit(this JiaYuanComponentServer self, long unitid)
        {
#if SERVER
            for (int i = self.JiaYuanMonster_2.Count - 1; i >= 0; i--)
            {
                JiaYuanMonster keyValuePair = self.JiaYuanMonster_2[i];
                if (keyValuePair.unitId == unitid)
                {
                    self.JiaYuanMonster_2.RemoveAt(i);
                }
            }
#endif
        }

        public static void CheckRefreshMonster(this JiaYuanComponentServer self)
        {
#if SERVER
            //keyValuePair.KeyId    怪物id
            //keyValuePair.Value    怪物出生时间戳
            //keyValuePair.Value2   怪物坐标
            long serverNow =  TimeHelper.ServerNow();
            for (int i = self.JiaYuanMonster_2.Count -1; i >= 0; i--)
            {
                JiaYuanMonster keyValuePair = self.JiaYuanMonster_2[i];
                LDMonster ldMonster = LDMonsterCategory.Instance.Get(keyValuePair.ConfigId);
                // deathTime = ldMonster.DeathTime * 1000;
                //if (serverNow - keyValuePair.BornTime >= deathTime)
                {
                    self.JiaYuanMonster_2.RemoveAt(i);
                }
            }
            
#endif
        }

        public static int OnPastureBuyRequest(this JiaYuanComponentServer self, int ProductId)
        {

            return ErrorCode.ERR_ItemNotEnoughError;
        }

        public static int OnMysteryBuyRequest(this JiaYuanComponentServer self, int ProductId, List<ShopGoodsItem> jiayuanMysterylist)
        {
            return ErrorCode.ERR_ItemNotEnoughError;
        }

        public static void SaveDB(this JiaYuanComponentServer self)
        { 
            
        }

        /// <summary>
        /// 日清
        /// </summary>
        /// <param name="self"></param>
        public static void OnDailyReset(this JiaYuanComponentServer self, bool notice)
        {
            self.UpdatePlanGoodList();
            self.UpdatePurchaseItemList(notice);
            self.CheckDaShiPro();
        }


        public static void UpdatePlanGoodList(this JiaYuanComponentServer self)
        {
#if SERVER
            int openday = DBHelper.GetOpenServerDay(self.DomainZone());
            int jiayuanlv = self.JiaYuanLv;

            /*LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(87);

            self.PlantGoods_7 = RandomShopHelper.InitJiaYuanPlanItemInfos(openday, jiayuanlv, ldGlobalValue.Value);
            self.PastureGoods_7 = JiaYuanHelper.InitJiaYuanPastureList(jiayuanlv);

            self.JiaYuanStore = RandomShopHelper.InitJiaYuanPlanItemInfos(openday, jiayuanlv, "400001;8");*/

#endif
        }

        /// <summary>
        /// 整点刷新
        /// </summary>
        /// <param name="self"></param>
        /// <param name="hour_1"></param>
        /// <param name="hour_2"></param>
        public static void OnHourUpdate(this JiaYuanComponentServer self, int hour_1, bool notice)
        {
#if SERVER
            ///收购12点刷新
            if (hour_1 == 12)
            {
                self.UpdatePurchaseItemList(true);
            }
            if (hour_1 == 6 || hour_1 == 12 || hour_1 == 18)
            {
                self.UpdatePlanGoodList();
            }
#endif
        }

        public static void UpdatePurchaseItemList_2(this JiaYuanComponentServer self)
        {
#if SERVER
            self.PurchaseItemList_7.Clear();

            JiaYuanHelper.InitPurchaseItemList(self.JiaYuanLv, self.PurchaseItemList_7);
#endif
        }

        public static void UpdatePurchaseItemList(this JiaYuanComponentServer self, bool notice)
        {
#if SERVER
            long serverTime = TimeHelper.ServerNow();
            for (int i = 0; i < self.PurchaseItemList_7.Count; i++)
            {
                if (self.PurchaseItemList_7[i].EndTime < serverTime)
                {
                    self.PurchaseItemList_7.RemoveAt(i);
                }
            }

            JiaYuanHelper.InitPurchaseItemList(self.JiaYuanLv, self.PurchaseItemList_7);
            if (notice)
            {
                M2C_JiaYuanUpdate m2C_JiaYuan = new M2C_JiaYuanUpdate() { PurchaseItemList = self.PurchaseItemList_7 };
                MessageHelper.SendToClient( self.GetParent<Unit>(), m2C_JiaYuan);
            }
#endif
        }

        public static void UprootPasture(this JiaYuanComponentServer self, long unitid)
        {
#if SERVER
            for (int i = self.JiaYuanPastureList_7.Count - 1; i >= 0; i--)
            {
                if (self.JiaYuanPastureList_7[i].UnitId == unitid)
                {
                    self.JiaYuanPastureList_7.RemoveAt(i);
                }
            }
#endif
        }

        public static JiaYuanPastures GetJiaYuanPastures(this JiaYuanComponentServer self, long unitid)
        {
#if SERVER
            for (int i = 0; i < self.JiaYuanPastureList_7.Count; i++)
            {
                if (self.JiaYuanPastureList_7[i].UnitId == unitid)
                {
                    return self.JiaYuanPastureList_7[i];
                }
            }
#endif

            return null;
        }

        public static int GetRubbishNumber(this JiaYuanComponentServer self)
        {
#if SERVER
            int number = 0;
            long serverNow = TimeHelper.ServerNow();
            
            return number;
#else
            return 0;
#endif
        }

        public static int GetCanGatherNumber(this JiaYuanComponentServer self)
        {
#if SERVER
            int number = 0;
            for (int i = 0; i < self.JianYuanPlantList_7.Count; i++)
            {
                JiaYuanPlant jiaYuanPlan = self.JianYuanPlantList_7[i];
                int errorcode = JiaYuanHelper.GetPlanShouHuoItem(jiaYuanPlan.ItemId, jiaYuanPlan.StartTime, jiaYuanPlan.GatherNumber, jiaYuanPlan.GatherLastTime);
                if (errorcode == ErrorCode.ERR_Success)
                {
                    number++;
                }
            }
            for (int i = 0; i < self.JiaYuanPastureList_7.Count; i++)
            {
                JiaYuanPastures jiaYuanPasture = self.JiaYuanPastureList_7[i];
                int errorcode = JiaYuanHelper.GetPastureShouHuoItem(jiaYuanPasture.ConfigId, jiaYuanPasture.StartTime, jiaYuanPasture.GatherNumber, jiaYuanPasture.GatherLastTime);
                if (errorcode == ErrorCode.ERR_Success)
                {
                    number++;
                }
            }
            return number;
#else
            return 0;
#endif
        }

        public static JiaYuanPlant GetJiaYuanPlant(this JiaYuanComponentServer self, long unitid)
        {
#if SERVER
            for (int i = 0; i < self.JianYuanPlantList_7.Count; i++)
            {
                if (self.JianYuanPlantList_7[i].UnitId == unitid)
                {
                    return self.JianYuanPlantList_7[i];
                }
            }
#endif
            return null;
        }

        public static JiaYuanPlant GetCellPlant(this JiaYuanComponentServer self, int cell)
        {
#if SERVER
            for (int i = 0; i < self.JianYuanPlantList_7.Count; i++)
            {
                if (self.JianYuanPlantList_7[i].CellIndex == cell)
                { 
                    return self.JianYuanPlantList_7[i];
                }
            }
#endif
            return null;
        }

        public static void UprootPlant(this JiaYuanComponentServer self, int cellIndex)
        {
#if SERVER
            for (int i = self.JianYuanPlantList_7.Count - 1; i >= 0; i--)
            {
                if (self.JianYuanPlantList_7[i].CellIndex == cellIndex)
                {
                    self.JianYuanPlantList_7.RemoveAt(i);
                }
            }
#endif
        }

        public static int GetPeopleNumber(this JiaYuanComponentServer self)
        {
            int number = 0;
            for (int i = 0; i < self.JiaYuanPastureList_7.Count; i++)
            {
                LDHome_Farm jiaYuanPastureConfig = LDHome_FarmCategory.Instance.Get(self.JiaYuanPastureList_7[i].ConfigId);
                number += jiaYuanPastureConfig.Id;
            }
            return number;
        }

        public static int GetOpenPlanNumber(this JiaYuanComponentServer self)
        {
            return self.PlanOpenList_7.Count;
        }
    }
}
