using System;
using System.Collections.Generic;

namespace ET
{
    public static class LoginHelper
    {

        public static  void OnLogin(this Unit unit, string remoteip)
        {
           
            long currentTime = TimeHelper.ServerNow();
            UserInfoComponent userInfoComponent = unit.GetComponent<UserInfoComponent>();
            userInfoComponent.OnLogin(remoteip);
            
            UserInfo userInfo = userInfoComponent.UserInfo;
            DateTime dateTime = TimeInfo.Instance.ToDateTime(currentTime);
            long lastLoginTime = userInfoComponent.LastLoginTime;
            if (lastLoginTime != 0)
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastLoginTime);
                if (dateTime.Day != lastdateTime.Day)
                {
                    Log.Debug($"OnZeroClockUpdate [登录刷新]: {unit.Id}");
                    float passhour = ((currentTime - lastLoginTime) *1f / TimeHelper.Hour);
                    if (passhour >= 24f)
                    {
                        userInfoComponent.RecoverPiLao(120, false);
                    }
                    else
                    {

                        List<int> indexids_1 = userInfoComponent.GetTiLiIndexsNew(lastdateTime.Hour, 23);
                        List<int> indexids_2 = userInfoComponent.GetTiLiIndexsNew(0, dateTime.Hour);
                        List<int> indexids = new List<int>();
                        indexids.Add(0);
                        indexids.AddRange(indexids_1);
                        indexids.AddRange(indexids_2);
                        if (indexids.Count > 0)
                        {
                            int recoverTili = userInfoComponent.GetTiliRecover(indexids);
                            userInfoComponent.RecoverPiLao(recoverTili, false);
                            string indexstr = $"{unit.Id}  two day : hour_1: {lastdateTime.Hour}  hour_2:{dateTime.Hour}   indexs: ";
                            for (int index = 0; index < indexids.Count; index++)
                            {
                                indexstr = indexstr + indexids[index].ToString() + "   ";
                            }
                            indexstr = indexstr + $"recover: {recoverTili}";
                            Log.Debug(indexstr);
                        }

                    }
                    userInfoComponent.OnZeroClockUpdate(false);
                    unit.GetComponent<TaskComponent>().CheckWeeklyUpdate(lastLoginTime, currentTime);
                    unit.GetComponent<TaskComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<EnergyComponent>().OnResetEnergyInfo();
                    unit.GetComponent<HeroDataComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<ActivityComponent>().OnZeroClockUpdate(userInfo.Lv);
                    unit.GetComponent<ChengJiuComponent>().OnZeroClockUpdate();
                    unit.GetComponent<JiaYuanComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<DataCollationComponent>().OnZeroClockUpdate(false);
                    userInfoComponent.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
                else
                {
                    int hour_1, hour_2 = 0;
                    hour_1 = lastdateTime.Hour;
                    hour_2 = dateTime.Hour;

                    List<int> indexids = userInfoComponent.GetTiLiIndexsNew(hour_1, hour_2);
                    if (indexids.Count > 0)
                    { 
                        int recoverTili = userInfoComponent.GetTiliRecover(indexids);
                        userInfoComponent.RecoverPiLao(recoverTili, false);
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
                    userInfoComponent.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
            }
            else
            {
                Log.Debug($"OnZeroClockUpdate [数据初始化]: {unit.Id}");
                unit.GetComponent<TaskComponent>().OnZeroClockUpdate(false);
            }

            unit.GetComponent<BagComponent>().OnLogin(userInfo.RobotId);
            unit.GetComponent<TaskComponent>().OnLogin();
            unit.GetComponent<HeroDataComponent>().OnLogin(userInfo.RobotId);
            unit.GetComponent<DBSaveComponent>().OnLogin();
            unit.GetComponent<RechargeComponent>().OnLogin();
            unit.GetComponent<PetComponent>().OnLogin();
            unit.GetComponent<ActivityComponent>().OnLogin(userInfo.Lv);
            unit.GetComponent<TitleComponent>().OnCheckTitle(false);
            unit.GetComponent<ChengJiuComponent>().OnLogin();
            unit.GetComponent<JiaYuanComponent>().OnLogin();
            unit.GetComponent<SkillSetComponent>().OnLogin(userInfo.Occ);

        }

    }
}