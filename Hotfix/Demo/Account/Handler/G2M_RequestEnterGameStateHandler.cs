using System;

namespace ET
{
    public class G2M_RequestEnterGameStateHandler : AMActorLocationRpcHandler<Unit, G2M_RequestEnterGameState, M2G_RequestEnterGameState>
    {
        protected override async ETTask Run(Unit unit, G2M_RequestEnterGameState request, M2G_RequestEnterGameState response, Action reply)
        {
            bool replied = false;
            try
            {
                if (request.GateSessionActorId != 0 && request.GateSessionActorId != -1)  //重连
                {
                    unit.GetComponent<DBSaveComponent>().OnRelogin(request.GateSessionActorId);
                }
                if(request.GateSessionActorId == 0)  //重登：先摘掉位置，再销毁 Unit
                {
                    reply();
                    replied = true;
                    await unit.RemoveLocation();
                    unit.GetComponent<DBSaveComponent>().OnDisconnect();
                    return;
                }
                if (request.GateSessionActorId == -1) //查询
                {
                    response.Message = "玩家在线: {unit.Id}";
                }
            }
            catch (Exception ex)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($" {unit.Id}  {ex.ToString()}");
                }
            }
            if (!replied)
            {
                reply();
            }
            await ETTask.CompletedTask; 
        }
    }
}