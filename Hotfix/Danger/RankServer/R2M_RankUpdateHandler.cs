namespace ET
{
    [ActorMessageHandler]
    public class R2M_RankUpdateHandler : AMActorLocationHandler<Unit, R2M_RankUpdateMessage>
    {
        protected override async ETTask Run(Unit unit, R2M_RankUpdateMessage message)
        {
            //Log.Console($"R2M_RankUpdateMessage； {message.RankId} {message.OccRankId}");
            switch (message.RankType)
            {
                case 1:
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.CombatRankID, message.RankId);
                    unit.GetComponent<TaskComponentServer>().OnCombatRank(message.RankId);
                   
                    break;
                case 2:
               
                    break;
                case 4:
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.SoloRankId, message.RankId);
                    break;
                default:
                    break;
            }
            await ETTask.CompletedTask;
        }
    }
}
