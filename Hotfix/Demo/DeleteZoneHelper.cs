using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 删档前先备份
    /// </summary>
    public static class DeleteZoneHelper
    {
        public static async ETTask DeletionZone(int zone)
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            
            Log.Error("DeleteZoneHelper.DeletionZone");
            await ETTask.CompletedTask;
        }

    }
}
