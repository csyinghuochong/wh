using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2U_UnionOperatateHandler : AMActorRpcHandler<Scene, C2U_UnionOperatateRequest, U2C_UnionOperatateResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionOperatateRequest request, U2C_UnionOperatateResponse response, Action reply)
        {
            UnionSceneComponent unionSceneComponent = scene.GetComponent<UnionSceneComponent>();
            DBUnionInfo dBUnionInfo = await unionSceneComponent.GetDBUnionInfo(request.UnionId);
            if (dBUnionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                // 1 改名已迁到地图服扣费入口；2 宣言，3 任职，4 对内公告。
                if (request.Operatate == 2 || request.Operatate == 4)
                {
                    UnionPlayerInfo operatorInfo = UnionHelper.GetUnionPlayerInfo(dBUnionInfo.UnionInfo.UnionPlayerList, request.UnitId);
                    if (operatorInfo == null || (operatorInfo.UserID != dBUnionInfo.UnionInfo.LeaderId
                        && operatorInfo.Position != UnionPosition.ViceLeader))
                    {
                        response.Error = ErrorCode.ERR_Union_NoLimits;
                        reply();
                        return;
                    }
                }
                switch (request.Operatate)
                {
                    case 1:
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    case 2:
                    case 4:
                        string lengthKey = request.Operatate == 2
                            ? GlobalValueKey.Global_Union_Declaration_Character_Length
                            : GlobalValueKey.Global_Union_Notic_Character_Length;
                        if (!UnionHelper.IsValidUnionText(request.Value, lengthKey))
                        {
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }
                        if (request.Operatate == 2)
                        {
                            dBUnionInfo.UnionInfo.UnionPurpose = request.Value ?? string.Empty;
                        }
                        else
                        {
                            dBUnionInfo.UnionInfo.UnionNotice = request.Value ?? string.Empty;
                        }
                        break;
                    case 3:
                        string[] operatevalue = (request.Value ?? string.Empty).Split('_');
                        if (operatevalue.Length != 2 || !long.TryParse(operatevalue[0], out long operateid)
                            || !int.TryParse(operatevalue[1], out int position)
                            || (position != UnionPosition.Member && position != UnionPosition.ViceLeader && position != UnionPosition.Elder))
                        {
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }

                        UnionPlayerInfo unionPlayerInfo_1 = UnionHelper.GetUnionPlayerInfo(dBUnionInfo.UnionInfo.UnionPlayerList, request.UnitId);
                        if (unionPlayerInfo_1 == null)
                        {
                            response.Error = ErrorCode.ERR_Union_NoPlayer;
                            reply();
                            return;
                        }
                        if (unionPlayerInfo_1.UserID == dBUnionInfo.UnionInfo.LeaderId)
                        {
                            unionPlayerInfo_1.Position = UnionPosition.Leader;
                        }
                        if (unionPlayerInfo_1.Position == UnionPosition.Member
                            || (unionPlayerInfo_1.Position >= position && position != UnionPosition.Member))
                        {
                            response.Error = ErrorCode.ERR_Union_NoLimits;
                            reply();
                            return;
                        }

                        UnionPlayerInfo unionPlayerInfo_2 = UnionHelper.GetUnionPlayerInfo(dBUnionInfo.UnionInfo.UnionPlayerList, operateid);
                        if (unionPlayerInfo_2 == null)
                        {
                            response.Error = ErrorCode.ERR_Union_NoPlayer;
                            reply();
                            return;
                        }
                        if (unionPlayerInfo_2.Position != UnionPosition.Member && unionPlayerInfo_2.Position <= unionPlayerInfo_1.Position)
                        {
                            response.Error = ErrorCode.ERR_Union_NoLimits;
                            reply();
                            return;
                        }

                        unionPlayerInfo_2.Position = position;
                        break;
                    default:
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                }

                await DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo);
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
