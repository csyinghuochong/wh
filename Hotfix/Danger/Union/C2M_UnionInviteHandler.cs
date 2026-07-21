namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionInviteHandler : AMActorLocationHandler<Unit, C2M_UnionInviteRequest>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionInviteRequest message)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            Unit beinvite = unit.GetParent<UnitComponent>().Get(message.InviteId);

            if (string.IsNullOrEmpty(roleInfo.UnionName))
            {
                return;
            }
            long unionid = numericComponent.GetAsLong( NumericType.UnionId_0 );
            if (unionid == 0)
            {
                return;
            }

            if (beinvite != null)
            {
                if (beinvite.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0) != 0)
                {
                    return;
                }

                MessageHelper.SendToClient(beinvite, new M2C_UnionInviteMessage()
                { 
                    UnionId = unionid,
                    UnionName = roleInfo.UnionName,
                    PlayerName = roleInfo.Name,
                });
            }
            await ETTask.CompletedTask;
        }
    }
}
