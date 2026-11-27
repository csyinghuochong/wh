using System;

namespace ET
{
    public class G2M_RequestExitGameHandler : AMActorLocationRpcHandler<Unit, G2M_RequestExitGame, M2G_RequestExitGame>
    {
        protected override async ETTask Run(Unit unit, G2M_RequestExitGame request, M2G_RequestExitGame response, Action reply)
        {

            try
            {
                // 完成玩家落库、摘位置与销毁后才回包，网关停服据此等待退出完成。
                DBSaveComponent dBSaveComponent = unit.GetComponent<DBSaveComponent>();
                if (dBSaveComponent != null)
                {
                    await dBSaveComponent.OnDisconnect();
                }
                else
                {
                    await unit.RemoveLocation();
                    unit.GetParent<UnitComponent>().Remove(unit.Id);
                }
            }
            catch (Exception e) 
            {
                Log.Error(e.ToString());
                response.Error = ErrorCore.ERR_RpcFail;
            }

            reply();
        }
    }

}