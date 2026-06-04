using System;
using System.Net;
using System.Collections.Generic;

namespace ET
{

    public class AppStart_Init_Custom : AEvent<EventType.AppStart>
    {
        protected override void Run(EventType.AppStart args)
        {
            //服务器列表移过来
            ConfigData.AccountOldLogic = true;
            ConfigData.CleanSkill = true;
            ConfigData.PackageLimit = 500;
            Console.WriteLine($"AppStart_Init_Custom.CSkill: {ConfigData.CleanSkill}  {ConfigData.FunctionOpenIds.Count}");
        }
    }
}
