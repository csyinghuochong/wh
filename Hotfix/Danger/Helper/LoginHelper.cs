using System;
using System.Collections.Generic;

namespace ET
{
    public static class LoginHelper
    {

        public static  void OnLogin(this Unit unit, string remoteip)
        {
           
            long currentTime = TimeHelper.ServerNow();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            roleInfoComponentServer.OnLogin(remoteip);
            
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            DateTime dateTime = TimeInfo.Instance.ToDateTime(currentTime);
            long lastLoginTime = roleInfoComponentServer.LastLoginTime;
            if (lastLoginTime != 0)
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(lastLoginTime);
                if (dateTime.Day != lastdateTime.Day)
                {
                    Log.Debug($"OnZeroClockUpdate [登录刷新]: {unit.Id}");
                    float passhour = ((currentTime - lastLoginTime) *1f / TimeHelper.Hour);
                    if (passhour >= 24f)
                    {
                        roleInfoComponentServer.RecoverPiLao(120, false);
                    }
                    else
                    {

                        List<int> indexids_1 = roleInfoComponentServer.GetTiLiIndexsNew(lastdateTime.Hour, 23);
                        List<int> indexids_2 = roleInfoComponentServer.GetTiLiIndexsNew(0, dateTime.Hour);
                        List<int> indexids = new List<int>();
                        indexids.Add(0);
                        indexids.AddRange(indexids_1);
                        indexids.AddRange(indexids_2);
                        if (indexids.Count > 0)
                        {
                            int recoverTili = roleInfoComponentServer.GetTiliRecover(indexids);
                            roleInfoComponentServer.RecoverPiLao(recoverTili, false);
                            string indexstr = $"{unit.Id}  two day : hour_1: {lastdateTime.Hour}  hour_2:{dateTime.Hour}   indexs: ";
                            for (int index = 0; index < indexids.Count; index++)
                            {
                                indexstr = indexstr + indexids[index].ToString() + "   ";
                            }
                            indexstr = indexstr + $"recover: {recoverTili}";
                            Log.Debug(indexstr);
                        }

                    }
                    roleInfoComponentServer.OnZeroClockUpdate(false);
                    unit.GetComponent<TaskComponentServer>().CheckWeeklyUpdate(lastLoginTime, currentTime);
                    unit.GetComponent<TaskComponentServer>().OnZeroClockUpdate(false);
                    unit.GetComponent<HeroDataComponent>().OnZeroClockUpdate(false);
                    unit.GetComponent<ActivityComponentServer>().OnZeroClockUpdate(roleInfo.Lv);
                    unit.GetComponent<ChengJiuComponentServer>().OnZeroClockUpdate();
                    unit.GetComponent<JiaYuanComponentServer>().OnZeroClockUpdate(false);
                    unit.GetComponent<DataCollationComponent>().OnZeroClockUpdate(false);
                    roleInfoComponentServer.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
                else
                {
                    int hour_1, hour_2 = 0;
                    hour_1 = lastdateTime.Hour;
                    hour_2 = dateTime.Hour;

                    List<int> indexids = roleInfoComponentServer.GetTiLiIndexsNew(hour_1, hour_2);
                    if (indexids.Count > 0)
                    { 
                        int recoverTili = roleInfoComponentServer.GetTiliRecover(indexids);
                        roleInfoComponentServer.RecoverPiLao(recoverTili, false);
                        string indexstr = $"{unit.Id}  one day  hour_1: {hour_1}  hour_2:{hour_2}   indexs: ";
                        for (int index = 0; index < indexids.Count; index++)
                        {
                            indexstr = indexstr + indexids[index].ToString() + "   ";
                        }
                        indexstr = indexstr + $"recover: {recoverTili}";
                        Log.Debug(indexstr);
                    }
  
                    unit.GetComponent<JiaYuanComponentServer>().OnLoginCheck(hour_1, hour_2);
                    float passhour = ((currentTime - lastLoginTime) * 1f / TimeHelper.Hour);
                    roleInfoComponentServer.OnJiaYuanExp(Math.Min(passhour, 12f));
                }
            }
            else
            {
                Log.Debug($"OnZeroClockUpdate [数据初始化]: {unit.Id}");
                unit.GetComponent<TaskComponentServer>().OnZeroClockUpdate(false);
            }

            unit.GetComponent<BagComponentServer>().OnLogin(roleInfo.RobotId);
            unit.GetComponent<TaskComponentServer>().OnLogin();
            unit.GetComponent<HeroDataComponent>().OnLogin(roleInfo.RobotId);
            unit.GetComponent<DBSaveComponent>().OnLogin();
            unit.GetComponent<RechargeComponentServer>().OnLogin();
            unit.GetComponent<PetComponentServer>().OnLogin();
            unit.GetComponent<ActivityComponentServer>().OnLogin(roleInfo.Lv);
            unit.GetComponent<TitleComponentServer>().OnCheckTitle(false);
            unit.GetComponent<ChengJiuComponentServer>().OnLogin();
            unit.GetComponent<JiaYuanComponentServer>().OnLogin();
            unit.GetComponent<SkillSetComponentServer>().OnLogin(roleInfo.Occ);

        }

    }
}