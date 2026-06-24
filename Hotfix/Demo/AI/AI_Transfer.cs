using UnityEngine;


namespace ET
{

    [AIHandler]
    public class AI_Transfer : AAIHandler
    {
        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            return aiComponent.TargetPoint.Count == 0;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            await ETTask.CompletedTask;
            Unit unit = aiComponent.GetParent<Unit>();
            unit.Stop(0);
            unit.SetBornPosition(unit.Position, true);
            aiComponent.IsRetreat = 0;
            aiComponent.AIConfigId = int.Parse(ldai.NodeParams);
        }
    }
}
