using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    public class HomeComponentAwake : AwakeSystem<HomeComponentServer>
    {
        public override void Awake(HomeComponentServer self)
        {
            self.InitOpenList();
        }
    }

    public static class HomeComponentServerSystem
    {



        public static void CheckDaShiPro(this HomeComponentServer self)
        {
        }


        public static void OnGmGaoJi(this HomeComponentServer self)
        {

        }

        public static bool IsMyHome(this HomeComponentServer self, long selfId)
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
        public static void CheckOvertime(this HomeComponentServer self)
        {
        }

        public static List<int> InitOpenList(this HomeComponentServer self)
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
        public static void EnsureHomeData(this HomeComponentServer self)
        {
            if (self.HomeLv > 0)
            {
                return;
            }

            self.HomeLv = 1;
            self.HomeFund = 10000;
        }

        public static void CheckHomeData(this HomeComponentServer self)
        {
            self.EnsureHomeData();
            if (!LDHomeCategory.Instance.Contain(self.HomeLv))
            {
                self.HomeLv = 1;
            }
            if (!LDHomeCategory.Instance.Contain(self.HomeLv + 1) && self.HomeExp > 0)
            {
                self.HomeExp = 0;
            }
        }

        public static void AddHomeFund(this HomeComponentServer self, long delta)
        {
            self.HomeFund += delta;
            if (self.HomeFund < 0)
            {
                self.HomeFund = 0;
            }
        }

        public static void AddHomeExp(this HomeComponentServer self, long delta)
        {
            self.HomeExp += delta;
            if (self.HomeExp < 0)
            {
                self.HomeExp = 0;
            }
        }

        public static void AddHomeLv(this HomeComponentServer self, int delta)
        {
            self.HomeLv += delta;
            if (self.HomeLv < 1)
            {
                self.HomeLv = 1;
            }
            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit?.GetComponent<RoleInfoComponentServer>()?.RoleInfo;
            PlayerEconomyHelper.NotifyRoleDataProgression(unit, UserDataType.HomeLv, roleInfo);
        }

        public static void OnLogin(this HomeComponentServer self)
        {
#if SERVER
            self.CheckHomeData();
            List<int> numbers = self.LearnMakeIds_7;

            // 使用 Distinct() 去除重复元素
            self.LearnMakeIds_7 = numbers.Distinct().ToList();

            if (self.RefreshMonsterTime_2 == 0)
            {
                self.RefreshMonsterTime_2 = TimeHelper.ServerNow() - TimeHelper.Hour * 5;
            }
#endif
        }

        public static void OnBeforEnter(this HomeComponentServer self)
        {
            self.CheckOvertime();
            self.CheckRefreshMonster();
            self.CheckPetExp();
        }

        public static void CheckPetExp(this HomeComponentServer self)
        {

        }

        public static void OnRemoveUnit(this HomeComponentServer self, long unitid)
        {

        }

        public static void CheckRefreshMonster(this HomeComponentServer self)
        {

        }

        public static int OnPastureBuyRequest(this HomeComponentServer self, int ProductId)
        {

            return ErrorCode.ERR_ItemNotEnoughError;
        }


        public static void SaveDB(this HomeComponentServer self)
        { 
            
        }

        /// <summary>
        /// 日清
        /// </summary>
        /// <param name="self"></param>
        public static void OnDailyReset(this HomeComponentServer self, bool notice)
        {
            self.UpdatePlanGoodList();
            self.UpdatePurchaseItemList(notice);
            self.CheckDaShiPro();
        }


        public static void UpdatePlanGoodList(this HomeComponentServer self)
        {

        }

        /// <summary>
        /// 整点刷新
        /// </summary>
        /// <param name="self"></param>
        /// <param name="hour_1"></param>
        /// <param name="hour_2"></param>
        public static void OnHourUpdate(this HomeComponentServer self, int hour_1, bool notice)
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

        public static void UpdatePurchaseItemList_2(this HomeComponentServer self)
        {

        }

        public static void UpdatePurchaseItemList(this HomeComponentServer self, bool notice)
        {

        }

        public static void UprootPasture(this HomeComponentServer self, long unitid)
        {
        }

        public static HomePastures GetHomePastures(this HomeComponentServer self, long unitid)
        {

            return null;
        }

        public static int GetRubbishNumber(this HomeComponentServer self)
        {
#if SERVER
            int number = 0;
            long serverNow = TimeHelper.ServerNow();
            
            return number;
#else
            return 0;
#endif
        }

        public static int GetCanGatherNumber(this HomeComponentServer self)
        {
            return 0;
        }

        public static HomePlant GetHomePlant(this HomeComponentServer self, long unitid)
        {
#if SERVER
            for (int i = 0; i < self.HomePlantList_7.Count; i++)
            {
                if (self.HomePlantList_7[i].UnitId == unitid)
                {
                    return self.HomePlantList_7[i];
                }
            }
#endif
            return null;
        }

        public static HomePlant GetCellPlant(this HomeComponentServer self, int cell)
        {
#if SERVER
            for (int i = 0; i < self.HomePlantList_7.Count; i++)
            {
                if (self.HomePlantList_7[i].CellIndex == cell)
                { 
                    return self.HomePlantList_7[i];
                }
            }
#endif
            return null;
        }

        public static void UprootPlant(this HomeComponentServer self, int cellIndex)
        {
#if SERVER
            for (int i = self.HomePlantList_7.Count - 1; i >= 0; i--)
            {
                if (self.HomePlantList_7[i].CellIndex == cellIndex)
                {
                    self.HomePlantList_7.RemoveAt(i);
                }
            }
#endif
        }

        public static int GetPeopleNumber(this HomeComponentServer self)
        {
            int number = 0;
            for (int i = 0; i < self.HomePastureList_7.Count; i++)
            {
                LDHome_Farm homePastureConfig = LDHome_FarmCategory.Instance.Get(self.HomePastureList_7[i].ConfigId);
                number += homePastureConfig.Id;
            }
            return number;
        }

        public static int GetOpenPlanNumber(this HomeComponentServer self)
        {
            return self.PlanOpenList_7.Count;
        }
    }
}
