using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class M2U_UnionOperationHandler : AMActorRpcHandler<Scene, M2U_UnionOperationRequest, U2M_UnionOperationResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionOperationRequest request, U2M_UnionOperationResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
                if (dBUnionInfo == null)
                {
                    response.Error = ErrorCode.ERR_Union_Not_Exist;
                    reply();
                    return;
                }
               
                switch (request.OperateType)
                {
                    case 1: //增加公会经验
                        // Par = $"{playerName}_{getWay}_{dataType}_{dataValue}"
                        // 玩家名可能含下划线，固定从尾部取 dataType / dataValue，不能按固定下标取。
                        if (!TryParseOperationPar(request.Par, out int dataType, out int dataValue))
                        {
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }

                        var unionInfo = dBUnionInfo.UnionInfo;
                        if (unionInfo.Level == 0)
                        {
                            unionInfo.Level = 1;
                        }

                        if (dataType == UserDataType.UnionExp)
                        {
                            unionInfo.Exp += dataValue;
                            // 一次加的经验可能跨多级；没有下一级配置时经验继续累计，不再升级。
                            while (LDUnionCategory.Instance.Contain(unionInfo.Level)
                                   && LDUnionCategory.Instance.Contain(unionInfo.Level + 1))
                            {
                                LDUnion levelConfig = LDUnionCategory.Instance.Get(unionInfo.Level);
                                if (levelConfig.Exp <= 0 || unionInfo.Exp < levelConfig.Exp)
                                {
                                    break;
                                }

                                unionInfo.Exp -= levelConfig.Exp;
                                unionInfo.Level++;
                            }
                            // 升级奖励邮件暂不发放：LDUnion 配置已无 UpAllReward 字段，原逻辑到升级时必空引用崩溃。
                            // 补上奖励配置后在这里接回 MailHelp.SendUserMail。
                        }
                        else
                        {
                            // 旧工会资金 dataType=35 已废弃；工会资源统一走 OperateType=6 / UnionInfo.ResourceList。
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }

                        unionInfo.ActiveRecord ??= new List<string>();
                        unionInfo.ActiveRecord.Add(request.Par);
                        if (unionInfo.ActiveRecord.Count > MaxActiveRecord)
                        {
                            unionInfo.ActiveRecord.RemoveAt(0);
                        }

                        DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
                        break;
                    case 2:  //获取等级
                        response.Par = dBUnionInfo.UnionInfo.Level.ToString();
                        break;
                    case 6: //工会资源：由 UnionHelper.IsUnionResourceItem 识别后统一走这里写 UnionInfo.ResourceList
                        if (request.Items != null)
                        {
                            for (int i = 0; i < request.Items.Count; i++)
                            {
                                UnionHelper.AddUnionResource(dBUnionInfo.UnionInfo, request.Items[i]);
                            }
                        }

                        DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
                        break;
                    default:
                        // 3/4/5(捐献、增加工会金币)已废弃；未知 OperateType 明确报错，不再静默成功。
                        response.Error = ErrorCode.ERR_ModifyData;
                        break;
                }
            }

            reply();
            await ETTask.CompletedTask;
        }

        private const int MaxActiveRecord = 100;

        /// <summary>
        /// 兼容 "{playerName}_{getWay}_{dataType}_{dataValue}"。玩家名可能带下划线，
        /// 因此只解析尾部两个固定字段；非法 Par 直接拒绝，避免 handler 抛异常导致 RPC 不回包。
        /// </summary>
        private static bool TryParseOperationPar(string par, out int dataType, out int dataValue)
        {
            dataType = 0;
            dataValue = 0;
            if (string.IsNullOrEmpty(par))
            {
                return false;
            }

            string[] parts = par.Split('_');
            return parts.Length >= 4
                   && int.TryParse(parts[parts.Length - 2], out dataType)
                   && int.TryParse(parts[parts.Length - 1], out dataValue);
        }
    }
}
