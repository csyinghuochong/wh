using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_RoleAddPointHandler : AMActorLocationRpcHandler<Unit, C2M_RoleAddPointRequest, M2C_RoleAddPointResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_RoleAddPointRequest request, M2C_RoleAddPointResponse response, Action reply)
        {
            try
            {
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                int level = roleInfoComponentServer.RoleInfo.Lv;
                if (!RoleAddPointHelper.CanManualAddPoint(level))
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                if (request.PointList == null || request.PointList.Count != RoleAddPointHelper.PointNumericTypes.Length)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                int[] assignedPoints = new int[RoleAddPointHelper.PointNumericTypes.Length];
                for (int i = 0; i < assignedPoints.Length; i++)
                {
                    assignedPoints[i] = request.PointList[i];
                    if (assignedPoints[i] < 0 || assignedPoints[i] > 2000)
                    {
                        Log.Error($"C2M_RoleAddPointRequest: {unit.DomainZone()}  {unit.Id}  {assignedPoints[i]}");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                }

                int remainPoint = RoleAddPointHelper.GetRemainPoint(level, assignedPoints);
                if (remainPoint < 0)
                {
                    Log.Error($"C2M_RoleAddPointRequest 2");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                int[] fixPoints = RoleAddPointHelper.GetFixedPointByLevel(level);
                
                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                for (int i = 0; i < RoleAddPointHelper.PointNumericTypes.Length; i++)
                {
                    int oldvalue = numericComponent.GetAsInt(RoleAddPointHelper.PointNumericTypes[i]);
                    
                    numericComponent.ApplyValue(RoleAddPointHelper.PointNumericTypes[i], assignedPoints[i] - fixPoints[i]);
                }

                numericComponent.ApplyValue(NumericType.PointRemain, remainPoint);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);

                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error("C2M_RoleAddPointError: " + ex.ToString());
            }
        }
    }
}
