using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_JiaYuanOperateHandler : AMActorLocationRpcHandler<Unit, M2M_JiaYuanOperateRequest, M2M_JiaYuanOperateResponse>
    {
        protected override async ETTask Run(Unit unit, M2M_JiaYuanOperateRequest request, M2M_JiaYuanOperateResponse response, Action reply)
        {
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            JiaYuanOperate jiaYuanOperate = request.JiaYuanOperate;
            switch (jiaYuanOperate.OperateType)
            {
                case JiaYuanOperateType.Visit:
                    jiaYuanComponentServer.AddJiaYuanRecord( new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.Visit,
                        OperateId = 0,
                        PlayerName = jiaYuanOperate.PlayerName,
                        Time = TimeHelper.ServerNow(),
                    });
                    break;
                case JiaYuanOperateType.GatherPlant:
                    JiaYuanPlant jiaYuanPlan = jiaYuanComponentServer.GetJiaYuanPlant(jiaYuanOperate.UnitId);
                    if (jiaYuanPlan == null)
                    {
                        reply();
                        return;
                    }
                    jiaYuanPlan.StealNumber += 1;
                    jiaYuanPlan.GatherNumber += 1;
                    jiaYuanPlan.GatherLastTime = TimeHelper.ServerNow();
                    JiaYuanRecord jiaYuanRecord = new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.GatherPlant,
                        OperateId = jiaYuanPlan.ItemId,
                        PlayerName = jiaYuanOperate.PlayerName,
                        Time = TimeHelper.ServerNow(),
                        PlayerId = jiaYuanOperate.PlayerId,
                    };
                    jiaYuanComponentServer.AddJiaYuanRecord(jiaYuanRecord);
                    break;
                case JiaYuanOperateType.GatherPasture:
                    JiaYuanPastures jiaYuanPasture = jiaYuanComponentServer.GetJiaYuanPastures(jiaYuanOperate.UnitId);
                    if (jiaYuanPasture == null)
                    {
                        reply();
                        return;
                    }
                    jiaYuanPasture.StealNumber += 1;
                    jiaYuanPasture.GatherNumber += 1;
                    jiaYuanPasture.GatherLastTime = TimeHelper.ServerNow();

                    jiaYuanComponentServer.AddJiaYuanRecord(new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.GatherPasture,
                        OperateId = jiaYuanPasture.ConfigId,
                        PlayerName = jiaYuanOperate.PlayerName,
                        Time = TimeHelper.ServerNow(),
                    });
                    break;
                case JiaYuanOperateType.Pick:
                    unit.GetComponent<JiaYuanComponentServer>().OnRemoveUnit(jiaYuanOperate.UnitId);
                    jiaYuanComponentServer.AddJiaYuanRecord(new JiaYuanRecord()
                    {
                        OperateType = JiaYuanOperateType.Pick,
                        OperateId = jiaYuanOperate.OperateId,
                        PlayerName = jiaYuanOperate.PlayerName,
                        Time = TimeHelper.ServerNow(),
                    });
                    break;
            }

            await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer);
            reply();
        }
    }
}
