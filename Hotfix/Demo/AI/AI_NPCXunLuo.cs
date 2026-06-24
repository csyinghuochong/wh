using UnityEngine;

namespace ET
{

    [AIHandler]
    public class AI_NPCXunLuo : AAIHandler
    {

        public override bool Check(AIComponent aiComponent, LDAI ldai)
        {
            return true;
        }

        public override async ETTask Execute(AIComponent aiComponent, LDAI ldai, ETCancellationToken cancellationToken)
        {
            Unit unit = aiComponent.GetParent<Unit>();
            while (true)
            {
                bool timeRet = await TimerComponent.Instance.WaitAsync(5000, cancellationToken);
                if (!timeRet)
                {
                    return;
                }
            }
        }
    }
}
