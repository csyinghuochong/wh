using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SerialReardHandler : AMActorLocationRpcHandler<Unit, C2M_SerialReardRequest, M2C_SerialReardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SerialReardRequest request, M2C_SerialReardResponse response, Action reply)
        {
            if (unit.GetComponent<BagComponent>().GetBagLeftCell() < 5)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.SerialNumber) >= 5)
            {
                response.Error = ErrorCode.ERR_TimesIsNot;
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                long centerid = DBHelper.GetRealmCenter();
                M2R_SerialQueryRequest queryRequest = new M2R_SerialQueryRequest() { SerialNumber = request.SerialNumber };
                R2M_SerialQueryResponse queryResponse = (R2M_SerialQueryResponse)await ActorMessageSenderComponent.Instance.Call
                          (centerid, queryRequest);

                if (queryResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = queryResponse.Error;
                    reply();
                    return;
                }

                int serialIndex = queryResponse.SerialIndex;
                if (serialIndex <= 0)
                {
                    response.Error = ErrorCode.ERR_SerialNoExist;
                    reply();
                    return;
                }
                if (queryResponse.IsRewarded == 1)
                {
                    response.Error = ErrorCode.ERR_AlreadyReceived;
                    reply();
                    return;
                }

               
                if (unit.GetComponent<UserInfoComponent>().UserInfo.SerialRewards.Contains(serialIndex))
                {
                    response.Error = ErrorCode.ERR_AlreadyReceived2;
                    reply();
                    return;
                }

                M2R_SerialReardRequest m2Center_Serial = new M2R_SerialReardRequest() { SerialNumber = request.SerialNumber };
                R2M_SerialReardResponse m2m_TrasferUnitResponse = (R2M_SerialReardResponse)await ActorMessageSenderComponent.Instance.Call
                          (centerid, m2Center_Serial);

                if (m2m_TrasferUnitResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = m2m_TrasferUnitResponse.Error;
                    reply();
                    return;
                }

                string reward = CommonConfig.SerialReward[serialIndex];
                unit.GetComponent<BagComponent>().OnAddItemData(reward, $"{ItemGetWay.Serial}_{TimeHelper.ServerNow()}");
                numericComponent.ApplyChange(null, NumericType.SerialNumber, 1, 0);
                if (serialIndex >= 8 && serialIndex <= 11)
                {
                    unit.GetComponent<UserInfoComponent>().UserInfo.SerialRewards.Add(serialIndex);
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
