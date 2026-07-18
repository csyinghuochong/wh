using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanDaShiHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanDaShiRequest, M2C_JiaYuanDaShiResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanDaShiRequest request, M2C_JiaYuanDaShiResponse response, Action reply)
        {
            if (request.BagInfoIDs.Count < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoIDs[0]);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            bagComponentServer.OnCostItemData(request.BagInfoIDs[0], 1);

            int jiayuanlv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.JiaYuanLv;
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();  
            LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);
            //7,15;100403,1,5;119203,1,5
            string[] itemUsePars =  null;//gemitemCof.ItemUsePar.Split('@');
            for (int i = 0; i < itemUsePars.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                string[] attriinfo = itemUsePars[i].Split(',');
                if (attriinfo.Length < 2)
                {
                    continue;
                }

                int numeid = int.Parse(attriinfo[0]);
                int maxValue = int.Parse(attriinfo[2]);
                if ( CommonHelper.IfNull(useBagInfo.ItemPar) ) 
                {
                    useBagInfo.ItemPar = "50";
                }
                maxValue = (int)(maxValue * (float.Parse(useBagInfo.ItemPar) + 20f) / 100f);
                if (maxValue > int.Parse(attriinfo[2])) {
                    maxValue = int.Parse(attriinfo[2]);
                }

                if (maxValue < int.Parse(attriinfo[1])) {
                    maxValue = int.Parse(attriinfo[1]);
                }

                int addvalue = RandomHelper.RandomNumber(int.Parse(attriinfo[1]), maxValue + 1);
                KeyValuePair keyValuePair = jiaYuanComponentServer.GetDaShiProInfo(numeid);
                int curvalue = keyValuePair != null ? int.Parse(keyValuePair.Value) : 0;
                int maxvalue = LDHomeCategory.Instance.GetProMax(jiayuanlv, numeid);
                addvalue = Math.Min(addvalue, maxvalue - curvalue);
                addvalue = Math.Max( addvalue, 0 );
                jiaYuanComponentServer.UpdateDaShiProInfo( numeid, addvalue );

                response.JiaYuanProAdd.Add( new KeyValuePairInt() {  KeyId = numeid, Value = addvalue } ); 
            }
            jiaYuanComponentServer.JiaYuanDaShiTime_1++;
            response.JiaYuanDaShiTime = jiaYuanComponentServer.JiaYuanDaShiTime_1;
            response.JiaYuanProList = jiaYuanComponentServer.JiaYuanProList_7;

            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JiaYuanDashiNumber_96, 0, 1);

            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer).Coroutine();
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
