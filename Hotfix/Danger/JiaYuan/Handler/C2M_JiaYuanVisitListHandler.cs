using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JiaYuanVisitListHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanVisitListRequest, M2C_JiaYuanVisitListResponse>
    {

        private async ETTask<JiaYuanVisit> GetJiaYuanVisit(int zone, long id)
        {
            List<JiaYuanComponentServer> resultJiaYuan = await Game.Scene.GetComponent<DBComponent>().Query<JiaYuanComponentServer>(zone, _account => _account.Id == id);
            if (resultJiaYuan == null || resultJiaYuan.Count == 0)
            {
                return null;
            }

            List<RoleInfoComponentServer> resultUser = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(zone, _account => _account.Id == id);
            if (resultUser[0].RoleInfo.Lv < 10)
            {
                return null;
            }
            JiaYuanVisit jiaYuanVisit = new JiaYuanVisit() ;
            jiaYuanVisit.Occ = resultUser[0].RoleInfo.Occ;
            jiaYuanVisit.OccTwo = resultUser[0].RoleInfo.OccTwo;
            jiaYuanVisit.PlayerName = resultUser[0].RoleInfo.Name;
            jiaYuanVisit.UnitId = resultJiaYuan[0].Id;
            jiaYuanVisit.Rubbish = resultJiaYuan[0].GetRubbishNumber();
            jiaYuanVisit.Gather = resultJiaYuan[0].GetCanGatherNumber();
            return jiaYuanVisit;
        }

        protected override async ETTask Run(Unit unit, C2M_JiaYuanVisitListRequest request, M2C_JiaYuanVisitListResponse response, Action reply)
        {
            Log.Warning($"C2M_JiaYuanVisitListRequest:{request.ActorId}");
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.JiaYuan, unit.Id))
            {
                JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
                if (request.OperateType == 1)
                {
                    if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.JiaYuanVisitRefresh) >= 3)
                    {
                        return;
                    }
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.JiaYuanVisitRefresh, 1, 0);
                    jiaYuanComponentServer.JiaYuanFuJinTime_3 = 0;
                }

                DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(UnitZoneHelper.GetHomeZone(unit), unit.Id);

                List<long> friendList = new List<long>();
                HashSet<long> friendSet = new HashSet<long>();
                if (dBFriendInfo != null)
                {
                    friendList = dBFriendInfo.FriendList;
                    friendSet = new HashSet<long>(friendList);
                    for (int i = 0; i < friendList.Count; i++)
                    {
                        if (friendList[i] == unit.Id)
                        {
                            continue;
                        }
                        JiaYuanVisit jiaYuanVisit = await GetJiaYuanVisit(UnitZoneHelper.GetHomeZone(unit), friendList[i]);
                        if (jiaYuanVisit != null)
                        {
                            response.JiaYuanVisit_1.Add(jiaYuanVisit);
                        }
                    }
                }

                if (TimeHelper.ServerNow() - jiaYuanComponentServer.JiaYuanFuJinTime_3 > TimeHelper.Hour * 4)
                {
                    jiaYuanComponentServer.JiaYuanFuJins_3.Clear();

                    long mapInstanceId = DBHelper.GetMainCityServerId(unit);
                    M2M_AllPlayerListResponse reqEnter = (M2M_AllPlayerListResponse)await ActorMessageSenderComponent.Instance.Call(mapInstanceId, new M2M_AllPlayerListRequest()
                    {
                    });
                    List<long> allPlayers = new List<long>();
                    if (reqEnter.Error == ErrorCode.ERR_Success)
                    {
                        allPlayers = reqEnter.AllPlayers;
                    }

                    for (int i = allPlayers.Count - 1; i >= 0; i--)
                    {
                        if (allPlayers[i] == unit.Id || allPlayers[i] == request.MasterId)
                        {
                            allPlayers.RemoveAt(i);
                            continue;
                        }
                        if (friendSet.Contains(allPlayers[i]))
                        {
                            allPlayers.RemoveAt(i);
                            continue;
                        }
                    }

                    List<long> destUserinfos = new List<long>();
                    RandomHelper.GetRandListByCount(allPlayers, destUserinfos, Math.Min(allPlayers.Count, 3));
                    jiaYuanComponentServer.JiaYuanFuJinTime_3 = TimeHelper.ServerNow();
                    jiaYuanComponentServer.JiaYuanFuJins_3 = destUserinfos;
                }

                for (int i = 0; i < jiaYuanComponentServer.JiaYuanFuJins_3.Count; i++)
                {
                    JiaYuanVisit jiaYuanVisit = await GetJiaYuanVisit(UnitZoneHelper.GetHomeZone(unit), jiaYuanComponentServer.JiaYuanFuJins_3[i]);
                    if (jiaYuanVisit != null)
                    {
                        response.JiaYuanVisit_2.Add(jiaYuanVisit);
                    }
                }
            }    
            reply();
            await ETTask.CompletedTask;
        }
    }
}
