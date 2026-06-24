using System;
using System.Collections.Generic;

namespace ET
{
    public static class LoginHelper
    {

        public static  void OnLogin(this Unit unit, string remoteip)
        {
           
            long currentTime = TimeHelper.ServerNow();
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            roleInfoComponent.OnLogin(remoteip);
            
            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            DateTime dateTime = TimeInfo.Instance.ToDateTime(currentTime);
            long lastLoginTime = roleInfoComponent.LastLoginTime;
            if (lastLoginTime != 0)
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastLoginTime);
                if (dateTime.Day != lastdateTime.Day)
                {
                    Log.Debug($"OnZeroClockUpdate [登录刷新]: {unit.Id}");
                    float passhour = ((currentTime - lastLoginTime) *1f / TimeHelper.Hour);
                    if (passhour >= 24f)
                    {
                        roleInfoComponent.RecoverPiLao(120, false);
                    }
                    else
                    {

                        List<int> indexids_1 = roleInfoComponent.GetTiLiIndexsNew(lastdateTime.Hour, 23);
                        List<int> indexids_2 = roleInfoComponent.GetTiLiIndexsNew(0, dateTime.Hour);
                        List<int> indexids = new List<int>();
                        indexids.Add(0);
                        indexids.AddRange(indexids_1);
                        indexids.AddRange(indexids_2);
                        if (indexids.Count > 0)
                        {
                            int recoverTili = roleInfoComponent.GetTiliRecover(indexids);
                            roleInfoComponent.RecoverPiLao(recoverTili, false);
                            string indexstr = $"{unit.Id}  two day : hour_1: {lastdateTime.Hour}  hour_2:{dateTime.Hour}   indexs: ";
                            for (int index = 0; index < indexids.Count; index++)
                            {
                                indexstr = indexstr + indexids[index].ToString() + "   ";
                            }
                            indexstr = indexstr + $"recover: {recoverTili}";
                            Log.Debug(indexstr);
                        }

                    }
                    roleInfoComponent.OnZeroClockUpdate(false);
                    unit.GetComponent<TaskComponent>().CheckWeeklyUpdate(lastLoginTime, currentTime);
                    unit.GetComponent<TaskComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<EnergyComponent>().OnResetEnergyInfo();
                    unit.GetComponent<HeroDataComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<ActivityComponent>().OnZeroClockUpdate(roleInfo.Lv);
                    unit.GetComponent<ChengJiuComponent>().OnZeroClockUpdate();
                    unit.GetComponent<JiaYuanComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<DataCollationComponent>().OnZeroClockUpdate(false);
                    roleInfoComponent.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
                else
                {
                    int hour_1, hour_2 = 0;
                    hour_1 = lastdateTime.Hour;
                    hour_2 = dateTime.Hour;

                    List<int> indexids = roleInfoComponent.GetTiLiIndexsNew(hour_1, hour_2);
                    if (indexids.Count > 0)
                    { 
                        int recoverTili = roleInfoComponent.GetTiliRecover(indexids);
                        roleInfoComponent.RecoverPiLao(recoverTili, false);
                        string indexstr = $"{unit.Id}  one day  hour_1: {hour_1}  hour_2:{hour_2}   indexs: ";
                        for (int index = 0; index < indexids.Count; index++)
                        {
                            indexstr = indexstr + indexids[index].ToString() + "   ";
                        }
                        indexstr = indexstr + $"recover: {recoverTili}";
                        Log.Debug(indexstr);
                    }
  
                    unit.GetComponent<JiaYuanComponent>().OnLoginCheck(hour_1, hour_2);
                    float passhour = ((currentTime - lastLoginTime) * 1f / TimeHelper.Hour);
                    roleInfoComponent.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
            }
            else
            {
                Log.Debug($"OnZeroClockUpdate [数据初始化]: {unit.Id}");
                unit.GetComponent<TaskComponent>().OnZeroClockUpdate(false);
            }

            unit.GetComponent<BagComponentServer>().OnLogin(roleInfo.RobotId);
            unit.GetComponent<TaskComponent>().OnLogin();
            unit.GetComponent<HeroDataComponent>().OnLogin(roleInfo.RobotId);
            unit.GetComponent<DBSaveComponent>().OnLogin();
            unit.GetComponent<RechargeComponent>().OnLogin();
            unit.GetComponent<PetComponent>().OnLogin();
            unit.GetComponent<ActivityComponent>().OnLogin(roleInfo.Lv);
            unit.GetComponent<TitleComponent>().OnCheckTitle(false);
            unit.GetComponent<ChengJiuComponent>().OnLogin();
            unit.GetComponent<JiaYuanComponent>().OnLogin();
            unit.GetComponent<SkillSetComponent>().OnLogin(roleInfo.Occ);

        }

    }
}