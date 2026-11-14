using System;

namespace ET
{
    [ActorMessageHandler]
    public class T2M_TeamUpdateHandler : AMActorLocationHandler<Unit, T2M_TeamUpdateRequest>
    {
        protected override async ETTask Run(Unit unit, T2M_TeamUpdateRequest message)
        {
            Log.Debug($"T2M_TeamUpdate  {unit.Id}  {message.TeamId}");
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ApplyValue(NumericType.TeamId, message.TeamId);
            await DBHelper.SavePlayerComponent(unit.Id, numericComponent);

            long rolePetId = unit.GetComponent<PetComponentServer>().GetFightPetId();
            Unit unitpet = unit.GetParent<UnitComponent>().Get(rolePetId);
            if (unitpet != null)
            {
                unitpet.GetComponent<NumericComponent>().ApplyValue(NumericType.TeamId, message.TeamId);
            }

            // 副本内离队或被移出队伍，回主城；正常退出副本不影响队伍。
            if (message.TeamId == 0 && unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum == MapTypeEnum.TeamDungeon)
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Transfer, unit.Id, 0))
                {
                    if (!unit.IsDisposed && unit.GetTeamId() == 0
                        && unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum == MapTypeEnum.TeamDungeon)
                    {
                        await TransferHelper.MainCityTransfer(unit);
                    }
                }
            }
        }
    }
}
